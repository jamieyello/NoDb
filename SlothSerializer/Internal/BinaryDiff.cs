namespace SlothSerializer.Internal;

public class BinaryDiff
{
    public enum DiffMethodType
    {
        replace
    }

    DiffMethodType Method { get; set; }
    byte[] PatchData { get; set; } // this is completely arbituary, data depends on the method used.

    // values of the expected result
    ulong PatchedHash { get; set; }
    long PatchedLength { get; set; }

    // values of the binary that this should be applied to
    ulong TargetHash { get; set; }
    long TargetLength { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public BinaryDiff() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable IDE0060 // Remove unused parameter
    public BinaryDiff(BitBuilderBuffer old, BitBuilderBuffer new_, DiffMethodType method) {
#pragma warning restore IDE0060 // Remove unused parameter
        Method = method;
        PatchedLength = new_.TotalStreamLengthBytes;
        //Hash = CalculateHash(new_);

        if (Method == DiffMethodType.replace) {
            PatchData = new byte[new_.TotalStreamLengthBytes];
            var ms = new MemoryStream(PatchData);
            new_.WriteToStream(ms);
        }
        else throw new NotImplementedException();
    }

    public BinaryDiff(byte[] old, byte[] new_, DiffMethodType method) {
        Method = method;
        TargetHash = CalculateHash(old);
        TargetLength = old.Length;

        PatchedHash = CalculateHash(new_);
        PatchedLength = new_.Length;

        if (Method == DiffMethodType.replace) {
            PatchData = new byte[new_.Length];
            Array.Copy(new_, PatchData, new_.Length);
        }
        else throw new NotImplementedException();
    }

    public async Task ApplyToAsync(string file_path) {
        using var fs = new FileStream(file_path, FileMode.Open);
        await ApplyToAsync(fs);
    }

    public async Task ApplyToAsync(Stream stream) {
        await Task.Run(() => CheckHash(stream, TargetLength, TargetHash, "Unpatched"));
        
        if (Method == DiffMethodType.replace) await ApplyReplace(PatchData, stream);
        else throw new NotImplementedException();

        await Task.Run(() => CheckHash(stream, PatchedLength, PatchedHash, "Patched"));
    }

    static async Task ApplyReplace(byte[] patch, Stream stream) {
        stream.Position = 0;
        stream.SetLength(patch.Length);
        await stream.WriteAsync(patch);
        stream.Position = 0;
    }

    static void CheckHash(Stream stream, long expected_length, ulong expected_hash, string patched_or_unpatched) {
        stream.Position = 0;
        if (stream.Length != expected_length) throw new Exception($"{patched_or_unpatched} length mismatch. Expected:{expected_length} Read:{stream.Length}");
        if (expected_hash != CalculateHash(stream)) throw new Exception($"Hash check failure. {patched_or_unpatched} result differs from expected result.");
        stream.Position = 0;
    }

    // https://stackoverflow.com/a/9545731
    static ulong CalculateHash(Stream stream) {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < stream.Length; i++) {
            hashedValue += (byte)stream.ReadByte();
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    static ulong CalculateHash(ReadOnlySpan<byte> span) {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < span.Length; i++) {
            hashedValue += span[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}
