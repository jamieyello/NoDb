using System.Linq.Expressions;
using NoDb.Syncers;

namespace NoDb.Tests.SyncedObjects;

class User {
    public string Name { get; set; }
    public int LoadCount { get; set; }
}

[TestClass]
public class FileSyncTest {
    static readonly SyncedObject<User> test_user = new(SyncerConfig.FileSync("FileSyncTest_test_user.test"));
    static readonly SyncedObject<bool[]> test_bool_array = new(SyncerConfig.FileSync("FileSyncTest_test_bool_array.test"));
    static readonly SyncedObject<int> test_int = new(SyncerConfig.FileSync("FileSyncTest_test_int.test"));


    [TestMethod]
    public void TestFileSyncObject() {
        test_user.Value ??= new() { Name = "Jamie" };
        test_user.Value.LoadCount++;
        test_user.Save();
    }

    [TestMethod]
    public void TestFileSyncArray() {
        test_bool_array.Value ??= [true, true, true, false, true];
        test_bool_array.Save();
    }

    [TestMethod]
    public void TestFileSyncInt() {
        test_int.Value = 1;
        test_int.Save();
    }
}