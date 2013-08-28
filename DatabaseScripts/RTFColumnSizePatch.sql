alter table tblMaterial
alter column vchrIDNotes text

alter table tblSite
alter column vchrInformalLocal text

alter table tblSite
alter column vchrGeoNotes text

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER          PROCEDURE [dbo].[spMaterialUpdate] 
                                            @intMaterialID int,
                                            @vchrMaterialName varchar(255),
                                            @intSiteVisitID int,
                                            @vchrAccessionNo varchar(50),
                                            @vchrRegNo varchar(50),
                                            @vchrCollectorNo varchar(50),
                                            @intBiotaID int,
                                            @vchrIDBy varchar(50),
                                            @vchrIDDate varchar(50), 
                                            @intIDRefID int,
                                            @vchrIDRefPage varchar(100),
                                            @vchrIDMethod varchar(255),
                                            @vchrIDAccuracy varchar(50),
                                            @vchrIDNameQual varchar(255),
                                            @vchrIDNotes text,
                                            @vchrInstitution varchar(100), 
                                            @vchrCollectionMethod varchar(50), 
                                            @vchrAbundance varchar(255),
                                            @vchrMacroHabitat varchar (255), 
                                            @vchrMicroHabitat varchar(255), 
                                            @vchrSource varchar(50),
                                            @intAssociateOf int,
                                            @intTrapID int,
                                            @vchrSpecialLabel varchar(500),
                                            @vchrOriginalLabel varchar(500)
AS

set nocount on


    DECLARE @dtIDDate datetime
    IF @vchrIDDate = '' 
        SET @dtIDDate = Null
    ELSE
        SET @dtIDDate = CAST(@vchrIDDate as datetime)

        UPDATE tblMaterial 
            SET intSiteVisitID = @intSiteVisitID, 
                    vchrMaterialName = @vchrMaterialName,
                    vchrAccessionNo = @vchrAccessionNo, 
                    vchrRegNo = @vchrRegNo, 
                    vchrCollectorNo = @vchrCollectorNo, 
                     intBiotaID = @intBiotaID, 
                     vchrIDBy = @vchrIDBy,
                     dtIDDate = @dtIDDate, 
                     intIDRefID = @intIDRefID, 
                     vchrIDRefPage = @vchrIDRefPage, 
                     vchrIDMethod  = @vchrIDMethod, 
                     vchrIDAccuracy = @vchrIDAccuracy, 
                     vchrIDNameQual = @vchrIDNameQual, 
                     vchrIDNotes = @vchrIDNotes,
                     vchrInstitution = @vchrInstitution, 
                     vchrCollectionMethod = @vchrCollectionMethod, 
                     vchrAbundance = @vchrAbundance, 
                     vchrMacroHabitat = @vchrMacroHabitat, 
                     vchrMicroHabitat = @vchrMicroHabitat, 
                     vchrSource = @vchrSource,
                     intAssociateOf = @intAssociateOf, 
                     intTrapID = @intTrapID,
                     vchrOriginalLabel = @vchrOriginalLabel, 
                     vchrSpecialLabel = @vchrSpecialLabel, 
                     dtDateLastUpdated = CAST(GetDate() AS datetime),
                     vchrWhoLastUpdated = SYSTEM_USER
        WHERE intMaterialID = @intMaterialID;


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER     PROCEDURE [dbo].[spSiteUpdate]
                                         @intSiteID int, 
                                         @vchrSiteName varchar(100),
                                         @intPoliticalRegionID int, 
                                         @tintLocalType tinyint,
                                         @vchrLocal varchar  (255),
                                         @vchrDistanceFromPlace varchar  (50),
                                         @vchrDirFromPlace varchar  (10),
                                         @vchrInformalLocal text,
                                         @tintPosCoordinates tinyint,
                                         @tintPosAreaType tinyint,
                                         @fltPosX1 float,
                                         @fltPosY1 float,
                                         @fltPosX2 float,
                                         @fltPosY2 float,
                                         @tintPosXYDisplayFormat tinyint,
                                         @vchrPosSource varchar  (50),
                                         @vchrPosError varchar  (20),
                                         @vchrPosWho varchar  (20),
                                         @vchrPosDate varchar  (20),
                                         @vchrPosOriginal varchar  (20),
                                         @vchrPosUTMSource varchar  (255),
                                         @vchrPosUTMMapProj varchar  (255),
                                         @vchrPosUTMMapName varchar  (255),
                                         @vchrPosUTMMapVer varchar  (255),
                                         @tintElevType tinyint,
                                         @fltElevUpper float,
                                         @fltElevLower float,
                                         @fltElevDepth float,
                                         @vchrElevUnits varchar  (20),
                                         @vchrElevSource varchar  (50),
                                         @vchrElevError varchar  (20),
                                         @vchrGeoEra varchar  (50),
                                         @vchrGeoState varchar  (50),
                                         @vchrGeoPlate varchar  (50),
                                         @vchrGeoFormation varchar  (50),
                                         @vchrGeoMember varchar  (50),
                                         @vchrGeoBed varchar  (50),
                                         @vchrGeoName varchar  (50),
                                         @vchrGeoAgeBottom varchar  (50),
                                         @vchrGeoAgeTop varchar  (50),
                                         @vchrGeoNotes text
