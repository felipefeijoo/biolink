using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    public class MaterialService : BioLinkService {

        public MaterialService(User user)
            : base(user) {
        }

        public List<SiteExplorerNode> GetTopLevelExplorerItems() {
            return GetExplorerElementsForParent(0, SiteExplorerNodeType.Region);
        }

        public List<SiteExplorerNode> GetExplorerElementsForParent(int parentID, SiteExplorerNodeType parentElemType) {
            return GetExplorerElementsForParent(parentID, parentElemType.ToString());
        }

        public List<SiteExplorerNode> GetExplorerElementsForParent(int parentID, string parentElemType) {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().build();
            return StoredProcToList<SiteExplorerNode>("spSiteExplorerList", 
                mapper, 
                _P("intParentID", parentID), 
                _P("vchrParentType", parentElemType)
            );
        }

        #region Region

        public Region GetRegion(int regionID) {
            var mapper = new GenericMapperBuilder<Region>().build();
            Region result = null;
            StoredProcReaderFirst("spRegionGet", (reader) => {
                result = mapper.Map(reader);
            }, _P("intRegionID", regionID));
            return result;
        }

        public void UpdateRegion(int regionID, string name, string rank) {
            StoredProcUpdate("spRegionUpdate",
                _P("intRegionID", regionID),
                _P("vchrName", name),
                _P("vchrRank", rank)
            );
        }

        public void DeleteRegion(int regionID) {
            StoredProcUpdate("spRegionDelete", _P("intRegionID", regionID));
        }

        public void RenameRegion(int regionID, string name) {
            StoredProcUpdate("spRegionRename",
                _P("intRegionID", regionID),
                _P("vchrName", name)
            );
        }

        public int InsertRegion(string name, int parentID) {
            var retval = ReturnParam("NewRegionID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRegionInsert",
                _P("vchrName", name),
                _P("intParentID", parentID),
                retval
            );
            return (int) retval.Value;
        }

        #endregion

        #region Site

        public void RenameSite(int siteID, string name) {
            StoredProcUpdate("spSiteUpdateName",
                _P("intSiteID", siteID),
                _P("vchrSiteName", name)
            );
        }

        #endregion
    }
}
