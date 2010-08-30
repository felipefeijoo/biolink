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

        public List<PhraseCategory> GetPhraseCategories() {
            var mapper = new GenericMapperBuilder<PhraseCategory>().Map("Phrase", "PhraseText").build();
            return StoredProcToList<PhraseCategory>("spPhraseCategoryList", mapper);
        }

    }

}