AS

set nocount on

/*
**  Update the details of the site passed.
*/

DECLARE @intNewSiteGroupID int
DECLARE @intCurrPoliticalRegion int


	-- NJF 10 July 2001; Below added to ensure that when the political region of a  site changes, it 
	-- clears the site group id (if any). If the site group is not cleared, the site is incorrectly listed under 
	-- the old group (in another region) rather than the correct region

	-- Collect the information needed top determine if we have to clear the site group details when the
	-- politcal region changes.
	SELECT @intNewSiteGroupID = intSiteGroupID, @intCurrPoliticalRegion = intPoliticalRegionID
	FROM tblSite
	WHERE intSiteID = @intSiteID

	-- If the political region has changed, then clear the site group.
	IF @intCurrPoliticalRegion <> @intPoliticalRegionID
	BEGIN
		SET @intNewSiteGroupID = Null
	END

    UPDATE tblSite
    SET vchrSiteName = @vchrSiteName
      ,intPoliticalRegionID = @intPoliticalRegionID
      ,intSiteGroupID = @intNewSiteGroupID
      ,tintLocalType = @tintLocalType  
      ,vchrLocal = @vchrLocal
      ,vchrDistanceFromPlace = @vchrDistanceFromPlace 
      ,vchrDirFromPlace = @vchrDirFromPlace 
      ,vchrInformalLocal = @vchrInformalLocal 
      ,tintPosCoordinates = @tintPosCoordinates 
      ,tintPosAreaType = @tintPosAreaType 
      ,fltPosX1 = @fltPosX1 
      ,fltPosY1 = @fltPosY1 
      ,fltPosX2 = @fltPosX2 
      ,fltPosY2 = @fltPosY2 
      ,tintPosXYDisplayFormat = @tintPosXYDisplayFormat 
      ,vchrPosSource = @vchrPosSource 
      ,vchrPosError = @vchrPosError 
      ,vchrPosWho = @vchrPosWho 
      ,vchrPosDate = @vchrPosDate 
      ,vchrPosOriginal = @vchrPosOriginal 
      ,vchrPosUTMSource = @vchrPosUTMSource 
      ,vchrPosUTMMapProj = @vchrPosUTMMapProj 
      ,vchrPosUTMMapName = @vchrPosUTMMapName 
      ,vchrPosUTMMapVer = @vchrPosUTMMapVer 
      ,tintElevType = @tintElevType 
      ,fltElevUpper = @fltElevUpper 
      ,fltElevLower = @fltElevLower 
      ,fltElevDepth = @fltElevDepth 
      ,vchrElevUnits = @vchrElevUnits 
      ,vchrElevSource = @vchrElevSource 
      ,vchrElevError = @vchrElevError 
      ,vchrGeoEra = @vchrGeoEra 
      ,vchrGeoState = @vchrGeoState 
      ,vchrGeoPlate = @vchrGeoPlate 
      ,vchrGeoFormation = @vchrGeoFormation 
      ,vchrGeoMember = @vchrGeoMember 
      ,vchrGeoBed = @vchrGeoBed 
      ,vchrGeoName = @vchrGeoName 
      ,vchrGeoAgeBottom = @vchrGeoAgeBottom 
      ,vchrGeoAgeTop = @vchrGeoAgeTop 
      ,vchrGeoNotes = @vchrGeoNotes 
      ,dtDateLastUpdated = CAST(GetDate()  AS datetime)
      ,vchrWhoLastUpdated = SYSTEM_USER 

    WHERE intSiteID = @intSiteID;

    