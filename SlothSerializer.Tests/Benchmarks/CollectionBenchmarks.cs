using SlothSerializer;
using SlothSerializer.Internal;

namespace SlothSockets.Tests;

[TestClass]
public class CollectionBenchmarks
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
}
