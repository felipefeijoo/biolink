/*******************************************************************************
* Table 'tblBIOTA'                                                             *
*******************************************************************************/

print 'Modifying tblBIOTA...'

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[trgIUBiotaParentNames]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
drop trigger [dbo].[trgIUBiotaParentNames]
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentKingdom')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentKingdom] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentPhylum')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentPhylum] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentClass')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentClass] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentOrder')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentOrder] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentFamily')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentFamily] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentGenus')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentGenus] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentSpecies')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentSpecies] varchar(200)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblBIOTA]') and [NAME] = N'vchrParentSubspecies')
    ALTER TABLE [tblBIOTA]
        ADD [vchrParentSubspecies] varchar(200)

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE TRIGGER trgIUBiotaParentNames ON dbo.tblBiota 
FOR INSERT,UPDATE
AS
BEGIN
	SET NOCOUNT ON  /* Generally hide the row count when the trigger is recursed during the update of parentage */
    
	DECLARE @vchrParentage varchar(900)

	-- Update the parent classification columns Kingdom, Class, Order, Family, Genus, Species and Subspecies if the name or placement (parent) changes
	IF UPDATE(vchrEpithet) OR UPDATE(intParentID)
	BEGIN
		DECLARE temp_cursor CURSOR LOCAL FAST_FORWARD
			FOR 
			SELECT vchrParentage
			FROM inserted
		OPEN temp_cursor
		FETCH NEXT FROM temp_cursor INTO @vchrParentage
		WHILE @@FETCH_STATUS = 0
		BEGIN
			UPDATE tblBiota
				SET vchrParentKingdom = dbo.udfParentTaxonNameAtRank(intBiotaID,'KING'),
					vchrParentPhylum = dbo.udfParentTaxonNameAtRank(intBiotaID,'P'),
					vchrParentClass = dbo.udfParentTaxonNameAtRank(intBiotaID,'C'),
					vchrParentOrder = dbo.udfParentTaxonNameAtRank(intBiotaID,'O'),
					vchrParentFamily = dbo.udfParentTaxonNameAtRank(intBiotaID,'F'),
					vchrParentGenus = dbo.udfParentTaxonNameAtRank(intBiotaID,'G'),
					vchrParentSpecies = dbo.udfParentTaxonNameAtRank(intBiotaID,'SP'),
					vchrParentSubspecies = dbo.udfParentTaxonNameAtRank(intBiotaID,'SSP')
				WHERE vchrParentage LIKE @vchrParentage + '%'
			FETCH NEXT FROM temp_cursor INTO @vchrParentage
		END
		CLOSE temp_cursor
		DEALLOCATE temp_cursor
	END
END

GO

/*******************************************************************************
* Table 'tblPoliticalRegion'                                                   *
*******************************************************************************/

print 'Modifying tblPoliticalRegion...'

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[trgIUPoliticalRegionParentName]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
drop trigger [dbo].[trgIUPoliticalRegionParentName]
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblPoliticalRegion]') and [NAME] = N'vchrParentCountry')
    ALTER TABLE [tblPoliticalRegion]
        ADD [vchrParentCountry] varchar(50)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblPoliticalRegion]') and [NAME] = N'vchrParentPrimDiv')
    ALTER TABLE [tblPoliticalRegion]
        ADD [vchrParentPrimDiv] varchar(50)
IF NOT EXISTS (SELECT * FROM syscolumns WHERE [id] = OBJECT_ID(N'[tblPoliticalRegion]') and [NAME] = N'vchrParentSecDiv')
    ALTER TABLE [tblPoliticalRegion]
        ADD [vchrParentSecDiv] varchar(50)

SET QUOTED_IDENTIFIER ON 
GO
SET ANSI_NULLS ON 
GO

CREATE TRIGGER trgIUPoliticalRegionParentName ON dbo.tblPoliticalRegion
FOR INSERT,UPDATE
AS
BEGIN

BEGIN TRANSACTION

SET NOCOUNT ON  /* Generally hide the row count when the trigger is recursed during the update of parentage */

