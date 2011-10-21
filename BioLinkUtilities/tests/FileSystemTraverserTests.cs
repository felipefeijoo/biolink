using System.Collections.Generic;
using NUnit.Framework;

namespace BioLink.Client.Utilities.tests {

    [TestFixture]
    public class FileSystemTraverserTests {
        
        [Test]
        public void TestFilter1() {
            var t = new FileSystemTraverser();            
            const string testDir = "./tests/testData";
            var filenames = new List<string>();
            t.FilterFiles(testDir, fileinfo => true, fileinfo => filenames.Add(fileinfo.FullName), false);
            Assert.AreEqual(3, filenames.Count);
        }
        
        [Test]
        public void TestFilter2() {
            var t = new FileSystemTraverser();
            const string testDir = "./tests/testData";
            var filenames = new List<string>();
            t.FilterFiles(testDir, fileinfo => fileinfo.Name.Equals("empty2.txt"), fileinfo => filenames.Add(fileinfo.FullName), false);
            Assert.AreEqual(1, filenames.Count);
        }

    }
}
