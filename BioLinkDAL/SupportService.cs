using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using BioLink.Data.Model;
using System.Data.SqlClient;
using System.Reflection;
using System.Data;
using BioLink.Client.Utilities;

namespace BioLink.Data {

    public class SupportService : BioLinkService {

        public static Dictionary<string, RefTypeMapping> RefTypeMap = new Dictionary<string, RefTypeMapping>();

        static SupportService() {
            RefTypeMap["J"] = new RefTypeMapping("J", "Journal");
            RefTypeMap["JS"] = new RefTypeMapping("JS", "Journal Section");
            RefTypeMap["B"] = new RefTypeMapping("B", "Book");
            RefTypeMap["BS"] = new RefTypeMapping("BS", "Book Section");
            RefTypeMap["M"] = new RefTypeMapping("M", "Miscellaneous");
            RefTypeMap["U"] = new RefTypeMapping("U", "Internet URL");
        }

        public SupportService(User user)
            : base(user) {
        }

        #region Traits

        public TraitCategory GetTraitCategory(string category) {
            TraitCategory cat = null;
            int catId = StoredProcReturnVal<int>("spTraitCategoryGetSet", _P("vchrTraitCategory", category), ReturnParam("intTraitCategoryID", SqlDbType.Int));
            cat = new TraitCategory();
            cat.Category = category;
            cat.TraitCategoryID = catId;
            return cat;
        }

        public List<Trait> GetTraits(string category, int intraCategoryID) {
            var mapper = new GenericMapperBuilder<Trait>().Map("Trait", "Name").PostMapAction((t) => {
                t.Category = category;
            }).build();
            return StoredProcToList("spTraitList", mapper, _P("vchrCategory", category), _P("vchrIntraCatID", intraCategoryID + ""));
        }

