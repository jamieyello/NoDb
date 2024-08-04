using SlothSerializer.Internal;

namespace SlothSerializer.Tests;

[TestClass]
public class SegmentedListTests {
    [TestMethod]
    public void TestAdd() {
        var list = new SegmentedList<int>();

        for (int i = 0; i < 3000; i++) {
            list.Add(i);
        }

        for (int i = 0; i < 3000; i++) {
            Assert.AreEqual(i, list[i]);
        }
    }
}