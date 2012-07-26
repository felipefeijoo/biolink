using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using BioLink.Client.Utilities;


namespace BioLink.Client.Utilities.tests {

    [TestFixture]
    public class ByteConverterTests {

        [Test]
        public void Test1() {
            var result = ByteLengthConverter.FormatBytes(1024);
            Assert.AreEqual(result, "1024 Bytes");
        }

        [Test]
        public void Test2() {
            var result = ByteLengthConverter.FormatBytes(1025);
            Assert.AreEqual(result, "1 KB");
        }

        [Test]
        public void Test3() {
            var result = ByteLengthConverter.FormatBytes(512);
            Assert.AreEqual(result, "512 Bytes");
        }

        [Test]
        public void Test4() {
            var result = ByteLengthConverter.FormatBytes(2048);
            Assert.AreEqual(result, "2 KB");
        }

        [Test]
        public void Test5() {
            var result = ByteLengthConverter.FormatBytes(1024 * 1024);
            Assert.AreEqual(result, "1024 KB");
        }

        [Test]
        public void Test6() {
            var result = ByteLengthConverter.FormatBytes(1024 * 1024 + 1);
            Trace.WriteLine(result);
            Assert.AreEqual(result, "1 MB");
        }

    }
}
