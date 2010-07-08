using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Newtonsoft.Json;

namespace BioLink.Utilities {
    
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
            Preferences.SetPreference(key, expected);
            string actual = Preferences.GetPreference(key);
            Assert.Equal(expected, actual);            
        }

        [Fact]
        public void testInt1() {
            Preferences.SetPreference("int.key1", "100");
            Int32 i = Preferences.Get("int.key1", 0);
            Assert.Equal((Int32)100, i);
        }

        [Fact]
        public void testInt2() {
            Preferences.SetPreference("int.key2", "1001");
            int i = Preferences.Get("int.key2", 0);
            Assert.Equal(1001, i);
        }

        [Fact]
        public void testBool() {
            Preferences.SetPreference("bool.key", "true");
            Boolean b = Preferences.Get("bool.key", false);
            Assert.True(b);
        }

        [Fact]
        public void testLong() {
            Preferences.SetPreference("long.key", "10000");
            long b = Preferences.Get("long.key", 1);
            Assert.Equal(b, 10000);
        }

        [Fact]
        public void testInt64() {
            Preferences.SetPreference("int64.key", Int64.MaxValue.ToString());
            Int64 b = Preferences.Get("int64.key", (Int64) 1);
            Assert.Equal(b, Int64.MaxValue);
        }

        [Fact]
        public void testDate() {
            DateTime expected = new DateTime(1988, 1, 1);
            Preferences.Set("date.key", expected);
            DateTime actual = Preferences.Get("date.key", new DateTime());
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void testManyInserts() {
            for (int i = 0; i < 100; ++i) {
                Preferences.Set("test.many.key", i);
            }
        }

        [Fact]
        public void badTypesThrow() {
            Assert.Throws(typeof(BadPreferenceTypeException), () => {
                Preferences.Set("bad.type.key", "fff");
                Dictionary<String, String> foo = Preferences.Get("bad.type.key", new Dictionary<String, String>());
            });
        }

        public void testBadType() {            
            Preferences.Set("bad.type.key", "fff");
            Dictionary<String, String> foo = Preferences.Get("bad.type.key", new Dictionary<String, String>());
        }

        [Fact]
        public void testJSON() {
            Dictionary<string, string> map = new Dictionary<string, string>();
            map["item1"] = "Item value 1";
            string json = JsonConvert.SerializeObject(map);
            Preferences.Set("json.key", json);
            string val = Preferences.GetPreference("json.key", "");
            Dictionary<string, string> actual = (Dictionary<string, string>) JsonConvert.DeserializeObject(val, map.GetType());
            Console.WriteLine("actual: " + actual.ToString());
        }

        public void Dispose() {
            _prefs.Traverse((key, value) => System.Console.WriteLine("Key: {0} Value: {1}", key, value));            
        }
    }
}
