using NoDb.Syncers;

namespace NoDb.Tests.SyncedObjects;

class TestClass {
    public int Value { get; set; }
}

[TestClass]
public class FileSyncTest {
    readonly SyncedObject<TestClass> test = new(new(), SyncerConfig.FileSync("delete_me.test"));

    [TestMethod]
    public async Task TestFileSync() {
        await test.Initilized;
    }
}