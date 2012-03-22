using System;
using System.Collections.Generic;
using BioLink.Data.Model;
using NUnit.Framework;


namespace BioLink.Client.Tools.tests {

    [TestFixture]
    public class LoanFormTests {

        private void testTemplate(String template, String expected) {
            var loan = new Loan {
                OriginatorGivenName = "origGivenName", OriginatorName = "origName", OriginatorTitle = "origTitle", OriginatorPostalAddress = "orig postal address", OriginatorStreetAddress = "origStreetAddress", OriginatorID = -1,
                ReceiverGivenName = "recGivenName", ReceiverName = "recName", ReceiverInstitution = "recInstitution", ReceiverPostalAddress = "rec postal address", ReceiverStreetAddress = "rec street address", ReceiverTitle = "recTitle", ReceiverID = -1,
                RequestorGivenName = "reqGivenName", RequestorName = "reqName", RequestorInstitution = "reqInsititution", RequestorPostalAddress = "req postal address", RequestorStreetAddress = "req street address", RequestorTitle = "req title", RequestorID = -1,
                DateCreated = DateTime.Now, DateClosed = null, DateDue = DateTime.Now, DateInitiated = new DateTime(2001,1,1), LoanClosed = false, LoanID = 1, LoanNumber = "loan number", MethodOfTransfer = "transfer method", PermitNumber = "permit number", Restrictions = "restrictions",
                TypeOfReturn = "type of return"
            };
            var actual = LoanFormGenerator.GenerateLoanForm(template, loan, new List<LoanMaterial>(), new List<Trait>());
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void test1() {
            testTemplate("<<OriginatorGivenName>>", "origGivenName");
        }

        [Test]
        public void test2() {
            testTemplate("<<OriginatorGivenName>>, <<OriginatorName>>", "origGivenName, origName");
        }

        [Test]
        public void test3() {
            testTemplate(@"{rtf1\pard\f0{} <<OriginatorGivenName>>, <<OriginatorName>>}", @"{rtf1\pard\f0{} origGivenName, origName}");
        }

        [Test]
        public void test4() {
            testTemplate(@"{rtf1\pard\f0{} {\rtlch\fcs1 <<Or}{\rtlch\fcs1 iginatorGivenName}>>, <<OriginatorName>>}", @"{rtf1\pard\f0{} {\rtlch\fcs1 }{\rtlch\fcs1 }origGivenName, origName}");
        }

        [Test]
        public void test5() {
            const string template = @"{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 <<Re}{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid16543874 ceiver}{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 Title>>";
            const string expected = @"{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 }{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid16543874 }{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 recTitle";
            testTemplate(template, expected);
        }

        [Test]
        public void test6() {
            const string template = @"<{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 <Re}{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid16543874 ceiver}{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 Title>>";
            const string expected = @"{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 }{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid16543874 }{\rtlch\fcs1 \af0 \ltrch\fcs0 \lang1033\langfe1033\langnp1033\insrsid5601098 recTitle";
            testTemplate(template, expected);
        }

        [Test]
        public void test7() {
            testTemplate(@"<<OriginatorGivenName>{\rtf}>", @"{\rtf}origGivenName");
        }



    }
}
