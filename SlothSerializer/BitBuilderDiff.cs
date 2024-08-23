using SlothSerializer.DiffTracking;
using SlothSerializer.Internal;

namespace SlothSerializer;

// Rules; Does not apply to headers under any circumstances.

public class BitBuilderDiff
{
    public enum DiffMethodType
    {
        replace,
        patch
    }

    DiffMethodType Method { get; set; }
    MemoryStream? ReplaceData { get; set; }
    List<BinaryDiffSegment>? PatchSegments { get; set; }

    // values of the binary that this should be applied to
    ulong TargetHash { get; set; }
    long TargetLength { get; set; }

    // values of the expected result
    ulong ResultHash { get; set; }
    long ResultLength { get; set; }

    // Todo: This constructor needs to be hidden from the end user, but a method still needs to be made available for JSON serialization.
    [Obsolete("Don't use this manually.")]
    public BitBuilderDiff() { }

    public BitBuilderDiff(BitBuilderBuffer old, BitBuilderBuffer new_, DiffMethodType method) {
        Method = method;
        TargetHash = KnuthHash.Calculate(old);
        TargetLength = old.SerializedLengthBytes;
        ResultHash = KnuthHash.Calculate(new_);
        ResultLength = new_.SerializedLengthBytes;

        if (Method == DiffMethodType.replace) {
            ReplaceData = new();
            new_.WriteToStream(ReplaceData);
        }
        if (Method == DiffMethodType.patch) {
            PatchSegments = CreateDiffSegments(old, new_);
        }
        else throw new NotImplementedException();
    }

    public async Task ApplyToAsync(BitBuilderBuffer buffer) {
        if (Method == DiffMethodType.replace) {
            
        }

        throw new NotImplementedException();
    }

    // public async Task ApplyToAsync(string serialized_buffer_file_path) {
    //     using var fs = new FileStream(serialized_buffer_file_path, FileMode.Open);
    //     await ApplyToAsync(fs);
    //     fs.Flush();
    //     fs.Close();
    // }

    async Task ApplyToAsync(Stream stream) {
        await Task.Run(() => CheckHash(stream, TargetLength, TargetHash, "Unpatched"));
        
        if (Method == DiffMethodType.replace) await ApplyReplace(stream);
        else throw new NotImplementedException();

        await Task.Run(() => CheckHash(stream, ResultLength, ResultHash, "Patched"));
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

    static List<BinaryDiffSegment> CreateDiffSegments(BitBuilderBuffer old, BitBuilderBuffer new_) {
        var res = new List<BinaryDiffSegment>();

        long old_i_bits = 0;
        long new_i_bits = 0;
        var old_length_bits = old.DataLengthBits;
        var new_length_bits = new_.DataLengthBits;

        while (old_i_bits < old_length_bits || new_i_bits < new_length_bits) {

        }

        throw new NotImplementedException();
        return res;
    }
}


        // // Adam
        // var _world_of_warcraft = "boy.ssmh";
        // if (!string.IsNullOrWhiteSpace(_world_of_warcraft)) {
        //     Console.WriteLine("$");
        // }