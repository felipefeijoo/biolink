/****** Object:  StoredProcedure [dbo].[spAssociatesListForTaxon]    Script Date: 08/19/2011 11:17:58 ******/
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER PROCEDURE [dbo].[spAssociatesListForTaxon]
                                          @intPoliticalRegionID int, 
                                          @vchrBiotaID varchar(4000)
AS
-- return a list of associates for the taxa passed
BEGIN
SET NOCOUNT ON
DECLARE @RegionParentage varchar(8000)
DECLARE @vchrParentage varchar(8000)
DECLARE @intBiotaID int

	-- Prepare the parentage for the political region chosen
	SELECT @RegionParentage = vchrParentage
	FROM tblPoliticalRegion
	WHERE intPoliticalRegionID = @intPoliticalRegionID
	SELECT @RegionParentage = ISNULL(@RegionParentage, '\%')

	-- get our list of taxa into a table so we can join rather than 'IN'
	CREATE TABLE #ttblTaxa (intBiotaID int, vchrParentage varchar(255))
	INSERT INTO #ttblTaxa EXEC ('SELECT intBiotaID, vchrParentage + ''\%'' FROM tblBiota WHERE intBiotaID IN (' +@vchrBiotaID + ')')

	-- create the table to hold the results
	CREATE TABLE #ttblTemp (
		BiotaID int,
		BiotaFullName varchar(500),
		AssociateName varchar(255),
		Relationship varchar(50),
		RegionID int, 
		AssociateID int
	)
	
	CREATE TABLE #ttblAssocView (
		intAssociateID int,
		FromBiotaID int,	     
		FromBiotaName varchar(500),
		FromTo varchar(50),
		ToBiotaID int,
        ToDescription varchar(500),
        ToFrom varchar(500),
        intPoliticalRegionID int
	)
	
DECLARE @intTaxonCatID int
DECLARE @intMaterialCatID int

SELECT @intTaxonCatID = intTraitCategoryID /* Used to be 2 */
FROM tblTraitCategory
WHERE vchrCategory = 'Taxon'

SELECT @intMaterialCatID = intTraitCategoryID /* Used to be 1 */
FROM tblTraitCategory
WHERE vchrCategory = 'Material'

INSERT INTO #ttblAssocView
SELECT     A.intAssociateID, CASE A.intFromCatID WHEN @intMaterialCatID THEN MF.intBiotaID WHEN @intTaxonCatID THEN A.intFromIntracatID END AS FromBiotaID, 
                      CASE A.intFromCatID WHEN @intMaterialCatID THEN MBF.vchrFullname WHEN @intTaxonCatID THEN BF.vchrFullName END AS FromBiotaName, 
                      A.vchrRelationFromTo AS FromTo, 
                      CASE A.intToCatID WHEN @intMaterialCatID THEN MF.intBiotaID WHEN @intTaxonCatID THEN A.intToIntraCatID END AS ToBiotaID, 
                      CASE A.intToCatID WHEN @intMaterialCatID THEN MBT.vchrFullname WHEN @intTaxonCatID THEN BT.vchrFullname ELSE CONVERT(varchar(1000), 
                      A.txtAssocDescription) END AS ToDescription, 
                      A.vchrRelationToFrom AS ToFrom, ISNULL(A.intPoliticalRegionID,0)
FROM         dbo.tblAssociate AS A LEFT OUTER JOIN
                      dbo.tblBiota AS BF ON A.intFromIntraCatID = BF.intBiotaID AND A.intFromCatID = @intTaxonCatID LEFT OUTER JOIN
                      dbo.tblMaterial AS MF ON A.intFromIntraCatID = MF.intMaterialID AND A.intFromCatID = @intMaterialCatID LEFT OUTER JOIN
                      dbo.tblBiota AS MBF ON MF.intBiotaID = MBF.intBiotaID LEFT OUTER JOIN
                      dbo.tblBiota AS BT ON A.intToIntraCatID = BT.intBiotaID AND A.intToCatID = @intTaxonCatID LEFT OUTER JOIN
                      dbo.tblMaterial AS MT ON A.intToIntraCatID = MT.intMaterialID AND A.intToCatID = @intMaterialCatID LEFT OUTER JOIN
                      dbo.tblBiota AS MBT ON MT.intBiotaID = MBT.intBiotaID	
	

	-- Extract the parentage of the taxa specified.
	DECLARE temp_cursor_parentage CURSOR
	FOR SELECT intBiotaID, vchrParentage FROM #ttblTaxa
	FOR READ ONLY
	
	OPEN temp_cursor_parentage	
	FETCH NEXT FROM temp_cursor_parentage INTO @intBiotaID, @vchrParentage
	
	WHILE @@FETCH_STATUS = 0
	BEGIN

		INSERT INTO #ttblTemp (BiotaID, BiotaFullName, AssociateName, Relationship, RegionID, AssociateID)

		SELECT B1.intBiotaID, B1.vchrFullname,  
			V1.ToDescription, IsNull(V1.FromTo, '?') + '->' + isNull(V1.ToFrom, '?'),
			V1.intPoliticalRegionID, V1.intAssociateID
		FROM tblBiota B1
			INNER JOIN #ttblAssocView V1 ON B1.intBiotaID = V1.FromBiotaID
			INNER JOIN tblAssociate A1 ON V1.intAssociateID = A1.intAssociateID
			LEFT OUTER JOIN tblPoliticalRegion PR1 ON (V1.intPoliticalRegionID = PR1.intPoliticalRegionID)
		WHERE  IsNull(V1.ToDescription, '') <> '' 
		and B1.intBiotaId in (select intBiotaId from tblBiota where vchrParentage like @vchrParentage or intBiotaId = @intBiotaId)
		and (V1.intPoliticalRegionID = 0 or (PR1.vchrParentage LIKE @RegionParentage))

		UNION
		SELECT B2.intBiotaID, B2.vchrFullname,  
			V2.FromBiotaName, IsNull(V2.ToFrom, '?') + '->' + IsNull(V2.FromTo, '?') , 
			V2.intPoliticalRegionID, V2.intAssociateID
		FROM tblBiota B2 
			INNER JOIN #ttblAssocView V2 ON B2.intBiotaID = V2.ToBiotaID
			INNER JOIN tblAssociate A2 ON V2.intAssociateID = A2.intAssociateID
			LEFT OUTER JOIN tblPoliticalRegion PR2 ON (V2.intPoliticalRegionID = PR2.intPoliticalRegionID)
		WHERE IsNull(V2.FromBiotaName, '') <> ''
		and B2.intBiotaId in (select intBiotaId from tblBiota where vchrParentage like @vchrParentage or intBiotaId = @intBiotaId)
		and (V2.intPoliticalRegionID = 0 or (PR2.vchrParentage LIKE @RegionParentage))

		FETCH NEXT FROM temp_cursor_parentage INTO @intBiotaID, @vchrParentage

	END

	CLOSE temp_cursor_parentage
	DEALLOCATE temp_cursor_parentage

	-- return the details
	SELECT T.*,
		dbo.PoliticalRegionFullPath(T.RegionID, ': ') AS [FullRegion], 
		A.vchrSource AS [Source], R.vchrRefCode AS [RefCode], A.vchrRefPage AS [RefPage], A.txtNotes AS [Notes]
	FROM #ttblTemp T
		INNER JOIN tblAssociate A ON T.AssociateID = A.intAssociateID
		LEFT OUTER JOIN tblReference R ON (A.intRefID = R.intRefID)

	DROP TABLE #ttblTaxa
	DROP TABLE #ttblTemp
	DROP TABLE #ttblAssocView

END