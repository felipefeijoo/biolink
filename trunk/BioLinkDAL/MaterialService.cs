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

        public List<RegionTreeNode> GetTopLevelRegions() {
            var mapper = new GenericMapperBuilder<RegionTreeNode>().build();
            return StoredProcToList<RegionTreeNode>("spRegionListTop", mapper);
        }

        public List<RegionTreeNode> GetRegionsForParent(int parentID) {
            var mapper = new GenericMapperBuilder<RegionTreeNode>().build();
            return StoredProcToList<RegionTreeNode>("spRegionList", mapper, _P("intParentID", parentID));
        }

    }
}
