using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;

namespace BioLink.Data {

    internal class QuerySQLGenerator {

        private IEnumerable<QueryCriteria> _criteria;
        private bool _distinct;

        private List<String> _tableList = new List<String>();
        private List<String> _fieldList = new List<String>();
        private List<String> _hiddenFieldList = new List<String>();

        private List<string> _traitClauses = new List<string>();

        private StringBuilder _whereClause = new StringBuilder();
        private StringBuilder _orderByclause = new StringBuilder();
        private StringBuilder _formatting = new StringBuilder();

        protected QuerySQLGenerator(IEnumerable<QueryCriteria> criteria, bool distinct) {
            _criteria = criteria;
            _distinct = distinct;
        }

        protected string Generate() {

            foreach (QueryCriteria c in _criteria) {
                SplitField(c);
            }

            var strDistinctClause = _distinct ? " DISTINCT " : "";

            var strFieldList = "";
            var strFromClause = "";
            var strWhereClause = "";
            var strOrderByClause = "";

            var sql = String.Format("SELECT {0}{1}\nFROM {2}\nWHERE {3}\n", strDistinctClause, strFieldList, strFromClause, strWhereClause);

            if (strOrderByClause != "") {
                sql += "ORDER BY " + strOrderByClause;
            }

            return sql;
        }

        private void SplitField(QueryCriteria c) {
            string traitID, traitCatID, traitTableName;
            bool isTrait = IsTrait(c.Field, out traitID, out traitCatID, out traitTableName);
            if (isTrait) {
                var traitTableAlias = string.Format("T{0}", _traitClauses.Count + 1);
                AddTraitClause(traitTableAlias, traitID, traitCatID, traitTableName, FleshCriteria(c.Criteria, "", traitTableName + ".vchrValue"));
            }
        }

        private void AddTraitClause(string tableAlias, string traitId, string traitCatID, string traitTableName, string criteria) {

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
            var ret = "";
            string strCurrentCriteriaSegment;
            string strOperator;

            var operators = new List<String>();
            var components = new List<String>();

            while (ret == "") {
                GetNextCriteria(ref criteria, out strCurrentCriteriaSegment, out strOperator);
                elemCount++;
                components.Add(strCurrentCriteriaSegment);
                operators.Add(strOperator);

        //        ' If the field is a hierarchical field, look for the 'ONLY' flag and produce the approriate SQL Where component.
        //If LCase(strFullFieldName) = LCase(GetTableAlias(strTABLE_BIOTA) & ".vchrepithet") Or _
        //    LCase(strFullFieldName) = LCase(GetTableAlias(strTABLE_BIOTA) & ".vchrfullname") Or _
        //    LCase(strFullFieldName) = LCase(GetTableAlias(strTABLE_REGION) & ".vchrname") Or _
        //    LCase(strFullFieldName) = LCase(GetTableAlias(strTABLE_BIOTA_STORAGE) & ".vchrname") Then
        //        strComponents(lngElemCount) = BuildHierCriteria(strComponents(lngElemCount), pstrTable, pstrFName)
                
        //ElseIf LCase(strFullFieldName) = LCase(GetTableAlias(strTABLE_DIST_REGION) & ".vchrname") Then
        //    strComponents(lngElemCount) = BuildBiotaDistCriteria(strComponents(lngElemCount))
            
        //ElseIf LCase(Left(LTrim(pstrFName), 3)) = "bit" Then
        //    ' convert true/false to 0/1's for bit fields.
        //    strComponents(lngElemCount) = Replace(strComponents(lngElemCount), "'False'", "0", 1, -1, vbTextCompare)
        //    strComponents(lngElemCount) = Replace(strComponents(lngElemCount), "'True'", "1", 1, -1, vbTextCompare)
        //    strComponents(lngElemCount) = strFullFieldName & " " & strComponents(lngElemCount)
        //Else
        //    strComponents(lngElemCount) = strFullFieldName & " " & strComponents(lngElemCount)
        //End If
            }

            return ret;
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


            //  Extract the not from the front if applicable.
            var strNot = "";

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

            if (strCriteria.StartsWith("like ", StringComparison.CurrentCultureIgnoreCase)) {
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
   
            if (lngNextAndOperator > 0 &&  (lngNextAndOperator < lngNextOrOperator || lngNextOrOperator == 0) && (lngNextAndOperator < lngNextNotOperator || lngNextNotOperator == 0)) {
                return lngNextAndOperator;
            }
        
            if (lngNextNotOperator > 0 &&  (lngNextNotOperator < lngNextOrOperator || lngNextOrOperator == 0) &&  (lngNextNotOperator < lngNextAndOperator || lngNextAndOperator == 0)) {
                return lngNextNotOperator;
            }
            return 0;
        }


        private bool IsTrait(FieldDescriptor f, out string traitID, out string catID, out string tableName) {
            var s = f.TableName.Split('.');
            if (s.Length == 3) {
                traitID = s[0];
                catID = s[1];
                tableName = s[2];
                return true;
            }

            traitID = null;
            catID = null;
            tableName = null;

            return false;
        }

        public static string GenerateSQL(IEnumerable<QueryCriteria> criteria, bool distinct) {
            var instance = new QuerySQLGenerator(criteria, distinct);
            return instance.Generate();
        }
    }

}
