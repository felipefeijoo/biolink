using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BioLink.Data.Model;
using NUnit.Framework;

namespace BioLink.Data.tests {
    [TestFixture]
    public class QueryTests : DatabaseTestBase {

        const string testQueriesDir = "./tests/testQueries";

        private List<FieldDescriptor> _fields;

        protected User User { get; private set; }

        [SetUp]
        public void Setup() {
            User = connect();
            _fields = Service.GetFieldMappings();
        }

        protected SupportService Service { 
            get { return new SupportService(User); }
        }

        [Test]
        public void TestQuery1() {
            var queryFile = Path.Combine(testQueriesDir, "query1.blq");
            var criteria = Service.LoadQueryFile(queryFile);
            Service.ExecuteQuery(criteria, false);
        }

    }
}
