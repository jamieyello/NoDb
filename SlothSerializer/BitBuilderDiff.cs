using SlothSerializer.DiffTracking;
using SlothSerializer.Internal;

namespace SlothSerializer;

// Rules; Does not apply to/patch headers under any circumstances.
// Updates header when applied.
public class BitBuilderDiff
{
    public enum DiffMethodType
    {
        replace,
        patch
    }

    DiffMethodType Method { get; set; }
    MemoryStream? ReplaceData { get; set; }

    // values of the binary that this should be applied to
    ulong TargetHash { get; set; }
    long TargetLengthBits { get; set; }

    // values of the expected result
    ulong ResultHash { get; set; }
    long ResultLengthBits { get; set; }

    // Todo: This constructor needs to be hidden from the end user, but a method still needs to be made available for JSON serialization.
    [Obsolete("Don't use this manually.")]
    public BitBuilderDiff() { }

    public BitBuilderDiff(BitBuilderBuffer old, BitBuilderBuffer new_, DiffMethodType method) {
        Method = method;
        TargetHash = old.GetKnuthHash();
        TargetLengthBits = old.DataLengthBytes;
        ResultHash = new_.GetKnuthHash();
        ResultLengthBits = new_.DataLengthBytes;

        if (Method == DiffMethodType.replace) {
            ReplaceData = new();
            new_.WriteToStream(ReplaceData, false);
        }
        if (Method == DiffMethodType.patch) {
            throw new NotImplementedException();
        }
        else throw new NotImplementedException();
    }

    public void ApplyTo(BitBuilderBuffer buffer) {
        if (Method == DiffMethodType.replace) {
            if (ReplaceData == null) throw new Exception();
            ReplaceData.Position = 0;
            buffer.Clear();
            buffer.ReadFromStream(ReplaceData, ResultLengthBits);
        }
        else throw new NotImplementedException();
    }

    public async Task ApplyToAsync(string serialized_buffer_file_path) {
        using var fs = new FileStream(serialized_buffer_file_path, FileMode.Open);
        await ApplyToAsync(fs);
        fs.Flush();
        fs.Close();
    }

    async Task ApplyToAsync(Stream stream) {
        await Task.Run(() => CheckHash(stream, TargetLengthBits, TargetHash, "Unpatched"));
        
        if (Method == DiffMethodType.replace) await ApplyReplace(stream);
        else throw new NotImplementedException();

        await Task.Run(() => CheckHash(stream, ResultLengthBits, ResultHash, "Patched"));
    }

    async Task ApplyReplace(Stream stream) {
        if (ReplaceData == null || Method != DiffMethodType.replace) throw new Exception();
        stream.Position = 0;
        stream.SetLength(ReplaceData.Length);
        ReplaceData.Position = 0;
        await ReplaceData.CopyToAsync(stream);
        stream.Position = 0;
    }

    static void CheckHash(Stream stream, long expected_length, ulong expected_hash, string patched_or_unpatched) {
        stream.Position = 0;
        //if (stream.Length != expected_length) throw new Exception($"{patched_or_unpatched} length mismatch. Expected:{expected_length} Read:{stream.Length}");
        //if (expected_hash != KnuthHash.Calculate(stream)) throw new Exception($"Hash check failure. {patched_or_unpatched} result differs from expected result.");
        stream.Position = 0;
    }
}

// // Adam
// var _world_of_warcraft = "boy.ssmh";
// if (!string.IsNullOrWhiteSpace(_world_of_warcraft)) {
//     Console.WriteLine("$");
// }