DECLARE @vchrParentage varchar(900)

	-- Update the parent item columns Country, PrimDiv and SecDiv
	IF UPDATE(vchrName)
	BEGIN
		DECLARE temp_cursor CURSOR LOCAL FAST_FORWARD
			FOR 
			SELECT vchrParentage
			FROM inserted
		OPEN temp_cursor
		FETCH NEXT FROM temp_cursor INTO @vchrParentage
		WHILE @@FETCH_STATUS = 0
		BEGIN
			UPDATE tblPoliticalRegion
				SET vchrParentCountry = dbo.udfParentRegionNameAtRank(intPoliticalRegionID,'Country'),
					vchrParentPrimDiv = dbo.udfParentRegionNameAtRank(intPoliticalRegionID,'PrimDiv'),
					vchrParentSecDiv = dbo.udfParentRegionNameAtRank(intPoliticalRegionID,'County')
				WHERE vchrParentage LIKE @vchrParentage + '%'
			FETCH NEXT FROM temp_cursor INTO @vchrParentage
		END
		CLOSE temp_cursor
		DEALLOCATE temp_cursor
	END   
    COMMIT TRANSACTION
END

GO

/*******************************************************************************
* User-defined Functions                                                       *
*******************************************************************************/

print 'Adding User-defined Functions...'

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[udfBLDateToDayOfYear]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[udfBLDateToDayOfYear]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[udfParentRegionNameAtRank]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[udfParentRegionNameAtRank]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[udfParentTaxonNameAtRank]') and xtype in (N'FN', N'IF', N'TF'))
drop function [dbo].[udfParentTaxonNameAtRank]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE FUNCTION udfBLDateToDayOfYear (@intBLDate INT)  
		RETURNS INT
AS  

/* 
Returns the number of days from 1 January for the given BioLink date..
Returns 0 if no month given.  Assumes day = 15 if no day given..
*/

BEGIN 
	DECLARE @intDayOfYear INT
	DECLARE @intBLMonth INT
	DECLARE @intBLDay INT
	SET @intDayOfYear = 0

	IF LEN(@intBLDate) < 8
		RETURN 0

	SET @intBLMonth = SUBSTRING(CAST(@intBLDate AS VARCHAR),5,2)
	SET @intBLDay = RIGHT(@intBLDate,2)
	
	IF @intBLMonth = 0
		RETURN 0

	IF @intBLMonth = 2
		SET @intDayOfYear =31
	ELSE IF @intBLMonth = 3
		SET @intDayOfYear = 59
	ELSE IF @intBLMonth = 4
		SET @intDayOfYear = 90
	ELSE IF @intBLMonth = 5
		SET @intDayOfYear = 120
	ELSE IF @intBLMonth = 6
		SET @intDayOfYear = 151
	ELSE IF @intBLMonth = 7
		SET @intDayOfYear = 181
	ELSE IF @intBLMonth = 8
		SET @intDayOfYear = 212
	ELSE IF @intBLMonth = 9
		SET @intDayOfYear = 243
	ELSE IF @intBLMonth = 10
		SET @intDayOfYear = 273
	ELSE IF @intBLMonth = 11
		SET @intDayOfYear = 304
	ELSE IF @intBLMonth = 12
		SET @intDayOfYear = 334

	IF @intBLDay = 0
		SET @intDayOfYear = @intDayOfYear + 15
	ELSE	
		SET @intDayOfYear = @intDayOfYear + @intBLDay	

	RETURN @intDayOfYear
END


GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE FUNCTION udfParentRegionNameAtRank (@RegionID INT, @chrRank varchar(40))  
		RETURNS varchar (255)
AS

/* 
Returns the name of the parent of the given RegionID at the specified rank or type.
If rank/type not found returns Null
Assume PrimDiv can be either a State, Territory or Province.
*/

