using NoDb.Difference;

namespace NoDb.Tests.DW;

[TestClass]
public class DWBasic
{
    [TestMethod]
    public void TestDW()
    {
        var testobj = new TestUserClass();
        int dif_count = 0;

        async Task callback(DifferenceWatcherEventArgs<TestUserClass> args) { 
            dif_count++;
            testobj.Id++;
        }

        var dw = new DifferenceWatcher<TestUserClass?>(testobj, callback, new() { SyncInterval = TimeSpan.FromMilliseconds(50) });

        Thread.Sleep(100);
        testobj.Name = "jamie";
        Thread.Sleep(100);
        testobj.Name = "jamie3";
        Thread.Sleep(100);
    }
}
