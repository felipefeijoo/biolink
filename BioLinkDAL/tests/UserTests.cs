using System.Collections.Generic;
using BioLink.Data.Model;
using NUnit.Framework;

namespace BioLink.Data.tests {

    /// <summary>
    /// You'll need to change these to suit your own database/development environment
    /// </summary>
    public static class TestProperties {

        public const bool RunDatabaseTests = false;

        public const string Server = @"OCALA-BE\SQLEXPRESS";
        public const string Username = "sa";
        public const string Password = "biolink";
        public const string Database = "BiolinkStarter";

    }

    public abstract class DatabaseTestBase {

        protected User connect() {

// ReSharper disable ConditionIsAlwaysTrueOrFalse
// ReSharper disable CSharpWarnings::CS0162
// ReSharper disable HeuristicUnreachableCode
            if (!TestProperties.RunDatabaseTests) {
                Assert.Ignore();
            }
// ReSharper restore HeuristicUnreachableCode
// ReSharper restore CSharpWarnings::CS0162
// ReSharper restore ConditionIsAlwaysTrueOrFalse

            var profile = new ConnectionProfile { Server = TestProperties.Server, Database = TestProperties.Database, IntegratedSecurity = false };
            var user = new User(TestProperties.Username, TestProperties.Password, profile);
            string message;
            if (!user.Authenticate(out message)) {
                Assert.Fail("Failed to authenticate: " + message);
            }
            return user;
        }

        protected void Trace(string format, params object[] args) {
            System.Diagnostics.Trace.WriteLine(string.Format(format, args));
        }

        protected void AssertObjectsEqual(object actual, object expected, params string[] ignore) {
            var properties = expected.GetType().GetProperties();
            var ignores = new List<string>(ignore);
            foreach (var property in properties) {
                if (!ignores.Contains(property.Name)) {
                    var expectedValue = property.GetValue(expected, null);
                    var actualValue = property.GetValue(actual, null);                
                    if (!Equals(expectedValue, actualValue)) {
                        Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}", property.DeclaringType.Name, property.Name, expectedValue ?? "<null>", actualValue ?? "<null>");
                    }
                }
            }        
        }

    }


    [TestFixture]
    public class UserTests : DatabaseTestBase {

        [Test]
        public void TestConnect1() {
            var user = connect();
            Assert.NotNull(user);
        }

    }
}
