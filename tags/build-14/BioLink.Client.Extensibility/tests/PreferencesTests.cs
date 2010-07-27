using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Newtonsoft.Json;

namespace BioLink.Client.Extensibility {
    
    public class PreferencesTests : IDisposable {

        private static PreferenceStore _prefs;

        static PreferencesTests() {
            _prefs = new PreferenceStore("preferences.tests.prefs");
            _prefs.ResetPreferences();
        }

        [Fact]
        public void TestSet() {            
            string expected = "test.value";
            string key = "test.key";
            Preferences.SetGlobal(key, expected);
            string actual = Preferences.GetPreference(key);
            Assert.Equal(expected, actual);            
        }

        [Fact]
        public void testInt1() {
            Preferences.SetGlobal("int.key1", "100");
            Int32 i = Preferences.GetGlobal("int.key1", 0);
            Assert.Equal((Int32)100, i);
        }

        [Fact]
        public void testInt2() {
            Preferences.SetGlobal("int.key2", "1001");
            int i = Preferences.GetGlobal("int.key2", 0);
            Assert.Equal(1001, i);
        }

        [Fact]
        public void testBool() {
            Preferences.SetGlobal("bool.key", "true");
            Boolean b = Preferences.GetGlobal("bool.key", false);
            Assert.True(b);
        }

        [Fact]
        public void testLong() {
            Preferences.SetGlobal("long.key", "10000");
            long b = Preferences.GetGlobal("long.key", 1);
            Assert.Equal(b, 10000);
        }

        [Fact]
        public void testInt64() {
            Preferences.SetGlobal("int64.key", Int64.MaxValue.ToString());
            Int64 b = Preferences.GetGlobal("int64.key", (Int64) 1);
            Assert.Equal(b, Int64.MaxValue);
        }

        [Fact]
        public void testDate() {
            DateTime expected = new DateTime(1988, 1, 1);
            Preferences.SetGlobal("date.key", expected);
            DateTime actual = Preferences.GetGlobal("date.key", new DateTime());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void testManyInserts() {
            for (int i = 0; i < 100; ++i) {
                Preferences.SetGlobal("test.many.key", i);
            }
        }

        [Fact]
        public void testObject() {
            TestObject expected = new TestObject();
            expected.Name = "Test Name";
            expected.Age = 32;
            expected.Date = DateTime.Now;
            Preferences.SetGlobal("test.object", expected);
            TestObject actual = Preferences.GetGlobal<TestObject>("test.object", null);
            Assert.Equal(expected, actual);
        }

        public void Dispose() {
            _prefs.Traverse((key, value) => System.Console.WriteLine("Key: {0} Value: {1}", key, value));            
        }
    }

    public class TestObject {

        public String Name { get; set; }        
        public int Age { get; set; }
        public DateTime Date { get; set; }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is TestObject) {
                TestObject other = obj as TestObject;
                if (!Date.ToShortDateString().Equals(other.Date.ToShortDateString())) {                
                    return false;
                }
                if (!Date.ToShortTimeString().Equals(other.Date.ToShortTimeString())) {                
                    return false;
                }
                if (!this.Age.Equals(other.Age)) {
                    return false;
                }

                if (!this.Name.Equals(other.Name)) {
                    return false;
                }

                return true;
            }
            return false;
        }
    }
}
