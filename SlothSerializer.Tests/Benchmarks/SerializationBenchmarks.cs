using Newtonsoft.Json;
using SlothSerializer.Tests.TestClasses;

namespace SlothSerializer.Tests.Benchmarks;

[TestClass]
public class SerializationBenchmarks {
    const int COUNT = 1000000;
    
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