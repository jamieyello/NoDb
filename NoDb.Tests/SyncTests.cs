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
        var syncer = so.GetSyncers<DebugSyncer<int>>();
        syncer.First().InspectSyncer = so;
        await TestSyncIntIncrement(so);
    }

    [TestMethod]
    public async Task TestFileIntSync() =>
        await TestSyncIntIncrement(new FileObject<int>("TestFileIntSync.deleteme", 3, true));
    
    [TestMethod]
    public void TestObjectFileSync() {
        using var user = new FileObject<User>("TestObjectFileSync.deleteme", new() { Name = "Jamie"});
        user.Value.LoadCount++;
    }

    // expects the int value to be 3
    static async Task TestSyncIntIncrement(SyncedObject<int> synced_int_3) {
        synced_int_3.WaitForLoad();
        var read_value = synced_int_3.Value;
        Assert.AreEqual(3, read_value, "Expected initial value of 3. Loading is invalid.");
        synced_int_3.Value++;
        read_value = synced_int_3.Value;
        synced_int_3.Sync();
        await synced_int_3.TestingForceFullLoad();
        read_value = synced_int_3.Value;
        Assert.AreEqual(4, read_value);
    }
}