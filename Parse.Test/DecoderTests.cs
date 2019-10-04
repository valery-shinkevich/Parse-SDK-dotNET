using Parse;
using Parse.Core.Internal;
using System;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parse.Test
{
    [TestClass]
    public class DecoderTests
    {
        [TestMethod]
        public void TestParseDate()
        {
            DateTime dateTime = (DateTime) ParseDecoder.Instance.Decode(ParseDecoder.ParseDate("1990-08-30T12:03:59.000Z"));
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);
        }

        [TestMethod]
        public void TestDecodePrimitives()
        {
            Assert.AreEqual(1, ParseDecoder.Instance.Decode(1));
            Assert.AreEqual(0.3, ParseDecoder.Instance.Decode(0.3));
            Assert.AreEqual("halyosy", ParseDecoder.Instance.Decode("halyosy"));

            Assert.IsNull(ParseDecoder.Instance.Decode(null));
        }

        [TestMethod]
        // Decoding ParseFieldOperation is not supported on .NET now. We only need this for LDS.
        public void TestDecodeFieldOperation() => Assert.ThrowsException<NotImplementedException>(() => ParseDecoder.Instance.Decode(new Dictionary<string, object>() { { "__op", "Increment" }, { "amount", "322" } }));

        [TestMethod]
        public void TestDecodeDate()
        {
            DateTime dateTime = (DateTime) ParseDecoder.Instance.Decode(new Dictionary<string, object>() { { "__type", "Date" }, { "iso", "1990-08-30T12:03:59.000Z" } });
            Assert.AreEqual(1990, dateTime.Year);
            Assert.AreEqual(8, dateTime.Month);
            Assert.AreEqual(30, dateTime.Day);
            Assert.AreEqual(12, dateTime.Hour);
            Assert.AreEqual(3, dateTime.Minute);
            Assert.AreEqual(59, dateTime.Second);
            Assert.AreEqual(0, dateTime.Millisecond);
        }

        [TestMethod]
        public void TestDecodeImproperDate()
        {
            IDictionary<string, object> value = new Dictionary<string, object> { ["__type"] = "Date", ["iso"] = "1990-08-30T12:03:59.0Z" };

            for (int i = 0; i < 2; i++, value["iso"] = (value["iso"] as string).Substring(0, (value["iso"] as string).Length - 1) + "0Z")
            {
                DateTime dateTime = (DateTime) ParseDecoder.Instance.Decode(value);
                Assert.AreEqual(1990, dateTime.Year);
                Assert.AreEqual(8, dateTime.Month);
                Assert.AreEqual(30, dateTime.Day);
                Assert.AreEqual(12, dateTime.Hour);
                Assert.AreEqual(3, dateTime.Minute);
                Assert.AreEqual(59, dateTime.Second);
                Assert.AreEqual(0, dateTime.Millisecond);
            }
        }

        [TestMethod]
        public void TestDecodeBytes() => Assert.AreEqual("This is an encoded string", System.Text.Encoding.UTF8.GetString(ParseDecoder.Instance.Decode(new Dictionary<string, object>() { { "__type", "Bytes" }, { "base64", "VGhpcyBpcyBhbiBlbmNvZGVkIHN0cmluZw==" } }) as byte[]));

        [TestMethod]
        public void TestDecodePointer()
        {
            ParseObject obj = ParseDecoder.Instance.Decode(new Dictionary<string, object> { ["__type"] = "Pointer", ["className"] = "Corgi", ["objectId"] = "lLaKcolnu" }) as ParseObject;
            Assert.IsFalse(obj.IsDataAvailable);
            Assert.AreEqual("Corgi", obj.ClassName);
            Assert.AreEqual("lLaKcolnu", obj.ObjectId);
        }

        [TestMethod]
        public void TestDecodeFile()
        {

            ParseFile file1 = ParseDecoder.Instance.Decode(new Dictionary<string, object> { ["__type"] = "File", ["name"] = "Corgi.png", ["url"] = "http://corgi.xyz/gogo.png" }) as ParseFile;
            Assert.AreEqual("Corgi.png", file1.Name);
            Assert.AreEqual("http://corgi.xyz/gogo.png", file1.Url.AbsoluteUri);
            Assert.IsFalse(file1.IsDirty);

            Assert.ThrowsException<KeyNotFoundException>(() => ParseDecoder.Instance.Decode(new Dictionary<string, object> { ["__type"] = "File", ["name"] = "Corgi.png" }));
        }

        [TestMethod]
        public void TestDecodeGeoPoint()
        {
            ParseGeoPoint point1 = (ParseGeoPoint) ParseDecoder.Instance.Decode(new Dictionary<string, object> { ["__type"] = "GeoPoint", ["latitude"] = 0.9, ["longitude"] = 0.3 });
            Assert.IsNotNull(point1);
            Assert.AreEqual(0.9, point1.Latitude);
            Assert.AreEqual(0.3, point1.Longitude);

            Assert.ThrowsException<KeyNotFoundException>(() => ParseDecoder.Instance.Decode(new Dictionary<string, object> { ["__type"] = "GeoPoint", ["latitude"] = 0.9 }));
        }

        [TestMethod]
        public void TestDecodeObject()
        {
            IDictionary<string, object> value = new Dictionary<string, object>()
            {
                ["__type"] = "Object",
                ["className"] = "Corgi",
                ["objectId"] = "lLaKcolnu",
                ["createdAt"] = "2015-06-22T21:23:41.733Z",
                ["updatedAt"] = "2015-06-22T22:06:41.733Z"
            };

            ParseObject obj = ParseDecoder.Instance.Decode(value) as ParseObject;
            Assert.IsTrue(obj.IsDataAvailable);
            Assert.AreEqual("Corgi", obj.ClassName);
            Assert.AreEqual("lLaKcolnu", obj.ObjectId);
            Assert.IsNotNull(obj.CreatedAt);
            Assert.IsNotNull(obj.UpdatedAt);
        }

        [TestMethod]
        public void TestDecodeRelation()
        {
            IDictionary<string, object> value = new Dictionary<string, object>()
            {
                ["__type"] = "Relation",
                ["className"] = "Corgi",
                ["objectId"] = "lLaKcolnu"
            };

            ParseRelation<ParseObject> relation = ParseDecoder.Instance.Decode(value) as ParseRelation<ParseObject>;
            Assert.IsNotNull(relation);
            Assert.AreEqual("Corgi", relation.GetTargetClassName());
        }

        [TestMethod]
        public void TestDecodeDictionary()
        {
            IDictionary<string, object> value = new Dictionary<string, object>()
            {
                ["megurine"] = "luka",
                ["hatsune"] = new ParseObject("Miku"),
                ["decodedGeoPoint"] = new Dictionary<string, object>
                {
                    ["__type"] = "GeoPoint",
                    ["latitude"] = 0.9,
                    ["longitude"] = 0.3
                },
                ["listWithSomething"] = new List<object>
                {
                    new Dictionary<string, object>
                    {
                        ["__type"] = "GeoPoint",
                        ["latitude"] = 0.9,
                        ["longitude"] = 0.3
                    }
                }
            };

            IDictionary<string, object> dict = ParseDecoder.Instance.Decode(value) as IDictionary<string, object>;
            Assert.AreEqual("luka", dict["megurine"]);
            Assert.IsTrue(dict["hatsune"] is ParseObject);
            Assert.IsTrue(dict["decodedGeoPoint"] is ParseGeoPoint);
            Assert.IsTrue(dict["listWithSomething"] is IList<object>);
            IList<object> decodedList = dict["listWithSomething"] as IList<object>;
            Assert.IsTrue(decodedList[0] is ParseGeoPoint);

            IDictionary<object, string> randomValue = new Dictionary<object, string>()
            {
                ["ultimate"] = "elements",
                [new ParseACL()] = "lLaKcolnu"
            };

            IDictionary<object, string> randomDict = ParseDecoder.Instance.Decode(randomValue) as IDictionary<object, string>;
            Assert.AreEqual("elements", randomDict["ultimate"]);
            Assert.AreEqual(2, randomDict.Keys.Count);
        }

        [TestMethod]
        public void TestDecodeList()
        {
            IList<object> value = new List<object>()
            {
                1, new ParseACL(), "wiz",
                new Dictionary<string, object>()
                {
                    ["__type"] = "GeoPoint",
                    ["latitude"] = 0.9,
                    ["longitude"] = 0.3
                },
                new List<object>()
                {
                    new Dictionary<string, object>()
                    {
                        ["__type"] = "GeoPoint",
                        ["latitude"] =  0.9,
                        ["longitude"] = 0.3
                    }
                }
            };

            IList<object> list = ParseDecoder.Instance.Decode(value) as IList<object>;
            Assert.AreEqual(1, list[0]);
            Assert.IsTrue(list[1] is ParseACL);
            Assert.AreEqual("wiz", list[2]);
            Assert.IsTrue(list[3] is ParseGeoPoint);
            Assert.IsTrue(list[4] is IList<object>);
            IList<object> decodedList = list[4] as IList<object>;
            Assert.IsTrue(decodedList[0] is ParseGeoPoint);
        }

        [TestMethod]
        public void TestDecodeArray()
        {
            int[] value = new int[] { 1, 2, 3, 4 };

            int[] array = ParseDecoder.Instance.Decode(value) as int[];
            Assert.AreEqual(4, array.Length);
            Assert.AreEqual(1, array[0]);
            Assert.AreEqual(2, array[1]);
        }
    }
}
