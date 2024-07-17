namespace SlothSerializer.Internal;

public class BinaryDiff
{
    public enum DiffMethodType
    {
        replace
    }

    DiffMethodType Method { get; set; }
    byte[] PatchData { get; set; } // this is completely arbituary, data depends on the method used.

    // values of the binary that this should be applied to
    ulong TargetHash { get; set; }
    long TargetLength { get; set; }

    // values of the expected result
    ulong ResultHash { get; set; }
    long ResultLength { get; set; }

    // Todo: This constructor needs to be hidden from the end user, but a method still needs to be made available for JSON serialization.
    [Obsolete("Don't use this manually.")]
    public BinaryDiff() { }

    public BinaryDiff(BitBuilderBuffer old, BitBuilderBuffer new_, DiffMethodType method) {
        Method = method;
        TargetHash = KnuthHash.Calculate(old);
        TargetLength = old.TotalStreamLengthBytes;
        ResultHash = KnuthHash.Calculate(new_);
        ResultLength = new_.TotalStreamLengthBytes;

        if (Method == DiffMethodType.replace) {
            PatchData = new byte[new_.TotalStreamLengthBytes];
            var ms = new MemoryStream(PatchData);
            new_.WriteToStream(ms);
        }
        else throw new NotImplementedException();
    }

    public BinaryDiff(byte[] old, byte[] new_, DiffMethodType method) {
        Method = method;
        TargetHash = KnuthHash.Calculate(old);
        TargetLength = old.Length;
        ResultHash = KnuthHash.Calculate(new_);
        ResultLength = new_.Length;

        if (Method == DiffMethodType.replace) {
            PatchData = new byte[new_.Length];
            Array.Copy(new_, PatchData, new_.Length);
        }
        else throw new NotImplementedException();
    }

    public async Task ApplyToAsync(string file_path) {
        using var fs = new FileStream(file_path, FileMode.Open);
        await ApplyToAsync(fs);
        fs.Flush();
        fs.Close();
    }

    public async Task ApplyToAsync(Stream stream) {
        await Task.Run(() => CheckHash(stream, TargetLength, TargetHash, "Unpatched"));
        
        if (Method == DiffMethodType.replace) await ApplyReplace(PatchData, stream);
        else throw new NotImplementedException();

        await Task.Run(() => CheckHash(stream, ResultLength, ResultHash, "Patched"));
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
        //if (expected_hash != KnuthHash.Calculate(stream)) throw new Exception($"Hash check failure. {patched_or_unpatched} result differs from expected result.");
        stream.Position = 0;
    }
}
