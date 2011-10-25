/*******************************************************************************
 * Copyright (C) 2011 Atlas of Living Australia
 * All Rights Reserved.
 * 
 * The contents of this file are subject to the Mozilla Public
 * License Version 1.1 (the "License"); you may not use this file
 * except in compliance with the License. You may obtain a copy of
 * the License at http://www.mozilla.org/MPL/
 * 
 * Software distributed under the License is distributed on an "AS
 * IS" basis, WITHOUT WARRANTY OF ANY KIND, either express or
 * implied. See the License for the specific language governing
 * rights and limitations under the License.
 ******************************************************************************/
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
