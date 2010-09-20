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

        #endregion
    }

}
