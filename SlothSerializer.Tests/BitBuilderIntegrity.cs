using SlothSerializer;

namespace SlothSerializer.Tests
{
    [TestClass]
    public class BitBuilderIntegrity
    {
        [TestMethod]
        public void IntegrityTestRand()
        {
            var rand = new Random(2);
            var bb = new BitBuilderBuffer();

            bb.Append(true);
            Console.WriteLine("Expected;");
            for (int i = 0; i < 10; i++) {
                var d = (byte)rand.Next();
                bb.Append(d);
                Console.WriteLine(d);
            }
            Console.WriteLine();

            Console.WriteLine("Results;");
            rand = new(2);
            var reader = bb.GetReader();
            reader.ReadBool();
            for (int i = 0; i < 10; i++) { 
                var d = reader.ReadByte();
                Console.WriteLine(d);
                Assert.AreEqual((byte)rand.Next(), d);
            }
        }

        [TestMethod]
        public void IntegrityTestString()
        {
            var message = "Wowowowow.";

            var bb = new BitBuilderBuffer();
            bb.Append(true);
            bb.Append(message);

            var reader = bb.GetReader();

            reader.ReadBool();
            var s = reader.ReadString();
            Assert.AreEqual(message, s);
        }

        [TestMethod]
        public void IntegrityTestEnumerate() {
            var message = "Wowowowow.";
            var bb = new BitBuilderBuffer();
            bb.Append(true);
            bb.Append(message);

            var ms = new MemoryStream();
            foreach (var b in bb.EnumerateAsBytes()) {
                ms.WriteByte(b);
            }
            ms.Position = 0;
            var bb2 = new BitBuilderBuffer();
            bb2.ReadFromStream(ms);

            var reader = bb2.GetReader();

            reader.ReadBool();
            var s = reader.ReadString();
            Assert.AreEqual(message, s);
        }
    }
}
