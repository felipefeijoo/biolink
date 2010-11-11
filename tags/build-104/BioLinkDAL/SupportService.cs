using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BioLink.Data.Model;
using System.Data.SqlClient;
using System.Reflection;

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

        #region Favorites

        public List<T> GetTopFavorites<T>(FavoriteType type, bool global, GenericMapper<T> mapper) where T : Favorite, new() {
            var results = new List<T>();            
            StoredProcReaderForEach("spFavoriteListTop", (reader) => {
                var model = mapper.Map(reader);
                model.FavoriteType = type;
                results.Add(model);
            },
            _P("vchrType", type.ToString()),
            _P("bitListGlobal", global));
            return results;
        }

        public List<T> GetFavorites<T>(FavoriteType type, bool global, int parentFavoriteId, GenericMapper<T> mapper) where T : Favorite, new() {
            var results = new List<T>();
            StoredProcReaderForEach("spFavoriteList", (reader) => {
                var model = mapper.Map(reader);
                model.FavoriteType = type;
                results.Add(model);
            },
            _P("vchrType", type.ToString()),
            _P("intParentID", parentFavoriteId),
            _P("bitListGlobal", global));
            return results;
        }

        /// <summary>
        /// <para>Crikey!</para>
        /// <para>
        /// Ok, this function configures a mapper appropriate for the type (T) of favorite being retrieved. The mapping for favorites is complicated because:
        /// </para>
        /// <list type="bullet">
        /// <item><description>the columns in the favorites table do not line up with the columns retrieved by the stored procedure</description></item>
        /// <item><description>Each different type of favorite will yield a different set of columns</description></item>
        /// <item><description>Only a small set of columns are common across favorite, and not all desired common columns are desired (ID1 and ID2, for example).</description></item>
        /// </list>
        /// 
        /// <para>
        /// Of particular note the ID1 and ID2 columns. These columns hold the ID of the noun being held as a favorite. In the case of Taxa items ID1 is the taxon ID, and ID2 is unused
        /// For Site favorites (which can include material, visits, regions etc), ID1 holds the id of the database object, and ID2 holds a string describing what kind of object it is ("Site", for example).
        /// </para>
        /// <para>
        /// ID1 and ID2, therefore, are pretty important, but the stored proc doesn't return them explicitly, but rather returns their values in a "strongly" named column specific to each ID type (intTaxaID, for example).
        /// This is where the funky "Expression&lt;Func&lt;typeparamref name=",int&gt;&gt; params come in. These expressions describe which property of the strong type Favorite type (T) should be bound to ID1 and ID2.
        /// </para>
        /// <para>
        /// For example, in the case of Taxon favorites ID1 should be mapped to TaxonFavorite.TaxaID and ID2 is ignored.
        /// </para>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="isGlobal"></param>
        /// <param name="ID1Expr"></param>
        /// <param name="ID2Expr"></param>
        /// <returns></returns>
        internal GenericMapper<T> ConfigureFavoriteMapper<T>(GenericMapperBuilder<T> builder, bool isGlobal, Expression<Func<T, int>> ID1Expr, Expression<Func<T, string>> ID2Expr) where T : Favorite, new() {

            builder.Map("ElementIsGroup", "IsGroup");

            // ElementIsGroup comes back as a byte value, and needs to be coerced to a boolean...
            var cm = new ConvertingMapper("ElementIsGroup", new Converter<object, object>((o) => {
                if (o is byte) {
                    var b = (byte)o;
                    return b != 0;
                }
                return null;
            }));

            builder.PostMapAction((favorite) => {
                favorite.IsGlobal = isGlobal;

                // Copy over the nominated value for ID1
                if (ID1Expr != null) {
                    var srcProp = (PropertyInfo)((MemberExpression)ID1Expr.Body).Member;
                    favorite.ID1 = (int) srcProp.GetValue(favorite, null);
                }

                // Copy over the nominated value for ID1
                if (ID2Expr != null) {
                    var srcProp = (PropertyInfo)((MemberExpression)ID1Expr.Body).Member;
                    favorite.ID2 = (string) srcProp.GetValue(favorite, null);
                }

            });

            builder.Override(cm);
            return builder.build();
        }

        public List<TaxonFavorite> GetTopTaxaFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<TaxonFavorite>(), global, fav => fav.TaxaID, null);
            return GetTopFavorites<TaxonFavorite>(FavoriteType.Taxa, global, mapper);
        }

        public List<TaxonFavorite> GetTaxaFavorites(int parentFavoriteId, bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<TaxonFavorite>(), global, fav => fav.TaxaID, null);
            return GetFavorites<TaxonFavorite>(FavoriteType.Taxa, global, parentFavoriteId, mapper);
        }

        public List<SiteFavorite> GetTopSiteFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<SiteFavorite>(), global, fav => fav.ElemID, fav => fav.ElemType);
            return GetTopFavorites<SiteFavorite>(FavoriteType.Site, global, mapper);
        }

        public List<ReferenceFavorite> GetTopReferenceFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<ReferenceFavorite>(), global, fav => fav.RefID, null);
            return GetTopFavorites<ReferenceFavorite>(FavoriteType.Reference, global, mapper);
        }

        public List<DistRegionFavorite> GetTopDistRegionFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<DistRegionFavorite>(), global, fav => fav.DistRegionID, null);
            return GetTopFavorites<DistRegionFavorite>(FavoriteType.DistRegion, global, mapper);
        }

        public List<BiotaStorageFavorite> GetTopBiotaStorageFavorites(bool global) {
            var mapper = ConfigureFavoriteMapper(new GenericMapperBuilder<BiotaStorageFavorite>(), global, fav=>fav.BiotaStorageID, null);
            return GetTopFavorites<BiotaStorageFavorite>(FavoriteType.BiotaStorage, global, mapper);
        }

        public void DeleteFavorite(int favoriteID) {
            StoredProcUpdate("spFavoriteDelete", _P("intFavoriteID", favoriteID));
        }

        public int InsertFavoriteGroup(FavoriteType favType, int parentID, string name, bool global) {
            var retval = ReturnParam("intNewFavoriteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spFavoriteInsert",
                _P("vchrType", favType.ToString()),
                _P("intParentID", parentID),
                _P("tintGroup", 1),
                _P("vchrGroupName", name),
                _P("intID1", 0),
                _P("vchrID2", ""),
                _P("bitGlobal", global),
                retval
            );

            return (int) retval.Value;
        }

        public void RenameFavoriteGroup(int favoriteID, string newName) {
            StoredProcUpdate("spFavoriteRename",
                _P("intFavoriteID", favoriteID),
                _P("vchrName", newName)
            );
        }

        public void MoveFavorite(int favoriteID, int newParentID) {
            StoredProcUpdate("spFavoriteMove",
                _P("intFavoriteID", favoriteID),
                _P("intNewParentID", newParentID)
            );
        }

        public int InsertFavorite(FavoriteType favType, int parentID, int id1, string id2, bool global) {
            var retval = ReturnParam("intNewFavoriteID", System.Data.SqlDbType.Int);
            StoredProcUpdate("spFavoriteInsert",
                _P("vchrType", favType.ToString()),
                _P("intParentID", parentID),
                _P("tintGroup", 0),
                _P("vchrGroupName", null),
                _P("intID1", id1),
                _P("vchrID2", id2),
                _P("bitGlobal", global),
                retval
            );

            return (int)retval.Value;
        }

        #endregion

    }

}
