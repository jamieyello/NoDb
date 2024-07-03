using System.Diagnostics;

namespace SlothSerializer.Tests;

[TestClass]
public class StreamWriteTests {
    [TestMethod]
    public void StreamWrite() {
        var user_original = new TestUser() { Id = 1, Name = "guy" };
        var bb_original = new BitBuilderBuffer();
        bb_original.Append(user_original);
        var deserialized_user = bb_original.GetReader().Read<TestUser>();
        Assert.IsTrue(user_original.Matches(deserialized_user), "Deserialization failed.");

        var file_path = "streamwritetest.deleteme";

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