        public List<String> GetTraitDistinctValues(string traitName, string category) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitDistinctValues", (reader) => {
                results.Add(reader[0] as string);
            }, _P("vchrTraitType", traitName), _P("vchrCategory", category));

            return results;
        }

        public List<String> GetDistinctValues(string table, string field) {

            var results = new List<string>();

            // First we check to see if there is a Phrase Category that matches the concatenation of the table and the field. This is as 
            // per the legacy BioLink application - perhaps to allow installations to restrict otherwise distinct list queries to a constrained vocabulary
            var phraseCat = string.Format("{0}_{1}", table, field);

            var supportService = new SupportService(User);
            int phraseCatId = supportService.GetPhraseCategoryId(phraseCat, false);
            if (phraseCatId > 0) {
                Logger.Debug("Using phrase category {0} (id={1}) for Distinct List lookup", phraseCat, phraseCatId);
                var phrases = supportService.GetPhrases(phraseCatId);
                results = phrases.ConvertAll((phrase) => {
                    return phrase.PhraseText;
                });
            } else {
                Logger.Debug("Selecting distinct values for field {0} from table {1}", field, table);
                StoredProcReaderForEach("spSelectDistinct", (reader) => {
                    results.Add(reader[0] as string);
                }, _P("vchrTableName", table), _P("vchrFieldName", field));
            }

            return results;
        }


        public void DeleteTrait(int traitId) {
            StoredProcUpdate("spTraitDelete", _P("intTraitID", traitId));
        }

        public List<String> GetTraitNamesForCategory(string traitCategory) {
            var results = new List<string>();
            StoredProcReaderForEach("spTraitTypeListForCategory", (reader) => {
                results.Add(reader["Trait"] as string);
            }, _P("vchrCategory", traitCategory));
            return results;
        }

        public int InsertOrUpdateTrait(Trait trait) {
            if (trait.TraitID < 0) {
                var retval = ReturnParam("NewTraitId", SqlDbType.Int);
                StoredProcUpdate("spTraitInsert",
                    _P("vchrCategory", trait.Category),
                    _P("intIntraCatID", trait.IntraCatID),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value ?? ""),
                    _P("vchrComment", trait.Comment ?? ""),
                    retval);
                return (int)retval.Value;
            } else {
                StoredProcUpdate("spTraitUpdate",
                    _P("intTraitID", trait.TraitID),
                    _P("vchrCategory", trait.Category),
                    _P("vchrTrait", trait.Name),
                    _P("vchrValue", trait.Value),
                    _P("vchrComment", trait.Comment));

                return trait.TraitID;
            }
        }

        #endregion

        #region Multimedia

        public List<MultimediaLink> GetMultimediaItems(string category, int intraCatID) {
            var mapper = new GenericMapperBuilder<MultimediaLink>().Map("FileExtension", "Extension").build();
            List<MultimediaLink> ret = StoredProcToList("spMultimediaList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatID));
            return ret;
        }

        public byte[] GetMultimediaBytes(int mediaId) {
            byte[] ret = null;
            StoredProcReaderFirst("spMultimediaGetOne", (reader) => {
                var x = reader.GetSqlBinary(0);
                ret = x.Value;
            }, _P("intMultimediaID", mediaId));
            return ret;
        }

        public List<MultimediaType> GetMultimediaTypes() {
            var mapper = new GenericMapperBuilder<MultimediaType>().Map("MultimediaType", "Name").build();
            return StoredProcToList("spMultimediaTypeList", mapper);
        }

        public Multimedia GetMultimedia(int mediaID) {
            var mapper = new GenericMapperBuilder<Multimedia>().Map("vchrname", "Name").build();
            Multimedia ret = null;
            StoredProcReaderFirst("spMultimediaGet", (reader) => {
                ret = mapper.Map(reader);
                ret.MultimediaID = mediaID;
            }, _P("intMMID", mediaID));
            return ret;
        }

        public void DeleteMultimediaLink(int? multimediaLinkId) {
            StoredProcUpdate("spMultimediaLinkDelete", _P("intMultimediaLinkID", multimediaLinkId.Value));
        }

        public int InsertMultimedia(string name, string extension, byte[] bytes) {
            var retval = ReturnParam("NewMultimediaID", SqlDbType.Int);
            StoredProcUpdate("spMultimediaInsert", _P("vchrName", name), _P("vchrFileExtension", extension), _P("intSizeInBytes", bytes.Length), retval);
            // Now insert the actual bytes...
            UpdateMultimediaBytes((int)retval.Value, bytes);

            return (int)retval.Value;
        }

        public int InsertMultimediaLink(string category, int intraCatID, string multimediaType, int multimediaID, string caption) {
            var retval = ReturnParam("[NewMultimediaLinkID]", SqlDbType.Int);

            if (multimediaType == null) {
                multimediaType = "";
            }

            StoredProcUpdate("spMultimediaLinkInsert",
                _P("vchrCategory", category.ToString()),
                _P("intIntraCatID", intraCatID),
                _P("vchrMultimediaType", multimediaType),
                _P("intMultimediaID", multimediaID),
                _P("vchrCaption", caption),
                retval);

            return (int)retval.Value;
        }

        public void UpdateMultimedia(int multimediaId, string name, string number, string artist, string dateRecorded, string owner, string copyright) {

            StoredProcUpdate("spMultimediaUpdateLong",
                _P("intMultimediaID", multimediaId),
                _P("vchrName", name),
                _P("vchrNumber", number),
                _P("vchrArtist", artist),
                _P("vchrDateRecorded", dateRecorded),
                _P("vchrOwner", owner),
                _P("txtCopyright", copyright)
            );
        }

        public void UpdateMultimediaLink(int multimediaLinkID, string category, string multimediaType, string caption) {
            StoredProcUpdate("spMultimediaLinkUpdate",
                _P("intMultimediaLinkID", multimediaLinkID),
                _P("vchrCategory", category),
                _P("vchrMultimediaType", multimediaType),
                _P("vchrCaption", caption)
            );
        }

        public void UpdateMultimediaBytes(int? multimediaId, byte[] bytes) {
            // Multimedia is the only place where we don't have a stored procedure for the insert/update. This is probably due to a 
            // limitation of ADO.NET or SQL Server back in the 90's or something like that. Either way, we need to insert the actual blob
            // "manually"...
            Command((conn, cmd) => {

                if (User.InTransaction && User.CurrentTransaction != null) {
                    cmd.Transaction = User.CurrentTransaction;
                }

                cmd.CommandText = "UPDATE [tblMultimedia] SET imgMultimedia = @blob, intSizeInBytes=@size WHERE intMultimediaID = @multimediaId";
                cmd.Parameters.Add(_P("blob", bytes));
                cmd.Parameters.Add(_P("size", bytes.Length));
                cmd.Parameters.Add(_P("multimediaId", (int)multimediaId));
                cmd.ExecuteNonQuery();
            });

        }

        public Multimedia FindDuplicateMultimedia(System.IO.FileInfo file, out int sizeInBytes) {
            var name = file.Name;
            if (file.Name.Contains(".")) {
                name = file.Name.Substring(0, file.Name.LastIndexOf("."));
            }
            sizeInBytes = 0;
            var extension = file.Extension.Substring(1);
            // Not that the following service all returns partial results (incomplete stored proc), so a two stage approach is required.
            // First we find candidates based on name.
            var candidates = FindMultimediaByName(name);
            if (candidates.Count > 0) {
                // Look for matching names and extensions...
                foreach (Multimedia candidate in candidates) {
                    if (candidate.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase) && candidate.FileExtension.Equals(extension, StringComparison.CurrentCultureIgnoreCase)) {
                        // Now we do a deeper analysis of each matching candidate, checking the filelength. Theoretically, if we kept a hash on the content in the database
                        // we should compare that, but for now we'll use name and size.
                        sizeInBytes = GetMultimediaSizeInBytes(candidate.MultimediaID);
                        if (sizeInBytes > -1) {
                            if (sizeInBytes == file.Length) {
                                return GetMultimedia(candidate.MultimediaID);
                            }
                        } else {
                            throw new Exception("Failed to get size of multimedia " + candidate.MultimediaID);
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// TODO: should probably be made into a stored proc!
        /// </summary>
        /// <param name="multimediaId"></param>
        /// <returns></returns>
        public int GetMultimediaSizeInBytes(int multimediaId) {
            int result = -1;
            SQLReaderForEach("SELECT DATALENGTH(imgMultimedia) FROM [tblMultimedia] where intMultimediaID = @mmid", (reader) => {
                result = (int)reader[0];
            }, _P("mmid", multimediaId));

            return result;
        }

        public List<Multimedia> FindMultimediaByName(string name) {
            string searchTerm = name.Replace("*", "%") + "%";
            var mapper = new GenericMapperBuilder<Multimedia>().Map("Extension", "FileExtension").build();
            List<Multimedia> results = StoredProcToList("spMultimediaFindByName", mapper, _P("txtSearchTerm", searchTerm));
            return results;
        }

        #endregion

        #region Notes

        public List<Note> GetNotes(string category, int intraCatID) {
            var mapper = new GenericMapperBuilder<Note>().PostMapAction((n) => {
                n.NoteCategory = category;
                n.IntraCatID = intraCatID;
            }).build();

            return StoredProcToList("spNoteList", mapper, _P("vchrCategory", category), _P("intIntraCatID", intraCatID));
        }

        public List<string> GetNoteTypesForCategory(string categoryName) {
            var results = new List<string>();
            StoredProcReaderForEach("spNoteTypeListForCategory", (reader) => {
                results.Add(reader["Note"] as string);
            }, _P("vchrCategory", categoryName));
            return results;
        }

        public void DeleteNote(int noteID) {
            StoredProcUpdate("spNoteDelete", _P("intNoteID", noteID));
        }

        public int InsertNote(string category, int intraCatID, string noteType, string note, string author, string comments, bool useInReports, int refID, string refPages) {
            var retval = ReturnParam("NewNoteID", SqlDbType.Int);
            StoredProcUpdate("spNoteInsert",
                _P("vchrCategory", category),
                _P("intIntraCatID", intraCatID),
                _P("vchrNoteType", noteType),
                _P("txtNote", note),
                _P("vchrAuthor", author),
                _P("txtComments", comments),
                _P("bitUseInReports", useInReports),
                _P("intRefID", refID),
                _P("vchrRefPages", refPages),
                retval);

            return (int)retval.Value;
        }

        public void UpdateNote(int noteID, string category, string noteType, string note, string author, string comments, bool useInReports, int refID, string refPages) {
            StoredProcUpdate("spNoteUpdate",
                _P("intNoteID", noteID),
                _P("vchrCategory", category),
                _P("vchrNoteType", noteType),
                _P("txtNote", note),
                _P("vchrAuthor", author),
                _P("txtComments", comments),
                _P("bitUseInReports", useInReports),
                _P("intRefID", refID),
                _P("vchrRefPages", refPages)
            );
        }

        #endregion        

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

        public Reference GetReference(int refID) {
            var mapper = new GenericMapperBuilder<Reference>().build();
            Reference ret = null;
            StoredProcReaderFirst("spReferenceList", (reader) => {
                ret = mapper.Map(reader);
            }, _P("vchrRefIDList", refID + ""));
            return ret;
        }

        public void DeleteReference(int refID) {
            StoredProcUpdate("spReferenceDelete", _P("intRefID", refID));
        }

        public int InsertReference(Reference r) {
            var retval = ReturnParam("NewRefID", SqlDbType.Int);
            StoredProcUpdate("spReferenceInsert",
                _P("vchrRefCode", r.RefCode),
			    _P("vchrAuthor", r.Author),
			    _P("vchrTitle", r.Title),
			    _P("vchrBookTitle", r.BookTitle),
			    _P("vchrEditor", r.Editor),
			    _P("vchrRefType", r.RefType),
			    _P("vchrYearOfPub", r.YearOfPub),
			    _P("vchrActualDate", r.ActualDate),
			    _P("intJournalID", r.JournalID),
			    _P("vchrPartNo", r.PartNo),
			    _P("vchrSeries", r.Series),
			    _P("vchrPublisher", r.Publisher),
			    _P("vchrPlace", r.Place),
			    _P("vchrVolume", r.Volume),
			    _P("vchrPages", r.Pages),
			    _P("vchrTotalPages", r.TotalPages),
			    _P("vchrPossess", r.Possess),
			    _P("vchrSource", r.Source),
			    _P("vchrEdition", r.Edition),
			    _P("vchrISBN", r.ISBN),
			    _P("vchrISSN", r.ISSN),
			    _P("txtAbstract", r.Abstract),
			    _P("txtFullText", r.FullText),
			    _P("txtFullRTF", r.FullRTF),
			    _P("intStartPage", r.StartPage),
			    _P("intEndPage", r.EndPage),
                retval
            );
            
            return (int) retval.Value;
        }

        public void UpdateReference(Reference r) {
            StoredProcUpdate("spReferenceUpdate",
                _P("intRefID", r.RefID),
                _P("vchrRefCode", r.RefCode),
                _P("vchrAuthor", r.Author),
                _P("vchrTitle", r.Title),
                _P("vchrBookTitle", r.BookTitle),
                _P("vchrEditor", r.Editor),
                _P("vchrRefType", r.RefType),
                _P("vchrYearOfPub", r.YearOfPub),
                _P("vchrActualDate", r.ActualDate),
                _P("intJournalID", r.JournalID),
                _P("vchrPartNo", r.PartNo),
                _P("vchrSeries", r.Series),
                _P("vchrPublisher", r.Publisher),
                _P("vchrPlace", r.Place),
                _P("vchrVolume", r.Volume),
                _P("vchrPages", r.Pages),
                _P("vchrTotalPages", r.TotalPages),
                _P("vchrPossess", r.Possess),
                _P("vchrSource", r.Source),
                _P("vchrEdition", r.Edition),
                _P("vchrISBN", r.ISBN),
                _P("vchrISSN", r.ISSN),
                _P("txtAbstract", r.Abstract),
                _P("txtFullText", r.FullText),
                _P("txtFullRTF", r.FullRTF),
                _P("intStartPage", r.StartPage),
                _P("intEndPage", r.EndPage));

        }

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

        public NewAutoNumber GetNextAutoNumber(int autoNumberCatID, int seed, bool ensureUnique, string table, string field) {
            NewAutoNumber result = null;
            var mapper = new GenericMapperBuilder<NewAutoNumber>().build();
            bool finished = false;
            do {
                StoredProcReaderFirst("spAutoNumberGetNext", (reader) => {
                    result = mapper.Map(reader);
                    result.AutoNumberCatID = autoNumberCatID;
                },
                _P("intAutoNumberCatID", autoNumberCatID),
                _P("intSeed", seed));

                if (ensureUnique) {
                    finished = CheckAutoNumberUnique(result.FormattedNumber, table, field);
                    if (!finished) {
                        seed = -1;
                    }
                } 
            } while (!finished && ensureUnique);

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
            return retval == 1;
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

    public class RefTypeMapping {

        public RefTypeMapping(string code, string name) {
            this.RefTypeCode = code;
            this.RefTypeName = name;
        }

        public string RefTypeCode { get; set; }
        public string RefTypeName { get; set; }
    }

}
