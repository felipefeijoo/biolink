using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BioLink.Data.Model;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    internal class QuerySQLGenerator {

        // Table name constants.
        private const string strTABLE_REGION = "tblPoliticalRegion";
        private const string strTABLE_REGION_ABBREV = "R";
        private const string strTABLE_SITEGROUP = "tblSiteGroup";
        private const string strTABLE_SITEGROUP_ABBREV = "SG";
        private const string strTABLE_SITE = "tblSite";
        private const string strTABLE_SITE_ABBREV = "S";
        private const string strTABLE_SITEVISIT = "tblSiteVisit";
        private const string strTABLE_SITEVISIT_ABBREV = "SV";
        private const string strTABLE_MATERIAL = "tblMaterial";
        private const string strTABLE_MATERIAL_ABBREV = "M";

        private const string strTABLE_MATERIALPART = "tblMaterialPart";
        private const string strTABLE_MATERIALPART_ABBREV = "MP";
        private const string strTABLE_MATERIALASSOC = "tblMaterialAssoc";
        private const string strTABLE_MATERIALASSOC_ABBREV = "MA";

        private const string strTABLE_BIOTA = "tblBiota";
        private const string strTABLE_BIOTA_ABBREV = "B";

        private const string strTABLE_BIOTA_RANK = "tblBiotaDefRank";
        private const string strTABLE_BIOTA_RANK_ABBREV = "DBF";

        private const string strTABLE_COMMON_NAME = "tblCommonName";
        private const string strTABLE_COMMON_NAME_ABBREV = "CN";

        private const string strTABLE_BIOTA_DIST = "tblBiotaDistribution";
        private const string strTABLE_BIOTA_DIST_ABBREV = "BD";

        private const string strTABLE_ASSOC = "vwAssociateText";
        private const string strTABLE_ASSOC_ABBREV = "AT";

        private const string strTABLE_STORAGE_LOCATION = "tblBiotaLocation";
        private const string strTABLE_STORAGE_LOCATION_ABBREV = "BL";

        private const string strTABLE_BIOTA_STORAGE = "tblBiotaStorage";
        private const string strTABLE_BIOTA_STORAGE_ABBREV = "BS";

        private const string strTABLE_DIST_REGION = "tblDistributionRegion";
        private const string strTABLE_DIST_REGION_ABBREV = "DR";


        private const string strFORMAT_BLDATE_TO_DISPLAY = "BLDateToDisplay";


        private IEnumerable<QueryCriteria> _criteria;
        private bool _distinct;

        private List<String> _tableList = new List<String>();
        private List<String> _fieldList = new List<String>();
        private List<String> _hiddenFieldList = new List<String>();

        private List<string> _traitClauses = new List<string>();


        private List<String> _whereClause = new List<String>();
        private List<string> _orderByclause = new List<string>();
        private StringBuilder _formatting = new StringBuilder();

        protected User User { get; private set; }

        protected QuerySQLGenerator(User user, IEnumerable<QueryCriteria> criteria, bool distinct) {
            _criteria = criteria;
            _distinct = distinct;
            this.User = user;
        }

        protected QueryComponents Generate() {

            foreach (QueryCriteria c in _criteria) {
                SplitField(c);
            }

            var strDistinctClause = _distinct ? " DISTINCT " : "";

            var strFromClause = BuildFromClause();
            var strWhereClause = BuildWhereClause();
            var strFieldList = _fieldList.Join(",");
            var strOrderByClause = _orderByclause.Join(",");

            if (!string.IsNullOrWhiteSpace(strOrderByClause)) {
                strWhereClause += " ORDER BY " + strOrderByClause;
            }

            var fullFrom = new List<String>(_fieldList);
            fullFrom.AddRange(_hiddenFieldList);

            var ret = new QueryComponents { Select = strDistinctClause + strFieldList, From = strFromClause, Where = strWhereClause, SelectHidden=fullFrom.Join(",") };
            return ret;
        }

        private string BuildWhereClause() {

            string where = _whereClause.Join(" AND ");

            foreach (String trait in _traitClauses) {
                if (trait.Contains((char) 1)) {
                    // Add the joining 'and' if the WHERE clause has contents.
                    if (where != "") { 
                        where += " AND ";
                    }
                    where += trait.Substring(trait.IndexOf((char)1) + 1);
                }
            }

            return where;
        }


        private bool TableInList(params string[]  tables) {
            foreach (string t in tables) {
                if (_tableList.Contains(t, StringComparer.CurrentCultureIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }

        private bool TableRequired(string pstrReqTable) {
            // If the table is in the list, return true
            if (_tableList.Contains(pstrReqTable, StringComparer.CurrentCultureIgnoreCase)) {
                return true;
            }
    
            switch (pstrReqTable) {
                case strTABLE_REGION:
                    return false;
                case strTABLE_SITE:
                    return TableInList(strTABLE_REGION) && TableInList(strTABLE_SITEVISIT, strTABLE_MATERIAL, strTABLE_MATERIALPART, strTABLE_BIOTA, strTABLE_BIOTA_RANK, strTABLE_COMMON_NAME, strTABLE_BIOTA_DIST, strTABLE_DIST_REGION, strTABLE_ASSOC, strTABLE_BIOTA_STORAGE);  
                case strTABLE_SITEVISIT:
                    return TableInList(strTABLE_REGION, strTABLE_SITE) && TableInList(strTABLE_MATERIAL, strTABLE_MATERIALPART, strTABLE_BIOTA, strTABLE_BIOTA_RANK, strTABLE_BIOTA_RANK, strTABLE_COMMON_NAME, strTABLE_BIOTA_DIST, strTABLE_DIST_REGION, strTABLE_ASSOC, strTABLE_BIOTA_STORAGE);    
                case strTABLE_MATERIAL:
                    return TableInList(strTABLE_REGION, strTABLE_SITE, strTABLE_SITEVISIT) &&  TableInList(strTABLE_MATERIALPART, strTABLE_BIOTA, strTABLE_BIOTA_RANK, strTABLE_COMMON_NAME, strTABLE_BIOTA_DIST, strTABLE_DIST_REGION, strTABLE_BIOTA_STORAGE);
                case strTABLE_MATERIALPART:
                    return false;    
                case strTABLE_BIOTA:
                    return TableInList(strTABLE_REGION, strTABLE_SITE, strTABLE_SITEVISIT, strTABLE_MATERIAL) &&  TableInList(strTABLE_BIOTA_RANK, strTABLE_COMMON_NAME, strTABLE_BIOTA_DIST, strTABLE_DIST_REGION, strTABLE_BIOTA_STORAGE);
                case strTABLE_BIOTA_RANK:
                    return false;
                case strTABLE_COMMON_NAME:
                    return false;
                case strTABLE_BIOTA_DIST:
                    return TableInList(strTABLE_REGION, strTABLE_SITE, strTABLE_SITEVISIT, strTABLE_MATERIAL, strTABLE_BIOTA) && TableInList(strTABLE_DIST_REGION);    
                case strTABLE_DIST_REGION:
                    return false;
                case strTABLE_ASSOC:
                    return TableInList(strTABLE_REGION, strTABLE_SITE, strTABLE_SITEVISIT, strTABLE_MATERIAL, strTABLE_BIOTA) && TableInList(strTABLE_ASSOC);
                case strTABLE_STORAGE_LOCATION:
                    return TableInList(strTABLE_REGION, strTABLE_SITE, strTABLE_SITEVISIT, strTABLE_MATERIAL, strTABLE_BIOTA) && TableInList(strTABLE_BIOTA_STORAGE);
                case strTABLE_BIOTA_STORAGE:
                    return false;
                default:    
                    return false;
            }

        }

        private string BuildFromClause() {
            string strWorkingClause = "";
            if (TableRequired(strTABLE_REGION)) {
                strWorkingClause = strTABLE_REGION + " " + GetTableAlias(strTABLE_REGION);
            }

            var lngLevels = 0;

            // Add the site table.
            if (TableRequired(strTABLE_SITE)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " " + strTABLE_SITE + " " + GetTableAlias(strTABLE_SITE);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_REGION) + ".intPoliticalRegionID = " + GetTableAlias(strTABLE_SITE) + ".intPoliticalRegionID))";
                    lngLevels++;
                } else {
                    strWorkingClause += " " + strTABLE_SITE + " " + GetTableAlias(strTABLE_SITE) + " ";
                }
            }

            //' Add the site visit table.
            if (TableRequired(strTABLE_SITEVISIT)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblSiteVisit " + GetTableAlias("tblSiteVisit");
                    strWorkingClause += " ON (" + GetTableAlias("tblSite") + ".intSiteID = " + GetTableAlias("tblSiteVisit") + ".intSiteID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblSiteVisit " + GetTableAlias("tblSiteVisit") + " ";
                }
            }

            // Add the Material table.
            if (TableRequired(strTABLE_MATERIAL)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblMaterial " + GetTableAlias("tblMaterial");
                    strWorkingClause += " ON (" + GetTableAlias("tblSiteVisit") + ".intSiteVisitID = " + GetTableAlias("tblMaterial") + ".intSiteVisitID))";
                    lngLevels++;
                } else {
                    strWorkingClause += strWorkingClause + "tblMaterial " + GetTableAlias("tblMaterial") + " ";
                }
            }

            // Material Part
            if (TableRequired(strTABLE_MATERIALPART)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " " + strTABLE_MATERIALPART + " " + GetTableAlias(strTABLE_MATERIALPART);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_MATERIAL) + ".intMaterialID = " + GetTableAlias(strTABLE_MATERIALPART) + ".intMaterialID))";
                    lngLevels++;
                } else {
                    strWorkingClause += " " + strTABLE_MATERIALPART + " " + GetTableAlias(strTABLE_MATERIALPART) + " ";
                }
            }

            // Add the Biota table.
            if (TableRequired(strTABLE_BIOTA)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblBiota " + GetTableAlias(strTABLE_BIOTA);
                    strWorkingClause += " ON (" + GetTableAlias("tblMaterial") + ".intBiotaID = " + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblBiota " + GetTableAlias(strTABLE_BIOTA) + " ";
                }
            }

            // Add the Rank table.
            if (TableRequired(strTABLE_BIOTA_RANK)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblBiotaDefRank " + GetTableAlias(strTABLE_BIOTA_RANK);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".chrElemType = " + GetTableAlias(strTABLE_BIOTA_RANK) + ".chrCode))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblBiotaDefRank " + GetTableAlias(strTABLE_BIOTA_RANK) + " ";
                }
            }

            // Add the common name table.
            if (TableRequired(strTABLE_COMMON_NAME)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblCommonName " + GetTableAlias(strTABLE_COMMON_NAME);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID = " + GetTableAlias(strTABLE_COMMON_NAME) + ".intBiotaID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblCommonName " + GetTableAlias(strTABLE_COMMON_NAME) + " ";
                }
            }

            // Add the associate table.
            if (TableRequired(strTABLE_ASSOC)) {
                if (strWorkingClause != "") {
                    if (TableRequired(strTABLE_BIOTA) && TableRequired(strTABLE_MATERIAL)) {
                        strWorkingClause += "\r\n\t";
                        strWorkingClause += " LEFT OUTER JOIN ";
                        strWorkingClause += " vwAssociateText " + GetTableAlias(strTABLE_ASSOC);
                        strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID = " + GetTableAlias(strTABLE_ASSOC) + ".FromBiotaID";
                        strWorkingClause += " AND " + GetTableAlias(strTABLE_MATERIAL) + ".intMaterialID = " + GetTableAlias(strTABLE_ASSOC) + ".FromMaterialID))";
                        lngLevels++;
                    } else if (TableRequired(strTABLE_BIOTA)) {
                        strWorkingClause += "\r\n\t";
                        strWorkingClause += " LEFT OUTER JOIN ";
                        strWorkingClause += " vwAssociateText " + GetTableAlias(strTABLE_ASSOC);
                        strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID = " + GetTableAlias(strTABLE_ASSOC) + ".FromBiotaID))";
                        lngLevels++;
                    } else if (TableRequired(strTABLE_MATERIAL)) {
                        strWorkingClause += "\r\n\t";
                        strWorkingClause += " LEFT OUTER JOIN ";
                        strWorkingClause += " vwAssociateText " + GetTableAlias(strTABLE_ASSOC);
                        strWorkingClause += " ON (" + GetTableAlias(strTABLE_MATERIAL) + ".intMaterialID = " + GetTableAlias(strTABLE_ASSOC) + ".FromMaterialID))";
                        lngLevels++;
                    }
                } else {
                    strWorkingClause += "vwAssociateText " + GetTableAlias(strTABLE_ASSOC) + " ";
                }
            }

            // Add the distribution....
            if (TableRequired(strTABLE_BIOTA_DIST)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblBiotaDistribution " + GetTableAlias(strTABLE_BIOTA_DIST);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID = " + GetTableAlias(strTABLE_BIOTA_DIST) + ".intBiotaID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblBiotaDistribution " + GetTableAlias(strTABLE_BIOTA_DIST) + " ";
                }
            }


            // Add the Distrubtion region
            if (TableRequired(strTABLE_DIST_REGION)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblDistributionRegion " + GetTableAlias(strTABLE_DIST_REGION);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA_DIST) + ".intDistributionRegionID = " + GetTableAlias(strTABLE_DIST_REGION) + ".intDistributionRegionID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblDistributionRegion " + GetTableAlias(strTABLE_DIST_REGION) + " ";
                }
            }

            // Add the storage....
            if (TableRequired(strTABLE_STORAGE_LOCATION)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblBiotaLocation " + GetTableAlias(strTABLE_STORAGE_LOCATION);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_BIOTA) + ".intBiotaID = " + GetTableAlias(strTABLE_STORAGE_LOCATION) + ".intBiotaID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblBiotaLocation " + GetTableAlias(strTABLE_STORAGE_LOCATION) + " ";
                }
            }

            // Add the storage area
            if (TableRequired(strTABLE_BIOTA_STORAGE)) {
                if (strWorkingClause != "") {
                    strWorkingClause += "\r\n\t";
                    strWorkingClause += " LEFT OUTER JOIN ";
                    strWorkingClause += " tblBiotaStorage " + GetTableAlias(strTABLE_BIOTA_STORAGE);
                    strWorkingClause += " ON (" + GetTableAlias(strTABLE_STORAGE_LOCATION) + ".intBiotaStorageID = " + GetTableAlias(strTABLE_BIOTA_STORAGE) + ".intBiotaStorageID))";
                    lngLevels++;
                } else {
                    strWorkingClause += "tblBiotaStorage " + GetTableAlias(strTABLE_BIOTA_STORAGE) + " ";
                }
            }

            // Add the traits
            foreach (string trait in _traitClauses) {
                if (trait.IndexOf((char)1) > 0) {
                    strWorkingClause += "\r\n\t" + " " + trait.Substring(0, trait.IndexOf((char)1)) + ")";
                } else {
                    strWorkingClause += "\r\n\t" + " " + trait + ")";
                }
                lngLevels++;
            }


            // add the beginning parenthesis.
            for (int i = 0; i < lngLevels; ++i) {
                strWorkingClause = "(" + strWorkingClause;
            }

            return strWorkingClause;
        }

        private void SplitField(QueryCriteria c) {
            string traitID, traitCatID, traitTableName, traitTableAlias = null;
            bool isTrait = IsTrait(c.Field, out traitID, out traitCatID, out traitTableName);
            if (isTrait) {
                traitTableAlias = string.Format("T{0}", _traitClauses.Count + 1);
                AddToTraitList(traitTableAlias, traitID, traitCatID, traitTableName, FleshCriteria(c.Criteria, "", traitTableAlias + ".vchrValue"));
                AddTable(traitTableName);
            } else {
                AddTable(c.Field.TableName);
            }

            string alias = c.Alias;
            if (string.IsNullOrEmpty(alias)) {
                alias = c.Field.DisplayName;
            }

            if (isTrait) {

                AddTraitToFieldList(c.Field.FieldName, c.Output, traitTableAlias, alias, traitID, traitCatID, traitTableName);
                if (!String.IsNullOrEmpty(c.Sort)) {
                    AddToSortList(c.Sort, traitTableAlias, "vchrValue");
                }
            } else {
                // Add the field to the list.
                AddToFieldList(c, FleshCriteria(c.Criteria, c.Field.TableName, c.Field.FieldName));
                if (!String.IsNullOrEmpty(c.Sort)) {
                    AddToSortList(c.Sort, GetTableAlias(c.Field.TableName), c.Field.FieldName);
                }
            }
        }

        private void AddToFieldList(QueryCriteria c, string pstrCriteria) {

            if (c.Output) {

                string alias = c.Alias;
                if (string.IsNullOrEmpty(alias)) {
                    alias = c.Field.DisplayName;
                }

                if (String.IsNullOrWhiteSpace(alias)) {
                    _fieldList.Add(GetTableAlias(c.Field.TableName) + ".[" + c.Field.FieldName + "]");
                } else {
                    _fieldList.Add(GetTableAlias(c.Field.TableName) + ".[" + c.Field.FieldName + "] AS [" + alias + "]");
                }

                // Add to the formatting requirements if required.
                if (!String.IsNullOrWhiteSpace(c.Field.Format)) {
                    _formatting.Append((char)1).Append(c.Alias).Append((char)2).Append(c.FormatOption).Append((char)2).Append(c.Field.Format);
                } else if (c.Field.FieldName == "intDateStart" || c.Field.FieldName == "intDateEnd") {
                    _formatting.Append((char)1).Append(c.Alias).Append((char)2).Append(strFORMAT_BLDATE_TO_DISPLAY).Append((char)2);
                }
            }
            if (!String.IsNullOrWhiteSpace(pstrCriteria)) {
                _whereClause.Add("(" + pstrCriteria.Trim() + ")");
            }
        }

        private void AddTraitToFieldList(string pstrFieldName, bool pbOutput, string pstrTraitTableAlias, string pstrAlias, string pstrTraitID, string pstrTraitCatID, string pstrTraitDBTable) {

            // Add the current trait details to the list for processing later.
            // Add the field definition.
            if (pbOutput) {

                // Add to select clause.                
                if (String.IsNullOrWhiteSpace(pstrAlias)) {
                    _fieldList.Add(pstrTraitTableAlias + ".vchrValue");
                } else {
                    _fieldList.Add(pstrTraitTableAlias + ".vchrValue AS [" + pstrAlias + "]");
                }

            }
        }

        private void AddToSortList(string pstrSort, string pstrTableAlias, string pstrFieldName) {

            //  Add the current entry to the sort list.

            if (pstrSort.StartsWith("a", StringComparison.CurrentCultureIgnoreCase)) {
                _orderByclause.Add(pstrTableAlias + "." + pstrFieldName + " ASC");
            } else if (pstrSort.StartsWith("d", StringComparison.CurrentCultureIgnoreCase)) {
                _orderByclause.Add(pstrTableAlias + "." + pstrFieldName + " DESC");
            }
        }


        private void AddTable(string tableName) {

            if (_tableList.Contains(tableName, StringComparer.CurrentCultureIgnoreCase)) {
                return;
            }
            
            _tableList.Add(tableName);

            var traitKeyField = GetTableTraitKeyField(tableName);
            if (!String.IsNullOrEmpty(traitKeyField)) {
                _hiddenFieldList.Add(string.Format("{0}.{1} AS [{2}{1}]",   GetTableAlias(tableName), traitKeyField, BioLinkService.HIDDEN_COLUMN_PREFIX));
            }
        }

        private string GetTableTraitKeyField(string pstrTableName) {
            //
            // Extract the field from the table that links to the traits table.
            //

            if (SupportService.TableTraitKeyFields.ContainsKey(pstrTableName)) {
                return SupportService.TableTraitKeyFields[pstrTableName];
            }

            return "";

        }

        private void AddToTraitList(string pstrTraitAlias, string pstrTraitID, string pstrTraitCatID, string pstrTraitDBTable, string pstrCriteria ) {
            // extract the alias for the table when used in queries.
            var strTableAlias = GetTableAlias(pstrTraitDBTable);
            // Extract the table field used to link the table to the traits
            var strTableTraitKeyField = GetTableTraitKeyField(pstrTraitDBTable);
            // Build the portion of the web clause.
            var strColKey = strTableAlias + "." + strTableTraitKeyField;
    
            // Look to see if the table has been placed in the collection and if not,
            // insert it.
            string strTraitFromWhereClause = "";
            if (!string.IsNullOrWhiteSpace(pstrCriteria)) {
                strTraitFromWhereClause = "INNER JOIN ";  // if we havce a criteria, we can get away with an inner join
            } else {
                strTraitFromWhereClause = "LEFT OUTER JOIN ";
            }
            strTraitFromWhereClause += " tblTrait " + pstrTraitAlias + " ON (" + strColKey + " = " + pstrTraitAlias + ".intIntraCatID AND " + pstrTraitAlias + ".intTraitTypeID = " + pstrTraitID + ")";
    
            if (pstrCriteria != "") {
                strTraitFromWhereClause += (char) 1 + pstrCriteria;
            }
    
            // Add the current criteria to the trait, including the OR if it is not the first.
            _traitClauses.Add(strTraitFromWhereClause);
        }

        private string FleshCriteria(string criteria, string table, string fname) {
            if (string.IsNullOrWhiteSpace(criteria)) {
                return "";
            }

            string fullFieldName = "";

            if (string.IsNullOrWhiteSpace(table)) {
                fullFieldName = fname;
            } else {
                fullFieldName = string.Format("{0}.{1}", GetTableAlias(table), fname.Trim());
            }
            int elemCount = 0;            
            string strCurrentCriteriaSegment;
            string strOperator;

            var operators = new List<String>();
            var components = new List<String>();

            while (criteria != "") {
                GetNextCriteria(ref criteria, out strCurrentCriteriaSegment, out strOperator);
                elemCount++;
                // components.Add(strCurrentCriteriaSegment);
                operators.Add(strOperator);

                // If the field is a hierarchical field, look for the 'ONLY' flag and produce the approriate SQL Where component.
                if (fullFieldName.Equals(GetTableAlias(strTABLE_BIOTA) + ".vchrepithet", StringComparison.CurrentCultureIgnoreCase) ||
                    fullFieldName.Equals(GetTableAlias(strTABLE_BIOTA) + ".vchrFullName", StringComparison.CurrentCultureIgnoreCase) ||
                    fullFieldName.Equals(GetTableAlias(strTABLE_REGION) + ".vchrName", StringComparison.CurrentCultureIgnoreCase) ||
                    fullFieldName.Equals(GetTableAlias(strTABLE_BIOTA_STORAGE) + ".vchrName", StringComparison.CurrentCultureIgnoreCase)) {
                    components.Add(BuildHierCriteria(strCurrentCriteriaSegment, table, fname));
                } else if (fullFieldName.Equals(GetTableAlias(strTABLE_DIST_REGION) + ".vchrName", StringComparison.CurrentCultureIgnoreCase)) {
                    components.Add(BuildBiotaDistCriteria(strCurrentCriteriaSegment));
                } else if (fname.StartsWith("bit", StringComparison.CurrentCultureIgnoreCase)) {
                    var s = strCurrentCriteriaSegment.ToLower().Replace("'false'", "0");
                    s = s.Replace("'true'", "1");
                    components.Add(fullFieldName + " " + s);
                } else {
                    components.Add(fullFieldName + " " + strCurrentCriteriaSegment);
                }

            }

            var ret = "";
            for (int i = 0; i < components.Count; ++i) {
                ret += " " + operators[i] + " " + components[i];
            }

            return ret;
        }

        private string BuildHierCriteria(string pstrCriteria, string pstrTable, string pstrFieldName) {
            //
            // If the field is hierarchical, the 'ONLY' keyword at the end means only that item. If the keyword is
            // missing, all elements under the hierarchy are included.
            //

            string strCriteria = pstrCriteria.Trim();

            // Determine if the 'ONLY' keyword is visible.
            if (strCriteria.EndsWith(" only'", StringComparison.CurrentCultureIgnoreCase)) {
                // We have the 'ONLY' keyword.
                return GetTableAlias(pstrTable) + "." + pstrFieldName + " " + strCriteria.Substring(0, strCriteria.Length - 5) + "'";
            } else if (strCriteria.EndsWith(" only", StringComparison.CurrentCultureIgnoreCase)) {
                return GetTableAlias(pstrTable) + "." + pstrFieldName + " " + strCriteria.Substring(0, strCriteria.Length - 4);
            }

            // Change the criteria for one that determines the Parentage paths of all the relevant data and produces
            // a 'x like <parentage path 1>% or x like <parentage path 2>%' type structure.

            var service = new SupportService(User);
            var criteria = "(";
            service.StoredProcReaderForEach("spQueryHierLookup", (reader) => {
                if (criteria != "(") {
                    criteria += " OR ";
                }
                criteria += GetTableAlias(pstrTable) + ".vchrParentage LIKE '" + reader["vchrParentage"].ToString().Trim() + "%'";
            }, service._P("vchrTableName", pstrTable), service._P("txtCriteria", pstrFieldName + " " + pstrCriteria));

            criteria += ")";


            if (criteria == "()") {
                criteria = GetTableAlias(pstrTable) + ".vchrParentage LIKE ''";
            }

            return criteria;
        }

        private string BuildBiotaDistCriteria(string pstrCriteria) {
            //
            // If the field is Biota Distribution, the 'ONLY' keyword at the end means only that item. If the 'only' keyword is
            // missing, all elements under the Distribution as well as above are included.
            //

            //On Error GoTo BuildBiotaDistCriteria_ErrH
            //Dim strCriteria As String
            //Dim rs As ADODB.Recordset
            //Dim bOk As Boolean
            //Dim strParentage As String
            //Dim strID As String

            var strCriteria = pstrCriteria.Trim();

            // Determine if the 'ONLY' keyword is visible.
            if (strCriteria.EndsWith(" only", StringComparison.CurrentCultureIgnoreCase)) {
                // We have the 'ONLY' keyword.
                return strTABLE_DIST_REGION_ABBREV + ".vchrName " + strCriteria.Substring(0, strCriteria.Length - 4);
            }


            // Change the criteria for one that determines the Parentage paths of all the relevant data and produces
            // a 'x like <parentage path 1>% or x like <parentage path 2>%' type structure.

            strCriteria = "(";
            var service = new SupportService(User);
            service.StoredProcReaderForEach("spQueryBiotaDistLookup", (reader) => {
                // Extract the components
                var strID = reader["intDistributionRegionID"].ToString().Trim();
                var strParentage = reader["vchrParentage"].ToString().Trim();

                var regex = new Regex(string.Format(@"^[\\](.*?)[\\]*{0}$", strID));

                var match = regex.Match(strParentage);
                if (match.Success && match.Groups.Count > 1) {
                    strParentage = match.Groups[1].Value;
                    if (!string.IsNullOrEmpty(strParentage)) {
                        strParentage = strParentage.Replace("\\", ",");
                        // Perform the join for multiple selectors
                        if (strCriteria != "(") {
                            strCriteria += " OR ";
                        }
                        strCriteria += "((" + strTABLE_BIOTA_DIST_ABBREV + ".intDistributionRegionID IN (" + strParentage + ") AND " + strTABLE_BIOTA_DIST_ABBREV + ".bitThroughoutRegion = 1) OR " + strTABLE_BIOTA_DIST_ABBREV + ".intDistributionRegionID = " + strID + " OR " + strTABLE_DIST_REGION_ABBREV + ".vchrParentage LIKE '" + reader["vchrParentage"].ToString().Trim() + "\\%')";
                    }
                }

            }, service._P("txtCriteria", pstrCriteria));

            strCriteria += ")";

            if (strCriteria == "()") {
                strCriteria = strTABLE_DIST_REGION_ABBREV + ".vchrParentage LIKE ''";
            }

            return strCriteria;
        }

        private string GetTableAlias(string table) {
            if (SupportService.TableAliases.ContainsKey(table)) {
                return SupportService.TableAliases[table];
            }
            return table;
        }

        private void GetNextCriteria(ref string pstrCriteria, out string pstrCurrCriteriaSegment, out string pstrOperator, bool pbDefaultOperator = true) {
            //
            // Extract the first criteria segment in the criteria. Split this into the criteria and the joining. Then remove this selection from the criteria.
            //

            pstrCriteria = pstrCriteria.Trim();


            // Extract the boolean operator if applicable.
            pstrOperator = "";

            if (pstrCriteria.StartsWith("and ", StringComparison.CurrentCultureIgnoreCase)) {
                pstrOperator = "AND";
                pstrCriteria = pstrCriteria.Substring(4);
            } else if (pstrCriteria.StartsWith("or ", StringComparison.CurrentCultureIgnoreCase)) {
                pstrOperator = "OR";
                pstrCriteria = pstrCriteria.Substring(3);
            }

            if (pstrCriteria.StartsWith("not ", StringComparison.CurrentCultureIgnoreCase)) {
                pstrOperator += " NOT";
                pstrCriteria = pstrCriteria.Substring(4);
            }

            // Look for the next criteria joining operator like 'and', 'or' or 'not'.
            var lngCurrCriteriaEnd = GetNextBooleanOperator(ref pstrCriteria);

            if (lngCurrCriteriaEnd == 0) {
                // No operators so return elements.
                pstrCurrCriteriaSegment = ReplaceAsterisks(pstrCriteria);
                if (pbDefaultOperator) {
                    pstrCurrCriteriaSegment = DefaultOperator(pstrCurrCriteriaSegment);
                }
                pstrCriteria = "";
            } else {
                pstrCurrCriteriaSegment = ReplaceAsterisks(pstrCriteria.Substring(0, lngCurrCriteriaEnd));
                if (pbDefaultOperator) {
                    pstrCurrCriteriaSegment = DefaultOperator(pstrCurrCriteriaSegment);
                }

                pstrCriteria = pstrCriteria.Substring(lngCurrCriteriaEnd + 1).Trim();
            }
        }

        //
        // Replace wildcard *'s with #'s
        //
        private string ReplaceAsterisks(string pstrCriteria) {
            var strCriteria = pstrCriteria.Trim();

            if (!strCriteria.StartsWith("like ", StringComparison.CurrentCultureIgnoreCase)) {
                return strCriteria;
            }

            return strCriteria.Replace("*", "%");
        }

        private string DefaultOperator(string pstrSQLSnippet) {
            //
            // Ensure the SQL snippet starts with a valid operator.
            //

            var strSnippet = pstrSQLSnippet.TrimStart(' ', '\t', '\r', '\n').ToLower();

            // If the start of the criteria is not a valid operator, insert an equals.
            if (strSnippet.StartsWith("=") || strSnippet.StartsWith(">") || strSnippet.StartsWith("<") || strSnippet.StartsWith("!") ||
               strSnippet.StartsWith("like") || strSnippet.StartsWith("in ") || strSnippet.StartsWith("in(") || strSnippet.StartsWith("between ") ||
               strSnippet.StartsWith("not ") || strSnippet.StartsWith("is ")) {
                // Do nothing, there is a correct joiner.
                return strSnippet;
            } else {
                return " = '" + strSnippet + "'";
            }
        }

        private int GetNextBooleanOperator(ref string pstrCriteria) {
            //
            // Extract the next criteria and optionally the operator at its start.
            //

            var lngNextNotOperator = pstrCriteria.IndexOf(" not ", StringComparison.CurrentCultureIgnoreCase);
            var lngNextOrOperator = pstrCriteria.IndexOf(" or ", StringComparison.CurrentCultureIgnoreCase);
            var lngNextAndOperator = pstrCriteria.IndexOf(" and ", StringComparison.CurrentCultureIgnoreCase);
            var lngNextBetweenOperator = pstrCriteria.IndexOf("between ", StringComparison.CurrentCultureIgnoreCase);

            // If we have an 'and' and 'between' then we need to look past this 'and' for the
            // next one.
            if (lngNextBetweenOperator < lngNextAndOperator && lngNextBetweenOperator > 0) {
                lngNextAndOperator = pstrCriteria.IndexOf(" and ", lngNextAndOperator, StringComparison.CurrentCultureIgnoreCase);
            }

            // Determine which of the operators is first
            if (lngNextOrOperator > 0 && (lngNextOrOperator < lngNextAndOperator || lngNextAndOperator == 0) && (lngNextOrOperator < lngNextNotOperator || lngNextNotOperator == 0)) {
                return lngNextOrOperator;
            }

            if (lngNextAndOperator > 0 && (lngNextAndOperator < lngNextOrOperator || lngNextOrOperator == 0) && (lngNextAndOperator < lngNextNotOperator || lngNextNotOperator == 0)) {
                return lngNextAndOperator;
            }

            if (lngNextNotOperator > 0 && (lngNextNotOperator < lngNextOrOperator || lngNextOrOperator == 0) && (lngNextNotOperator < lngNextAndOperator || lngNextAndOperator == 0)) {
                return lngNextNotOperator;
            }
            return 0;
        }


        private bool IsTrait(FieldDescriptor f, out string traitID, out string catID, out string tableName) {
            var s = f.TableName.Split('.');
            if (s.Length == 3) {
                traitID = s[1];
                catID = s[2];
                tableName = s[0];
                return true;
            }

            traitID = null;
            catID = null;
            tableName = null;

            return false;
        }

        public static QueryComponents GenerateSQL(User user, IEnumerable<QueryCriteria> criteria, bool distinct) {
            var instance = new QuerySQLGenerator(user, criteria, distinct);
            return instance.Generate();
        }
    }

    class QueryComponents {
        public string Select { get; set; }
        public string SelectHidden { get; set; }
        public string Where { get; set; }
        public string From { get; set; }        
    }

}
