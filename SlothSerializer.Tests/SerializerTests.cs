using Newtonsoft.Json;
using SlothSerializer;
using SlothSerializer.Tests.TestClasses;
using System.Diagnostics;

namespace SlothSerializer.Tests
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
        public void SerializeProperties()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestPropClass()
            {
                TestValue1 = 1,
                TestValue2 = 2,
            };
            original.SetTestValue3(3);
            original.SetTestValue4(4);
            bb.Append(original, SerializeMode.Properties);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestPropClass>(SerializeMode.Properties)
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest1()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass1()
            {
                test1 = 1,
                test2 = 2,
                test3 = 3,
                test4 = 4,
                test_string = "wowowow",
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass1>(SerializeMode.Fields);
            Assert.AreEqual(original, read);
        }

        [TestMethod]
        public void SerializeTest2()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass2()
            {
                test_array = [1, 2, 3],
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

            var read = bb.GetReader().Read<TestClass2>(SerializeMode.Fields)
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
                test_values = [value, value, value,]
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass3>(SerializeMode.Fields)
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest4()
        {
            var bb = new BitBuilderBuffer();
            var original = new TestClass4()
            {
                test_list = [1, 2, 4, 5],
                test_kvp = new(8, 7),
                test_dictionary = new() {
                    { 0, 3 },
                    { 1, 5 },
                    { 7, 8 },
                },
                test_enum = TestClass4.TestEnum.second
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass4>(SerializeMode.Fields)
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void SerializeTest5()
        {
            var bb = new BitBuilderBuffer();
            byte value = 5;
            var original = new TestClass5()
            {
                test_value1 = value,
                test_values = [value, value, value,]
            };
            bb.Append(original, SerializeMode.Fields);
            Debug.WriteLine(bb.GetDebugString());

            var read = bb.GetReader().Read<TestClass5>(SerializeMode.Fields)
                ?? throw new Exception("Read null.");
            Assert.IsTrue(original.Matches(read));
        }

        [TestMethod]
        public void TestKVP()
        {
            var original = new TestReadOnlyClass(4);
            var json = JsonConvert.SerializeObject(original);

            var kvp_original = new KeyValuePair<int, int>(2, 3);
            var json_kvp = JsonConvert.SerializeObject(kvp_original);
        }
    }
}