BEGIN 
	DECLARE @vchrSourceParentage varchar(8000)
	DECLARE @vchrParentName varchar(255)
	DECLARE @vchrCurrentNumber varchar(15)
	SET @vchrParentName = Null

	/* Get the parentage of the requested region */
	SELECT @vchrSourceParentage = vchrParentage
		FROM tblPoliticalRegion
		WHERE intPoliticalRegionID = @RegionID

	/* Strip the first \ and add one to the end */
	SET @vchrSourceParentage = RIGHT(@vchrSourceParentage,LEN(@vchrSourceParentage) - 1) + '\'

	/* Check each region in the parentage (by extracting its intPoliticalRegionID from the parentage string) 
		and see if its the requested rank.  If it is, exit the WHILE loop and return */
	WHILE CHARINDEX('\',@vchrSourceParentage) > 0
	BEGIN
		SET @vchrCurrentNumber = LEFT(@vchrSourceParentage,CHARINDEX('\',@vchrSourceParentage) - 1)

		IF UPPER(@chrRank) = 'PRIMDIV'
			SELECT @vchrParentName = vchrName 
				FROM tblPoliticalRegion
				WHERE vchrRank IN ('State', 'Territory', 'Province') AND intPoliticalRegionID = @vchrCurrentNumber
		ELSE
			SELECT @vchrParentName = vchrName 
				FROM tblPoliticalRegion
				WHERE vchrRank = @chrRank AND intPoliticalRegionID = @vchrCurrentNumber
	
		IF @@ROWCOUNT <> 0 
			BREAK
	
		SET @vchrSourceParentage = RIGHT(@vchrSourceParentage,LEN(@vchrSourceParentage) - LEN(@vchrCurrentNumber) - 1)
	END

	RETURN @vchrParentName
END

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS OFF 
GO

CREATE FUNCTION udfParentTaxonNameAtRank (@BiotaID INT, @chrElemType varchar(10))  
		RETURNS varchar (255)
AS  
/* 
Returns the name of the parent of the given BiotaID at the specified rank.
If rank not found returns Null
*/
BEGIN 
	DECLARE @vchrSourceParentage varchar(8000)
	DECLARE @vchrParentName varchar(255)
	DECLARE @vchrCurrentNumber varchar(15)
	SET @vchrParentName = Null

	/* Get the parentage of the requested biota */
	SELECT @vchrSourceParentage = vchrParentage
		FROM tblBiota
		WHERE intBiotaID = @BiotaID

	/* Strip the first \ and add one to the end */
	SET @vchrSourceParentage = RIGHT(@vchrSourceParentage,LEN(@vchrSourceParentage) - 1) + '\'

	/* Check each biota in the parentage (by extracting its intBiotaID from the parentage string) 
		and see if its the requested rank (as chrElemType).  If it is, exit the WHILE loop and return */
	WHILE CHARINDEX('\',@vchrSourceParentage) > 0
	BEGIN
		SET @vchrCurrentNumber = LEFT(@vchrSourceParentage,CHARINDEX('\',@vchrSourceParentage) - 1)
		SELECT @vchrParentName = vchrEpithet 
			FROM tblBiota
			WHERE chrElemType = @chrElemType AND intBiotaID = @vchrCurrentNumber
	
		IF @@ROWCOUNT <> 0 
			BREAK
	
		SET @vchrSourceParentage = RIGHT(@vchrSourceParentage,LEN(@vchrSourceParentage) - LEN(@vchrCurrentNumber) - 1)
	END

	RETURN @vchrParentName
END
GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

/*******************************************************************************
* Adding Views                                                                 *
*******************************************************************************/

print 'Adding Views...'

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vwAPPDV2]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vwAPPDV2]
GO

if exists (select * from dbo.sysobjects where id = object_id(N'[dbo].[vwDarwinCoreV2]') and OBJECTPROPERTY(id, N'IsView') = 1)
drop view [dbo].[vwDarwinCoreV2]
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE VIEW dbo.vwAPPDV2
AS
-- Forward associates where From part is Taxon and To part is Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	BA.vchrParentOrder AS PestOrder, BA.vchrParentFamily AS PestFamily, BA.vchrParentGenus AS PestGenus,
	BA.vchrParentSpecies AS PestSpecies, BA.vchrParentSubspecies AS PestInfraTaxa, BA.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	B.vchrParentFamily AS HostFamily, B.vchrParentGenus AS HostGenus,
	B.vchrParentSpecies AS HostSpecies, B.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	INNER JOIN tblBiota B ON A.intFromIntraCatID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intToIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxon'))
UNION
-- Reverse associates where From part is Taxon and To part is Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	B.vchrParentOrder AS PestOrder, B.vchrParentFamily AS PestFamily, B.vchrParentGenus AS PestGenus,
	B.vchrParentSpecies AS PestSpecies, B.vchrParentSubspecies AS PestInfraTaxa, B.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	BA.vchrParentFamily AS HostFamily, BA.vchrParentGenus AS HostGenus,
	BA.vchrParentSpecies AS HostSpecies, BA.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	INNER JOIN tblBiota B ON A.intFromIntraCatID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intToIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxon'))
UNION
-- Forward associates where To part is Taxon and From part is Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	BA.vchrParentOrder AS PestOrder, BA.vchrParentFamily AS PestFamily, BA.vchrParentGenus AS PestGenus,
	BA.vchrParentSpecies AS PestSpecies, BA.vchrParentSubspecies AS PestInfraTaxa, BA.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	B.vchrParentFamily AS HostFamily, B.vchrParentGenus AS HostGenus,
	B.vchrParentSpecies AS HostSpecies, B.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	INNER JOIN tblBiota B ON A.intToIntraCatID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intFromIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxon'))
UNION
-- Reverse associates where To part is Taxon and From part is Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	B.vchrParentOrder AS PestOrder, B.vchrParentFamily AS PestFamily, B.vchrParentGenus AS PestGenus,
	B.vchrParentSpecies AS PestSpecies, B.vchrParentSubspecies AS PestInfraTaxa, B.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	BA.vchrParentFamily AS HostFamily, BA.vchrParentGenus AS HostGenus,
	BA.vchrParentSpecies AS HostSpecies, BA.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	INNER JOIN tblBiota B ON A.intToIntraCatID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intFromIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxon'))
UNION
-- Forward associates where both parts are Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	BA.vchrParentOrder AS PestOrder, BA.vchrParentFamily AS PestFamily, BA.vchrParentGenus AS PestGenus,
	BA.vchrParentSpecies AS PestSpecies, BA.vchrParentSubspecies AS PestInfraTaxa, BA.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	B.vchrParentFamily AS HostFamily, B.vchrParentGenus AS HostGenus,
	B.vchrParentSpecies AS HostSpecies, B.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	LEFT OUTER JOIN tblMaterial MA ON A.intFromIntraCatID = MA.intMaterialID 
	INNER JOIN tblBiota B ON MA.intBiotaID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intToIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material'))
UNION
--Reverse associates where both parts are Material
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	BA.vchrParentOrder AS PestOrder, BA.vchrParentFamily AS PestFamily, BA.vchrParentGenus AS PestGenus,
	BA.vchrParentSpecies AS PestSpecies, BA.vchrParentSubspecies AS PestInfraTaxa, BA.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	B.vchrParentFamily AS HostFamily, B.vchrParentGenus AS HostGenus,
	B.vchrParentSpecies AS HostSpecies, B.vchrParentSubspecies AS HostInfraTaxa, CNA.vchrCommonName AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblAssociate A 
	LEFT OUTER JOIN tblMaterial MA ON A.intToIntraCatID = MA.intMaterialID 
	INNER JOIN tblBiota B ON MA.intBiotaID = B.intBiotaID 
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON A.intFromIntraCatID = M.intMaterialID 
	LEFT OUTER JOIN tblBiota BA ON M.intBiotaID = BA.intBiotaID 
	LEFT OUTER JOIN tblCommonName CNA ON BA.intBiotaID = CNA.intBiotaID
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE (A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')) 
	AND (A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material'))
UNION
-- Records without Associates
SELECT 
	M.vchrAccessionNo AS AccessionNo, 
	B.vchrParentOrder AS PestOrder, B.vchrParentFamily AS PestFamily, B.vchrParentGenus AS PestGenus,
	B.vchrParentSpecies AS PestSpecies, B.vchrParentSubspecies AS PestInfraTaxa, B.vchrAuthor AS PestAuthority,
	CN.vchrCommonName AS PestCommonName, 
	'Null' AS HostFamily, 'Null' AS HostGenus,
	'Null' AS HostSpecies, 'Null' AS HostInfraTaxa, 'Null' AS HostCommonName, M.vchrMicroHabitat AS hostSubStrate, 
	S.vchrLocal AS LocationTown, PR.vchrParentPrimDiv AS LocationState, PR.vchrParentCountry AS LocationCountry, 
	S.fltPosY1 AS Latitude, S.fltPosX1 AS Longitude, M.vchrCollectionMethod AS CollectionMethod, 
	SV.intDateStart AS CollectionDate, LEFT(SV.intDateStart,4) AS CollectionDateYear,
	SV.vchrCollector AS CollectorName, 'Null' AS SpecimenIdentifier, M.vchrIDMethod AS IdentificationMethod,
	'Null' AS Symptom, 'Null' AS Stage, 'Null' AS HostDamage, 'Null' AS QualityIndicator, 'Null' AS Traits
FROM tblBiota B
	LEFT OUTER JOIN tblCommonName CN ON B.intBiotaID = CN.intBiotaID
	LEFT OUTER JOIN tblMaterial M ON B.intBiotaID = M.intBiotaID 
	LEFT OUTER JOIN tblSiteVisit SV ON M.intSiteVisitID = SV.intSiteVisitID 
	LEFT OUTER JOIN tblSite S ON SV.intSiteID = S.intSiteID
	INNER JOIN tblPoliticalRegion PR ON S.intPoliticalRegionID = PR.intPoliticalRegionID
WHERE B.intBiotaID NOT IN (SELECT A.intToIntraCatID FROM tblAssociate A WHERE A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxa'))
	AND B.intBiotaID NOT IN (SELECT A.intFromIntraCatID FROM tblAssociate A WHERE A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Taxa'))
	AND B.intBiotaID NOT IN (SELECT MA.intBiotaID FROM tblMaterial M 
		INNER JOIN tblAssociate A ON M.intMaterialID = A.intToIntraCatID AND A.intToCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')
		INNER JOIN tblMaterial MA ON MA.intMaterialID = A.intFromIntraCatID)
	AND B.intBiotaID NOT IN (SELECT MA.intBiotaID FROM tblMaterial M 
		INNER JOIN tblAssociate A ON M.intMaterialID = A.intFromIntraCatID AND A.intFromCatID = (SELECT intTraitCategoryID FROM tblTraitCategory WHERE vchrCategory = 'Material')
		INNER JOIN tblMaterial MA ON MA.intMaterialID = A.intToIntraCatID)

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

CREATE VIEW dbo.vwDarwinCoreV2
AS
SELECT CONVERT(datetime, GETDATE(),126) AS DateLastModified,
	'ANIC' AS InstitutionCode, 
	'Insects' AS CollectionCode, 
	dbo.tblMaterial.vchrAccessionNo AS CatalogNumber, 
	dbo.tblBiota.vchrFullName AS ScientificName, 
	'S' AS BasisOfRecord,
	dbo.tblBiota.vchrParentKingdom AS Kingdom, 
	dbo.tblBiota.vchrParentPhylum AS Phylum, 
	dbo.tblBiota.vchrParentClass AS Class, 
	dbo.tblBiota.vchrParentOrder AS tOrder, 
	dbo.tblBiota.vchrParentFamily AS Family, 
	dbo.tblBiota.vchrParentGenus AS Genus, 
	dbo.tblBiota.vchrParentSpecies AS Species, 
	dbo.tblBiota.vchrParentSubspecies AS Subspecies,
	dbo.tblBiota.vchrAuthor AS ScientificNameAuthor,
	dbo.tblMaterial.vchrIDBy AS IdentifiedBy,
	YEAR(dbo.tblMaterial.dtIDDate) AS YearIdentified, 
	MONTH(dbo.tblMaterial.dtIDDate) AS MonthIdentified, 
	DAY(dbo.tblMaterial.dtIDDate) AS DayIdentified, 
	Null AS TypeStatus,
	dbo.tblMaterial.vchrCollectorNo AS CollectorNumber,
	dbo.tblSiteVisit.vchrFieldNumber AS FieldNumber, 
	dbo.tblSiteVisit.vchrCollector AS Collector, 
	LEFT(dbo.tblSiteVisit.intDateStart,4) AS YearCollected, 
	SUBSTRING(CAST(dbo.tblSiteVisit.intDateStart AS CHAR(8)),5, 2) AS MonthCollected, 
	RIGHT(dbo.tblSiteVisit.intDateStart,2) AS DayCollected,
	dbo.udfBLDateToDayOfYear (dbo.tblSiteVisit.intDateStart) AS JulianDay,
	dbo.tblSiteVisit.intTimeStart AS TimeOfDay,
	Null AS ContinentOcean,
	dbo.tblPoliticalRegion.vchrParentCountry AS Country, 
	dbo.tblPoliticalRegion.vchrParentPrimDiv AS StateProvince,
	dbo.tblPoliticalRegion.vchrParentSecDiv AS County,
	dbo.tblSite.vchrLocal AS Locality, 
	dbo.tblSite.fltPosX1 AS Longitude, 
	dbo.tblSite.fltPosY1 AS Latitude, 
	dbo.tblSite.vchrPosError AS CoordinatePrecision, 
	Null AS Bbounding,
	dbo.tblSite.fltElevLower + dbo.tblSite.vchrElevUnits AS MinimumElevation,
	dbo.tblSite.fltElevUpper + dbo.tblSite.vchrElevUnits AS MaximumElevation,
	dbo.tblSite.fltElevDepth + dbo.tblSite.vchrElevUnits AS MinimumDepth,
	Null AS MaximumDepth,
	dbo.tblMaterialPart.vchrGender AS Sex,
	dbo.tblMaterialPart.vchrStorageMethod AS PreparationType,
	dbo.tblMaterialPart.intNoSpecimens AS IndividualCount,
	Null AS PreviousCatalogNumber,
	Null AS RelationshipType,
	Null AS RelatedCatalogItem,
	dbo.tblMaterialPart.txtNotes AS Notes
FROM dbo.tblBiota 
	INNER JOIN dbo.tblMaterial ON dbo.tblBiota.intBiotaID = dbo.tblMaterial.intBiotaID 
	INNER JOIN dbo.tblMaterialPart ON dbo.tblMaterial.intMaterialID = dbo.tblMaterialPart.intMaterialID
	INNER JOIN dbo.tblSiteVisit ON dbo.tblMaterial.intSiteVisitID = dbo.tblSiteVisit.intSiteVisitID 
	INNER JOIN dbo.tblSite ON dbo.tblSiteVisit.intSiteID = dbo.tblSite.intSiteID 
	INNER JOIN dbo.tblPoliticalRegion ON dbo.tblSite.intPoliticalRegionID = dbo.tblPoliticalRegion.intPoliticalRegionID
WHERE dbo.tblBiota.bitUnverified = 0 
	AND dbo.tblBiota.bitAvailableName = 0 
	AND dbo.tblBiota.bitLiteratureName = 0

GO
SET QUOTED_IDENTIFIER OFF 
GO
SET ANSI_NULLS ON 
GO

/*******************************************************************************
* Updating table data                                                          *
*******************************************************************************/

print 'Updating data in tblBIOTA, this will take a few minutes...'

UPDATE tblBiota SET vchrEpithet = vchrEpithet WHERE intParentID = 0

print 'Updating data in tblPOLITICALREGION, this will also take a few minutes...'

UPDATE tblPoliticalRegion SET vchrName = vchrName WHERE intParentID = 0

print 'Upgrade complete.'
