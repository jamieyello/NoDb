using NoDb.Syncers;
using NoDb.Tests.Syncers;
using SlothSerializer;

namespace NoDb.Tests;

class User {
    public string? Name { get; set; }
    public int LoadCount { get; set; }
}

[TestClass]
public class SyncTests {
    [TestMethod]
    public async Task TestDebugIntSync() {
        void inspect(string message, BitBuilderBuffer buffer) {
            var read_value = buffer.GetReader().Read<int>();
        }

        var config = new DebugSyncerConfig<int>(3, inspect);
        var so = new SyncedObject<int>(config);
        await TestSyncIntIncrement(so);
    }

    static void TestSyncObject(SyncedObject<User> synced_user) {
        SyncedObject<User> test_user = new(SyncerConfig.FileSync("FileSyncTest_test_user.test"));
        test_user.WaitForLoad();
        var test = test_user.Value;
        test_user.Value ??= new() { Name = "Jamie" };
        test_user.Value.LoadCount++;
        test_user.Save();
    }

    static void TestSyncArray(SyncedObject<bool[]> synced_array) {
        SyncedObject<bool[]> test_bool_array = new(SyncerConfig.FileSync("FileSyncTest_test_bool_array.test"));
        test_bool_array.WaitForLoad();
        var test = test_bool_array.Value;
        test_bool_array.Value ??= [true, true, true, false, true];
        test_bool_array.Save();
    }

    // expects the int value to be 3
    static async Task TestSyncIntIncrement(SyncedObject<int> synced_int_3) {
        synced_int_3.WaitForLoad();
        var read_value = synced_int_3.Value;
        Assert.AreEqual(3, read_value, "Expected initial value of 3. Loading is invalid.");
        synced_int_3.Value++;
        read_value = synced_int_3.Value;
        synced_int_3.Save();
        await synced_int_3.TestingForceFullLoad();
        read_value = synced_int_3.Value;
        Assert.AreEqual(4, read_value);
    }
}