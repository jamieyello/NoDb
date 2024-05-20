using Newtonsoft.Json;
using SlothSerializer;
using System.Diagnostics;
using System.Text;

namespace SlothSockets.Tests
{
    [TestClass]
    public class SerializerTests
    {
        // Tested;
        // class, struct
        // all/most common base value types
        //
        // Tbt;
        // Arrays, null values, T? types, properties, attributes

        #region Test Classes
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        struct TestClass1
        {
            //public ulong test1;
            //public ulong test2;
            public ulong test3;
            //public ulong test4;
            public string test_string;

            public bool Matches(TestClass1 t) =>
                test3 == t.test3 &&
                test_string == t.test_string;
        }

        class TestClass2
        {
            public ulong test_value;
            public ulong[] test_array;
            public ulong[,] test_array2;
            public string test_string;
            public string test_string2;
            public TestClass1 test_class;

            public bool Matches(TestClass2 t) =>
                test_value == t.test_value &&
                (test_array == null && t.test_array == null) || test_array.SequenceEqual(t.test_array) &&
                Array2DMatches(test_array2, t.test_array2) &&
                test_string == t.test_string &&
                test_string2 == t.test_string2 &&
                test_class.Matches(t.test_class);
        }

        class TestClass3
        {
            public ulong test_value1;
            public ulong[] test_values;

            public bool Matches(TestClass3 v) =>
                test_value1 == v.test_value1 &&
                ((test_values == null && v.test_values == null) || test_values.SequenceEqual(v.test_values));
        }

        public enum TestEnum
        {
            first,
            second,
            third
        }

        public class TestClass4
        {
            public List<ulong> test_list;
            public KeyValuePair<ulong, ulong> test_kvp;
            public Dictionary<ulong, ulong> test_dictionary;
            public TestEnum test_enum;

            public bool Matches(TestClass4 t) =>
                test_list.SequenceEqual(t.test_list) &&
                test_kvp.Key == t.test_kvp.Key && test_kvp.Value == t.test_kvp.Value &&
                test_dictionary.SequenceEqual(t.test_dictionary) &&
                test_enum.Equals(t.test_enum);
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        #endregion

        [TestMethod]
        public void SerializeBool()
        {
            var bb = new BitBuilderBuffer();
            bb.Append(true);
            var reader = bb.GetReader();
            var read = reader.ReadBool();
            Assert.AreEqual(true, read);
        }

        [TestMethod]
        public void SerializeTest1()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass1()
            {
                //test1 = 1,
                //test2 = 2,
                test3 = 3,
                //test4 = 4,
                //test_string = "wowowow",
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass1>();
            Assert.AreEqual(original, read);
        }

        [TestMethod]
        public void SerializeTest2()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass2()
            {
                test_array = new ulong[] { 1, 2, 3 },
                test_array2 = new ulong[,] { { 1, 2, 3 }, { 4, 5, 6 } },
                test_string = "wowowow",
                test_string2 = "@#wowowowFF",
                test_value = 3,
                test_class = new()
                {
                    test3 = 4,
                    test_string = "hhh"
                }

            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass2>()
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest3()
        {
            var bb = new BitBuilderBuffer();
            var value = ulong.MaxValue / 2;
            var original = new TestClass3()
            {
                test_value1 = value,
                test_values = new[] { value, value, value, }
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass3>()
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest4()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass4()
            {
                test_list = new() { 1, 2, 4, 5 },
                test_kvp = new(8, 7),
                test_dictionary = new() {
                    { 0, 3 },
                    { 1, 5 },
                    { 7, 8 },
                },
                test_enum = TestEnum.second
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass4>()
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [Serializable]
        public struct TestReadOnly
        {
            private readonly int value;

            public TestReadOnly()
            {

            }
            public TestReadOnly(int value)
            {
                this.value = value;
            }
        }

        [TestMethod]
        public void TestKVP()
        {
            var original = new TestReadOnly(4);
            var json = JsonConvert.SerializeObject(original);

            var kvp_original = new KeyValuePair<int, int>(2, 3);
            var json_kvp = JsonConvert.SerializeObject(kvp_original);
        }

        static bool Array2DMatches<T>(T?[,] array1, T?[,] array2)
        {
            var null_match = (array1 == null) == (array2 == null);
            if (null_match && array1 == null) return true;
            if (!null_match) return false;

            // I know it's not null here damnit
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if ((array1.GetLength(0) != array2.GetLength(0)) || (array1.GetLength(1) != array2.GetLength(1))) return false;
            for (int x = 0; x < array1.GetLength(0); x++)
            {
                for (int y = 0; y < array1.GetLength(1); y++)
                {
                    if (!array1[x, y].Equals(array2[x, y])) return false;
                }
            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            return true;
        }
    }
}
