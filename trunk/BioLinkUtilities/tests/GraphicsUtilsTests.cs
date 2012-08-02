using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace BioLink.Client.Utilities.tests {

    [TestFixture]
    class GraphicsUtilsTests {

        [Test]
        public void TestFontConversion1() {
            var pixels = GraphicsUtils.PointsToPixel(null, 10);
            Assert.AreEqual(13.333, pixels, 0.001);
        }

        [Test]
        public void TestFontConversion2() {
            var points = GraphicsUtils.PixelsToPoints(null, 13.33333);
            Assert.AreEqual(10, points, 0.001);
        }

    }
}
