using System.Text;

namespace SlothSerializer.Internal;

public class BinaryDiff
{
    public enum DiffMethodType
    {
        replace
    }

    DiffMethodType Method { get; set; }
    byte[] PatchData { get; set; }
    ulong? Hash { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public BinaryDiff() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE0060 // Remove unused parameter
    public BinaryDiff(BitBuilderBuffer old, BitBuilderBuffer new_, DiffMethodType method) {
#pragma warning restore IDE0060 // Remove unused parameter
        Method = method;
        var test = Encoding.ASCII.GetByteCount("test");

        if (Method == DiffMethodType.replace) {
            PatchData = new byte[new_.TotalStreamLengthBytes];
            var ms = new MemoryStream(PatchData);
            new_.WriteToStream(ms);
        }
        else throw new NotImplementedException();
    }

    public async Task ApplyToAsync(Stream stream) {
        if (Method == DiffMethodType.replace) await ApplyReplace(PatchData, stream);
        else throw new NotImplementedException();
    }

    static async Task ApplyReplace(byte[] patch, Stream stream) {
        stream.Position = 0;
        stream.SetLength(patch.Length);
        await stream.WriteAsync(patch);
    }

    // https://stackoverflow.com/a/9545731
    static ulong CalculateHash(ulong[] read)
    {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < read.Length; i++)
        {
            hashedValue += read[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}
