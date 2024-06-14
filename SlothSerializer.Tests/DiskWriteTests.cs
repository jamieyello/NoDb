using System.Diagnostics;

namespace SlothSerializer.Tests;

class TestUser {
    public int Id {get;set;}
    public string Name {get;set;}

    public bool Matches(TestUser? obj)
    {
        if (obj == null) return false;

        return obj.Id == Id && obj.Name == Name;
    }
}

[TestClass]
public class DiskWriteClass {
    [TestMethod]
    public void TestWriteRead() {
        var user_original = new TestUser() { Id = 1, Name = "guy" };
        var bb_original = new BitBuilderBuffer();
        bb_original.Append(user_original);
        var deserialized_user = bb_original.GetReader().Read<TestUser>();
        Assert.IsTrue(user_original.Matches(deserialized_user), "Deserialization failed.");

        var file_path = "diskwritetest.deleteme";

        bb_original.WriteToDisk(file_path);

        var bb_read = new BitBuilderBuffer();
        bb_read.ReadFromDisk(file_path);

        Debug.WriteLine("RESULTS-- (original vs read)");
        Debug.WriteLine(bb_original.GetDebugString());
        Debug.WriteLine(bb_read.GetDebugString());
        Debug.WriteLine("--");
        Assert.IsTrue(bb_original.Matches(bb_read), "Error de-serializing file.");

        var user_read = bb_read.GetReader().Read<TestUser>();
        Assert.IsTrue(user_original.Matches(user_read));

        File.Delete(file_path);
    }
}