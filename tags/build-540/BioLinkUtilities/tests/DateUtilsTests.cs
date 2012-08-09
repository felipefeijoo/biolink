using System;
using NUnit.Framework;

namespace BioLink.Client.Utilities.tests {
    [TestFixture]
    public class DateUtilsTests {

        [Test]
        public void BLDateComponents1() {
            int day, month, year;
            var result = DateUtils.BLDateComponents("20110215", out day, out month, out year);
            Assert.AreEqual(result, true);
            Assert.AreEqual(day,15);
            Assert.AreEqual(month, 2);
            Assert.AreEqual(year, 2011);
        }

        [Test]
        public void BLDateComponents2() {
            int day, month, year;
            var result = DateUtils.BLDateComponents("xxx", out day, out month, out year);
            Assert.AreEqual(result, false);
        }

        [Test]
        public void BLDateComponents3() {
            int day, month, year;
            var result = DateUtils.BLDateComponents("20110300", out day, out month, out year);
            Assert.AreEqual(result, true);
            Assert.AreEqual(day, 0);
            Assert.AreEqual(month, 3);
            Assert.AreEqual(year, 2011);
        }

        [Test]
        public void BLDateComponents4() {
            int day, month, year;
            var result = DateUtils.BLDateComponents("00000000", out day, out month, out year);
            Assert.AreEqual(result, true);
            Assert.AreEqual(day, 0);
            Assert.AreEqual(month, 0);
            Assert.AreEqual(year, 0);
        }

        [Test]
        public void BLDateToStr1() {
            var result = DateUtils.BLDateToStr(20110101);
            Assert.AreEqual(result, "1 Jan, 2011");
        }

        [Test]
        public void BLDateToStr2() {
            var result = DateUtils.BLDateToStr(20110100);
            Assert.AreEqual(result, "Jan, 2011");
        }

        [Test]
        public void BLDateToStr3() {
            var result = DateUtils.BLDateToStr(20110000);
            Assert.AreEqual(result, "2011");
        }

        [Test]
        public void BLDateToStr4() {
            var result = DateUtils.BLDateToStr(0);
            Assert.AreEqual(result, "");
        }

        [Test]
        public void BLDateToStr5() {
            var result = DateUtils.BLDateToStr(-1);
            Assert.AreEqual(result, "");
        }

        [Test]
        public void DateRomanMonth1() {
            var result = DateUtils.DateRomanMonth(20110101);
            Assert.AreEqual("1.i.2011", result);
        }

        [Test]
        public void DateRomanMonth2() {
            var result = DateUtils.DateRomanMonth(20110800);
            Assert.AreEqual("viii.2011", result);
        }

        [Test]
        public void DateRomanMonth3() {
            var result = DateUtils.DateRomanMonth(20110000);
            Assert.AreEqual("2011", result);
        }

        [Test]
        public void DateStrToBLDate1() {
            var result = DateUtils.DateStrToBLDate("1 Jan, 2011");
            Assert.AreEqual("20110101", result);
        }

        [Test]
        public void DateStrToBLDate2() {
            var result = DateUtils.DateStrToBLDate("April 2011");
            Assert.AreEqual("20110401", result);
        }

        [Test]
        public void DateToBLDate1() {
            var result = DateUtils.DateToBLDate(new DateTime(2001, 7, 23));
            Assert.AreEqual("20010723", result);
        }

        [Test]
        public void FormatDate1() {
            var result = DateUtils.FormatDate(19920911);
            Assert.AreEqual("11 Sep, 1992", result);
        }

        [Test]
        public void FormatDate2() {
            var result = DateUtils.FormatDate(19731000);
            Assert.AreEqual("Oct, 1973", result);
        }

        [Test]
        public void FormatDate3() {
            var result = DateUtils.FormatDate(16810000);
            Assert.AreEqual("1681", result);
        }

        [Test]
        public void FormatBLDate1() {
            var result = DateUtils.FormatBLDate("d MMM, yyyy", 15, 9, 1997);
            Assert.AreEqual("15 Sep, 1997", result);
        }

        [Test]
        public void FormatBLDate2() {
            var result = DateUtils.FormatBLDate("yyyy-MM-dd", 10, 3, 2002);
            Assert.AreEqual("2002-03-10", result);
        }

        [Test]
        public void FormatBLDate3() {
            var result = DateUtils.FormatBLDate("yyyy-MM-dd", 0, 3, 2002);
            Assert.AreEqual("2002-03-", result);
        }

        [Test]
        public void FormatBLDate4() {
            var result = DateUtils.FormatBLDate("yyyy-MM-dd", 0, 0, 2002);
            Assert.AreEqual("2002- -", result);
        }

        [Test]
        public void FormatBLDate5() {
            var result = DateUtils.FormatBLDate("d MMM, yyyy", 0, 3, 2002);
            Assert.AreEqual("Mar, 2002", result);
        }

        [Test]
        public void FormatBLDate6() {
            var result = DateUtils.FormatBLDate("dd.R.yyyy", 12, 3, 2002);
            Assert.AreEqual("12.iii.2002", result);
        }



    }
}
