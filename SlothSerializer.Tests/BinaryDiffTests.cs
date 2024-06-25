using System.Text;
using SlothSerializer.Internal;

namespace SlothSerializer.Tests;

[TestClass]
public class BinaryDiffTests {
    const string FIRST_TEXT = "first";
    const string SECOND_TEXT = "firstsecond";

    static readonly byte[] StartData = Encoding.ASCII.GetBytes(FIRST_TEXT);
    static readonly byte[] EndData = Encoding.ASCII.GetBytes(SECOND_TEXT);

    [TestMethod]
    public async Task TestReplaceDiffByteArray() {
        var diff = new BinaryDiff(StartData, EndData, BinaryDiff.DiffMethodType.replace);

        var ms = new MemoryStream();
        ms.Write(StartData);

        Assert.AreEqual(FIRST_TEXT, Encoding.ASCII.GetString(ms.ToArray()));
        await diff.ApplyToAsync(ms);
        Assert.AreEqual(SECOND_TEXT, Encoding.ASCII.GetString(ms.ToArray()));
    }
}