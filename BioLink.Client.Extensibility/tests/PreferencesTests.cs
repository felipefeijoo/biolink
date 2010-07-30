using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Newtonsoft.Json;
using BioLink.Data;

namespace BioLink.Client.Extensibility {
    
    public class PreferencesTests : IDisposable {

        private static ConfigurationStore _prefs;

        static PreferencesTests() {
            _prefs = new ConfigurationStore("preferences.tests.prefs");
            _prefs.ResetPreferences();
        }

        [Fact]
        public void TestSet() {            
            string expected = "test.value";
            string key = "test.key";
            Config.SetGlobal(key, expected);
            string actual = Config.GetGlobal(key, "");
            Assert.Equal(expected, actual);            
        }

        [Fact]
        public void testInt1() {
            Config.SetGlobal("int.key1", "100");
            Int32 i = Config.GetGlobal("int.key1", 0);
            Assert.Equal((Int32)100, i);
        }

        [Fact]
        public void testInt2() {
            Config.SetGlobal("int.key2", "1001");
            int i = Config.GetGlobal("int.key2", 0);
            Assert.Equal(1001, i);
        }

        [Fact]
        public void testBool() {
            Config.SetGlobal("bool.key", "true");
            Boolean b = Config.GetGlobal("bool.key", false);
            Assert.True(b);
        }

        [Fact]
        public void testLong() {
            Config.SetGlobal("long.key", "10000");
            long b = Config.GetGlobal("long.key", 1);
            Assert.Equal(b, 10000);
        }

        [Fact]
        public void testInt64() {
            Config.SetGlobal("int64.key", Int64.MaxValue.ToString());
            Int64 b = Config.GetGlobal("int64.key", (Int64) 1);
            Assert.Equal(b, Int64.MaxValue);
        }

        [Fact]
        public void testDate() {
            DateTime expected = new DateTime(1988, 1, 1);
            Config.SetGlobal("date.key", expected);
            DateTime actual = Config.GetGlobal("date.key", new DateTime());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void testManyInserts() {
            for (int i = 0; i < 100; ++i) {
                Config.SetGlobal("test.many.key", i);
            }
        }

        [Fact]
        public void testObject() {
            TestObject expected = new TestObject();
            expected.Name = "Test Name";
            expected.Age = 32;
            expected.Date = DateTime.Now;
            Config.SetGlobal("test.object", expected);
            TestObject actual = Config.GetGlobal<TestObject>("test.object", null);
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
