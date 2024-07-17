using NoDb.Syncers;
using NoDb.Tests.Syncers;

namespace NoDb.Tests;

class User {
    public string? Name { get; set; }
    public int LoadCount { get; set; }
}

[TestClass]
public class FileSyncTests {
    [TestMethod]
    public void TestSyncObject() {
        SyncedObject<User> test_user = new(SyncerConfig.FileSync("FileSyncTest_test_user.test"));
        test_user.WaitForLoad();
        var test = test_user.Value;
        test_user.Value ??= new() { Name = "Jamie" };
        test_user.Value.LoadCount++;
        test_user.Save();
    }

    [TestMethod]
    public void TestSyncArray() {
        SyncedObject<bool[]> test_bool_array = new(SyncerConfig.FileSync("FileSyncTest_test_bool_array.test"));
        test_bool_array.WaitForLoad();
        var test = test_bool_array.Value;
        test_bool_array.Value ??= [true, true, true, false, true];
        test_bool_array.Save();
    }

    [TestMethod]
    public async Task TestSyncIntIncrement() {
        var c = new DebugSyncerConfig<int>(0);
        SyncedObject<int> test_int = new(SyncerConfig.FileSync("FileSyncTest_test_int.test"));
        test_int.WaitForLoad();
        var test = test_int.Value;
        test_int.Value++;
        test = test_int.Value;
        test_int.Save();
        await test_int.TestingForceFullLoad();
        test = test_int.Value;
    }
}