using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using System.Data.SqlClient;

namespace BioLink.Data {

    public class SupportService : BioLinkService {

        public SupportService(User user)
            : base(user) {
        }

        #region Phrases

        public List<PhraseCategory> GetPhraseCategories() {
            var mapper = new GenericMapperBuilder<PhraseCategory>().build();
            return StoredProcToList<PhraseCategory>("spPhraseCategoryList", mapper);
        }

        public int GetPhraseCategoryId(string categoryName, bool @fixed) {
            return StoredProcReturnVal<int>("spPhraseCategoryGetID", new SqlParameter("vchrCategory", categoryName), new SqlParameter("bitFixed", @fixed));
        }

        public List<Phrase> GetPhrases(int categoryId) {
            var mapper = new GenericMapperBuilder<Phrase>().Map("Phrase", "PhraseText").build();
            return StoredProcToList<Phrase>("spPhraseList", mapper, new SqlParameter("intCategory", categoryId));
        }

        public void AddPhrase(Phrase phrase) {
            // Obviously a copy-pasta error in the Stored Proc, as the return value is called NewRegionID...oh well...
            SqlParameter retval = ReturnParam("NewRegionID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spPhraseInsert", _P("intPhraseCatID", phrase.PhraseCatID), _P("vchrPhrase", phrase.PhraseText), retval);
            if (retval != null && retval.Value != null) {
                phrase.PhraseID = (Int32)retval.Value;
            }
        }

        public void RenamePhrase(int phraseId, string phrase) {
            StoredProcUpdate("spPhraseRename", _P("intPhraseID", phraseId), _P("vchrPhrase", phrase)); 
        }

        public void DeletePhrase(int phraseId) {
            StoredProcUpdate("spPhraseDelete", _P("intPhraseID", phraseId));
        }

        public void DeletePhraseCategory(int categoryId) {
            StoredProcUpdate("spPhraseCategoryDelete", _P("intCatID", categoryId));
        }

        #endregion

        #region References

        public List<ReferenceSearchResult> FindReferences(string refCode, string author, string year, string other) {
            var mapper = new GenericMapperBuilder<ReferenceSearchResult>().Map("FullRTF","RefRTF").build();
            return StoredProcToList("spReferenceFind", mapper,
                _P("vchrRefCode", refCode, DBNull.Value),
                _P("vchrAuthor", author, DBNull.Value),
                _P("vchrYear", year, DBNull.Value),
                _P("vchrOther", other, DBNull.Value));
        }

        public List<RefLink> GetReferenceLinks(string categoryName, int intraCatID) {
            var mapper = new GenericMapperBuilder<RefLink>().Map("intCatID", "CategoryID").Map("RefLink", "RefLinkType").build();
            return StoredProcToList("spRefLinkList", mapper,
                _P("vchrCategory", categoryName),
                _P("intIntraCatID", intraCatID));
        }

        public void UpdateRefLink(RefLink link, string categoryName) {
            StoredProcUpdate("spRefLinkUpdate",
                _P("intRefLinkID", link.RefLinkID),
                _P("vchrRefLink", link.RefLinkType),
                _P("vchrCategory", categoryName),
                _P("intRefID", link.RefID),
                _P("vchrRefPage", link.RefPage),
                _P("txtRefQual", link.RefQual),
                _P("intOrder", link.Order),
                _P("bitUseInReport", link.UseInReport));
        }

        public void InsertRefLink(RefLink link, string categoryName, int intraCatID) {
            var retval = ReturnParam("RetVal", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRefLinkInsert",
                _P("vchrCategory", categoryName),
                _P("intIntraCatID", intraCatID),                
                _P("vchrRefLink", link.RefLinkType),                
                _P("intRefID", link.RefID),
                _P("vchrRefPage", link.RefPage),
                _P("txtRefQual", link.RefQual),
                _P("intOrder", link.Order),
                _P("bitUseInReport", link.UseInReport),
                retval);
            link.RefLinkID = (int)retval.Value;
        }

        public void DeleteRefLink(int refLinkID) {
            StoredProcUpdate("spRefLinkDelete", _P("intRefLinkID", refLinkID));
        }

        public IEnumerable<string> GetRefLinkTypes() {
            var list = new List<String>();
            StoredProcReaderForEach("spRefLinkTypeList", (reader) => {
                list.Add(reader["RefLink"] as string);
            });
            return list;
        }

        public int InsertRefLinkType(string linkType, string categoryName) {
            var retval = ReturnParam("intRefLinkTypeID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spRefLinkTypeGetSet",
                _P("vchrRefLinkType", linkType),
                _P("vchrCategory", categoryName),
                _P("intRefLinkTypeID", -1),
                retval);
            return (int)retval.Value;
        }

        #endregion

        #region AutoNumbers

        public List<AutoNumber> GetAutoNumbersForCategory(string category) {
            var mapper = new GenericMapperBuilder<AutoNumber>().build();
            return StoredProcToList("spAutoNumberCatList", mapper, _P("vchrCategory", category));
        }

        public NewAutoNumber GetNextAutoNumber(int autoNumberCatID, int seed) {
            NewAutoNumber result = null;
            var mapper = new GenericMapperBuilder<NewAutoNumber>().build();
            StoredProcReaderFirst("spAutoNumberGetNext", (reader) => {
                result = mapper.Map(reader);
                result.AutoNumberCatID = autoNumberCatID;
            },
            _P("intAutoNumberCatID", autoNumberCatID),
            _P("intSeed", seed));

            return result;
        }

        public int InsertAutoNumber(string category, string name, string prefix, string postfix, int numLeadingZeros, bool ensureUnique) {
            var retval = ReturnParam("identity", System.Data.SqlDbType.Int);
            StoredProcUpdate("spAutoNumberInsert",
                _P("vchrCategory", category),
                _P("vchrName", name),
                _P("vchrPrefix", prefix),
                _P("vchrPostfix", postfix),
                _P("intNumLeadingZeros", numLeadingZeros),
                _P("bitEnsureUnique", ensureUnique),
                retval);

            return (int) retval.Value;
        }

        public bool CheckAutoNumberUnique(string number, string table, string field) {
            int retval = StoredProcReturnVal<int>("spAutoNumberEnsureUnique", 
                _P("vchrNumber", number),
                _P("vchrFromTable", table),
                _P("vchrFieldName", field)
            );
            return retval != 0;
        }

        public void UpdateAutoNumber(int autoNumberCatID, string name, string prefix, string postfix, int numLeadingZeros, bool ensureUnique) {
            StoredProcUpdate("spAutoNumberUpdate",
                _P("intAutoNumberCatID", autoNumberCatID),
                _P("vchrName", name),
                _P("vchrPrefix", prefix),
                _P("vchrPostfix", postfix),
                _P("intNumLeadingZeros", numLeadingZeros),
                _P("bitEnsureUnique", ensureUnique)
            );
        }

        public void DeleteAutoNumberCategory(int autoNumberCatID) {
            StoredProcUpdate("spAutoNumberDelete", _P("intAutoNumberCatID", autoNumberCatID));
        }

        #endregion
    }

}
