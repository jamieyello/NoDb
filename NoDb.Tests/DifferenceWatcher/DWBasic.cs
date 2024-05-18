using NoDb.Difference;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoDb.Tests.DW;

[TestClass]
public class DWBasic
{
    [TestMethod]
    public void TestDW()
    {
        var testobj = new TestUserClass();
        int dif_count = 0;

        async Task callback(DifferenceWatcherEventArgs<TestUserClass?> args) { 
            dif_count++;
            testobj.Id++;
        }

        var dw = new DifferenceWatcher<TestUserClass>(testobj, callback, new() { SyncInterval = TimeSpan.FromSeconds(0.01) });

        Thread.Sleep(100);
        testobj.Name = "jamie";
        Thread.Sleep(100);
        testobj.Name = "jamie3";
        Thread.Sleep(100);
    }
}
