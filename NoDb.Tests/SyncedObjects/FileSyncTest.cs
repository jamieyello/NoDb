using NoDb.Syncers;

namespace NoDb.Tests.SyncedObjects;

class User {
    public string Name { get; set; }
    public int LoadCount { get; set; }
}

[TestClass]
public class FileSyncTest {
    readonly SyncedObject<User> test = new(SyncerConfig.FileSync("delete_me.test"));

    [TestMethod]
    public void TestFileSync() {
        test.Value ??= new() { Name = "Jamie" };
        test.Value.LoadCount++;
        test.Save();
    }
}