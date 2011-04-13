using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace BioLink.Client.Utilities.tests {

    public class FileSystemTraverserTests {

        [Fact]
        public void TestFilter1() {
            FileSystemTraverser t = new FileSystemTraverser();            
            String testDir = "./tests/testData";
            List<string> filenames = new List<string>();
            t.FilterFiles(testDir, fileinfo => { return true; }, fileinfo => { filenames.Add(fileinfo.FullName); }, false);
            Assert.Equal(3, filenames.Count);
        }

        [Fact]
        public void TestFilter2() {
            FileSystemTraverser t = new FileSystemTraverser();
            String testDir = "./tests/testData";
            List<string> filenames = new List<string>();
            t.FilterFiles(testDir, fileinfo => { return fileinfo.Name.Equals("empty2.txt"); }, fileinfo => { filenames.Add(fileinfo.FullName); }, false);
            Assert.Equal(1, filenames.Count);
        }

    }
}
