using System.Diagnostics;
using SlothSerializer.Tests.TestClasses;

namespace SlothSerializer.Tests;

[TestClass]
public class StreamWriteTests {
    [TestMethod]
    public void StreamWriteClass() {
        var user_original = new TestUser() { Id = 1, Name = "guy" };
        using var ms = new MemoryStream();
        StreamWriteObject(user_original, ms, (u1, u2) => u1.Matches(u2));
    }

    [TestMethod]
    public void StreamWriteByte() {
        using var ms = new MemoryStream();
        StreamWriteObject(byte.MaxValue, ms, (u1, u2) => u1 == u2);
    }

    [TestMethod]
    public void StreamWriteUnevenData() {
        var bb_original = new BitBuilderBuffer();
        bb_original.Append(ulong.MaxValue);
        bb_original.Append(true);
        bb_original.Append(false);
        bb_original.Append(true);

        using var ms = new MemoryStream();
        StreamWriteRead(bb_original, ms);
    }

    [TestMethod]
    public void FileStreamWrite() {
        var user_original = new TestUser() { Id = 1, Name = "guy" };
        var file_path = "diskwritetest.deleteme";

        using (var fs = new FileStream(file_path, FileMode.Create)) {
            StreamWriteObject(user_original, fs, (u1, u2) => u1.Matches(u2));
        }

        File.Delete(file_path);
    }

    static void StreamWriteObject<T>(T obj, Stream stream, Func<T, T?, bool> matches) {
        var bb_original = new BitBuilderBuffer();
        bb_original.Append(obj);
        var obj_deserialized = bb_original.GetReader().Read<T>();
        Assert.IsTrue(matches(obj, obj_deserialized), "Deserialization failed.");

        var bb_read = StreamWriteRead(bb_original, stream);

        var obj_read = bb_read.GetReader().Read<T>();
        Assert.IsTrue(matches(obj, obj_read));
    }

    static BitBuilderBuffer StreamWriteRead(BitBuilderBuffer buffer, Stream stream) {
        buffer.WriteToStream(stream);
        var bb_read = new BitBuilderBuffer();
        stream.Position = 0;
        bb_read.ReadFromStream(stream);

        Debug.WriteLine("RESULTS-- (original vs read)");
        Debug.WriteLine("");
        Debug.WriteLine(buffer.GetDebugString());
        Debug.WriteLine("");
        Debug.WriteLine(bb_read.GetDebugString());
        Debug.WriteLine("--");
        Assert.IsTrue(buffer.Matches(bb_read), "Error de-serializing file.");
        return bb_read;
    }
}