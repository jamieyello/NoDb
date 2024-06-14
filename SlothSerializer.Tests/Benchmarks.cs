using Newtonsoft.Json;
using SlothSerializer;
using SlothSerializer.Internal;
using static SlothSockets.Tests.SerializerTests;

namespace SlothSockets.Tests;

[TestClass]
public class Benchmarks
{
    const int COUNT = 1000000;

    [TestMethod]
    public void WriteBitBuilder()
    {
        var target = new BitBuilderBuffer();
        for (int i = 0; i < COUNT; i++) target.Append((byte)0);
    }

    [TestMethod]
    public void WriteList()
    {
        var target = new List<byte>();
        for (int i = 0; i < COUNT; i++) target.Add(0);
    }

    [TestMethod]
    public void WriteLowMemList()
    {
        var target = new LowMemList<byte>();
        for (int i = 0; i < COUNT; i++) target.Add(0);
    }

    [TestMethod]
    public void WriteArray()
    {
        var target = new byte[COUNT];
        for (int i = 0; i < COUNT; i++) target[i] = 0;
    }

    [TestMethod]
    public void BBTest()
    {
        var bb = new BitBuilderBuffer();
        var test = new TestClass4();
        for (int i = 0;i < COUNT; i++) {
            bb.Append(test);
        }
    }

    [TestMethod]
    public void JsonTest()
    {
        var test = new TestClass4();
        for (int i = 0; i < COUNT; i++)
        {
            JsonConvert.SerializeObject(test);
        }
    }
}
