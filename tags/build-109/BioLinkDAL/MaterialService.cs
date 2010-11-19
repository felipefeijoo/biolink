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
            return GetExplorerElementsForParent(0, "Region");
        }

        public List<SiteExplorerNode> GetExplorerElementsForParent(int parentID, string parentElemType) {
            var mapper = new GenericMapperBuilder<SiteExplorerNode>().build();
            return StoredProcToList<SiteExplorerNode>("spSiteExplorerList", 
                mapper, 
                _P("intParentID", parentID), 
                _P("vchrParentType", parentElemType)
            );
        }

    }
}
