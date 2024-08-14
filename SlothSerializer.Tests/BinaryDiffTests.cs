using System.Text;
using SlothSerializer.DiffTracking;
using SlothSerializer.Internal;

namespace SlothSerializer.Tests;

[TestClass]
public class BinaryDiffTests {
    const string FIRST_TEXT = "first";
    const string SECOND_TEXT = "firstsecond";

    static readonly byte[] StartData = Encoding.ASCII.GetBytes(FIRST_TEXT);
    static readonly byte[] EndData = Encoding.ASCII.GetBytes(SECOND_TEXT);

    // Diffs are for the time being exclusively for BitBuilderBuffers
    // [TestMethod]
    // public async Task TestReplaceDiffByteArray() {
    //     var diff = new BinaryDiff(StartData, EndData, BinaryDiff.DiffMethodType.replace);

    //     var ms = new MemoryStream();
    //     ms.Write(StartData);

    //     Assert.AreEqual(FIRST_TEXT, Encoding.ASCII.GetString(ms.ToArray()));
    //     await diff.ApplyToAsync(ms);
    //     Assert.AreEqual(SECOND_TEXT, Encoding.ASCII.GetString(ms.ToArray()));
    // }

    [TestMethod]
    public async Task TestReplaceDiffBitBuilder() {
        var ms = new MemoryStream();

        var bb1 = new BitBuilderBuffer();
        var bb2 = new BitBuilderBuffer();
        bb1.Append(StartData);
        bb2.Append(EndData);
        var diff = new BitBuilderDiff(bb1, bb2, BitBuilderDiff.DiffMethodType.replace);

        bb1.WriteToStream(ms);
        await diff.ApplyToAsync(ms);

        //diff.ApplyToAsync()
    }
}