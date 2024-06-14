using NoDb.Difference;
using SlothSerializer;

namespace NoDb.Tests.DW;

[TestClass]
public class DWBasic
{
    [TestMethod]
    public void TestDW()
    {
        var testobj = new TestUserClass();
        var test_bb = new BitBuilderBuffer();
        
        int dif_count = 0;

        void callback(DifferenceWatcherEventArgs<TestUserClass> args) { 
            dif_count++;
        }

        var dw = new DifferenceWatcher<TestUserClass>(testobj, callback, new() { AutoSyncInterval = TimeSpan.FromMilliseconds(50) });

        Thread.Sleep(100);
        testobj.Name = "jamie";
        Thread.Sleep(100);
        testobj.Name = "jamie3";
        Thread.Sleep(100);
        Assert.AreEqual(2, dif_count);
    }
}
