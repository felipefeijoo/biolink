/****** Object:  StoredProcedure [dbo].[spSiteImportGetID]    Script Date: 05/23/2014 09:50:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
/****** Object:  Stored Procedure dbo.spSiteImportGetID    Script Date: 2/4/99 9:39:12 AM ******/
ALTER   PROCEDURE [dbo].[spSiteImportGetID]
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

    DECLARE @intSiteID int

    -- Determine if the region is in the database
    SELECT @intSiteID = min(intSiteID) 
    FROM tblSite
    WHERE vchrSiteName = @vchrSiteName 
        AND intPoliticalRegionID = @intPoliticalRegionID
        AND ISNULL(vchrLocal, "") = ISNULL(@vchrLocal, "")
        AND ISNULL(vchrDistanceFromPlace, "") = ISNULL(@vchrDistanceFromPlace, "")
        AND ISNULL(vchrDirFromPlace, "") = ISNULL(@vchrDirFromPlace, "")
        AND ISNULL(vchrInformalLocal, "") like ISNULL(@vchrInformalLocal, "")
        AND ISNULL(tintPosCoordinates, 0) = ISNULL(@tintPosCoordinates, 0)
        AND ISNULL(tintPosAreaType, 0) = ISNULL(@tintPosAreaType, 0)
        AND CAST(ISNULL(fltPosX1, 0) * 1000 as int) = CAST(ISNULL(@fltPosX1, 0) * 1000 as int)
        AND CAST(ISNULL(fltPosY1, 0) * 1000 as int) = CAST(ISNULL(@fltPosY1, 0) * 1000 as int)
        AND CAST(ISNULL(fltPosX2, 0) * 1000 as int) = CAST(ISNULL(@fltPosX2, 0) * 1000 as int)
        AND CAST(ISNULL(fltPosY2, 0) * 1000 as int) = CAST(ISNULL(@fltPosY2, 0) * 1000 as int)
        AND ISNULL(tintPosXYDisplayFormat, 0) = ISNULL(@tintPosXYDisplayFormat, 0)
        AND ISNULL(vchrPosSource, "") = ISNULL(@vchrPosSource, "")
        AND ISNULL(vchrPosError, "") = ISNULL(@vchrPosError, "")
        AND ISNULL(vchrPosWho, "") = ISNULL(@vchrPosWho, "")
        AND ISNULL(vchrPosDate, "") = ISNULL(@vchrPosDate, "")
        AND ISNULL(vchrPosOriginal, "") = ISNULL(@vchrPosOriginal, "")
        AND ISNULL(vchrPosUTMSource, "") = ISNULL(@vchrPosUTMSource, "")
        AND ISNULL(vchrPosUTMMapProj, "") = ISNULL(@vchrPosUTMMapProj, "")
        AND ISNULL(vchrPosUTMMapName, "") = ISNULL(@vchrPosUTMMapName, "")
        AND ISNULL(vchrPosUTMMapVer, "") = ISNULL(@vchrPosUTMMapVer, "")
        AND ISNULL(tintElevType, 0) = ISNULL(@tintElevType, 0)
        AND ISNULL(fltElevUpper, 0.0) = ISNULL(@fltElevUpper, 0.0)
        AND ISNULL(fltElevLower, 0.0) = ISNULL(@fltElevLower, 0.0)
        AND ISNULL(fltElevDepth, 0.0) = ISNULL(@fltElevDepth, 0.0)
        AND ISNULL(vchrElevUnits, "") = ISNULL(@vchrElevUnits, "")
        AND ISNULL(vchrElevSource, "") = ISNULL(@vchrElevSource, "")
        AND ISNULL(vchrElevError, "") = ISNULL(@vchrElevError, "")
        AND ISNULL(vchrGeoEra, "") = ISNULL(@vchrGeoEra, "")
        AND ISNULL(vchrGeoState, "") = ISNULL(@vchrGeoState, "")
        AND ISNULL(vchrGeoPlate, "") = ISNULL(@vchrGeoPlate, "")
        AND ISNULL(vchrGeoFormation, "") = ISNULL(@vchrGeoFormation, "")
        AND ISNULL(vchrGeoMember, "") = ISNULL(@vchrGeoMember, "")
        AND ISNULL(vchrGeoBed, "") = ISNULL(@vchrGeoBed, "")
        AND ISNULL(vchrGeoName, "") = ISNULL(@vchrGeoName, "")
        AND ISNULL(vchrGeoAgeBottom, "") = ISNULL(@vchrGeoAgeBottom, "")
        AND ISNULL(vchrGeoAgeTop, "") = ISNULL(@vchrGeoAgeTop, "")
        AND ISNULL(vchrGeoNotes, "") like ISNULL(@vchrGeoNotes, "")

    -- If the site id does not exist, create it under the specified parent
    IF @intSiteID IS NULL
      BEGIN
        -- Do the insert
        EXEC @intSiteID = spSiteInsert @intPoliticalRegionID, 0, -1
        -- Fill the rest of the data.
        EXEC spSiteUpdate @intSiteID, 
                                         @vchrSiteName,
                                         @intPoliticalRegionID, 
                                         @tintLocalType,
                                         @vchrLocal, 
                                         @vchrDistanceFromPlace, 
                                         @vchrDirFromPlace, 
                                         @vchrInformalLocal, 
                                         @tintPosCoordinates, 
                                         @tintPosAreaType, 
                                         @fltPosX1, 
                                         @fltPosY1, 
                                         @fltPosX2, 
                                         @fltPosY2, 
                                         @tintPosXYDisplayFormat, 
                                         @vchrPosSource, 
                                         @vchrPosError, 
                                         @vchrPosWho, 
                                         @vchrPosDate, 
                                         @vchrPosOriginal, 
                                         @vchrPosUTMSource, 
                                         @vchrPosUTMMapProj, 
                                         @vchrPosUTMMapName, 
                                         @vchrPosUTMMapVer, 
                                         @tintElevType, 
                                         @fltElevUpper, 
                                         @fltElevLower, 
                                         @fltElevDepth, 
                                         @vchrElevUnits, 
                                         @vchrElevSource, 
                                         @vchrElevError, 
                                         @vchrGeoEra, 
                                         @vchrGeoState, 
                                         @vchrGeoPlate, 
                                         @vchrGeoFormation, 
                                         @vchrGeoMember, 
                                         @vchrGeoBed, 
                                         @vchrGeoName, 
                                         @vchrGeoAgeBottom, 
                                         @vchrGeoAgeTop, 
                                         @vchrGeoNotes
      END
   
    -- return the region id.    
    RETURN @intSiteID

