using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data;

namespace BioLink.Client.Extensibility {

    public static class TraitCategoryTypeHelper {

        private static Dictionary<TraitCategoryType, int> _IDMap = new Dictionary<TraitCategoryType, int>();

        public static int GetTraitCategoryTypeID(TraitCategoryType type) {

            lock (_IDMap) {
                if (!_IDMap.ContainsKey(type)) {
                    var service = new XMLIOService(PluginManager.Instance.User);
                    var id = service.GetTraitCategoryID(type.ToString());
                    _IDMap[type] = id;
                }
            }

            return _IDMap[type];
        }

        public static void Reset() {
            lock (_IDMap) {
                _IDMap.Clear();
            }
        }

        public static LookupType GetLookupTypeFromCategoryID(int catId) {

            if (catId == GetTraitCategoryTypeID(TraitCategoryType.Material)) {
                return LookupType.Material;
            } else if (catId == GetTraitCategoryTypeID(TraitCategoryType.Taxon)) {
                return LookupType.Taxon;
            } else {
                return LookupType.Unknown;
            }

        }

        public static int GetCategoryIDFromLookupType(LookupType l) {
            switch (l) {
                case LookupType.Material:
                    return TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Material);
                case LookupType.Taxon:
                    return TraitCategoryTypeHelper.GetTraitCategoryTypeID(TraitCategoryType.Taxon);
                default:
                    return -1;
            }
        }

    }

    public enum TraitCategoryType {
        Taxon,
        Region,
        Site,        
        SiteVisit,
        Trap,
        Material,                
        Multimedia,
        Reference,
        Journal,
        Contact,
        Loan,
        Biolink
    }

}
