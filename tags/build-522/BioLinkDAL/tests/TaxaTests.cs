using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using NUnit.Framework;

namespace BioLink.Data.tests {

    [TestFixture]
    public class TaxaTests : DatabaseTestBase {

        protected User User { get; private set; }

        protected TaxaService Service { 
            get { return new TaxaService(User); } 
        }

        [SetUp]
        public void Setup() {
            User = connect();
        }

        [Test]
        public void TestTopLevelTaxa() {           
            var taxa = Service.GetTopLevelTaxa();
            Assert.AreEqual(taxa.Count, 1);
            Assert.AreEqual(taxa[0].TaxaFullName, "Animalia");
        }

        [Test]
        public void TestAddTaxa() {
            // First find a root to start from...                        
            var parent = Service.GetTaxon(7009); // Insecta
            Assert.NotNull(parent);
            // Create a new family...
            var taxon = new Taxon { TaxaParentID = parent.TaxaID, Author = "Smith",  YearOfPub = "1990", AvailableName = false, ChgComb = false, Epithet = "Test Taxa", ElemType = "F", KingdomCode = "A", Unplaced = false, Unverified = false, LiteratureName = false, Order = 0 };
            // insert it...
            Service.InsertTaxon(taxon);
            Assert.IsTrue(taxon.TaxaID.HasValue);
            Trace("Inserted TaxaID: {0}", taxon.TaxaID);

            var other = Service.GetTaxon(taxon.TaxaID.Value);
            AssertObjectsEqual(other, taxon, "Rank", "NameStatus", "Parentage", "Shadowed", "RankLong", "KingdomLong", "RankCategory", "DateCreated", "WhoCreated", "DateModified", "WhoModified", "GUID", "ObjectID");
        }

    }
}
