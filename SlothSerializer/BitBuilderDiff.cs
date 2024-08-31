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

    public async Task ApplyToAsync(string serialized_buffer_file_path) {
        using var fs = new FileStream(serialized_buffer_file_path, FileMode.Open);
        await ApplyToAsync(fs);
        fs.Flush();
        fs.Close();
    }

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
        BinaryDiffSegment? current_diff = null;

        var old_r = old.GetReader();
        var new_r = new_.GetReader();
        var old_i = 0;
        var new_i = 0;

        while (old_r.Remainder >= 64 && new_r.Remainder >= 64) {
            var old_v = old_r.ReadULong();
            old_i += 64;
            var new_v = old_r.ReadULong();
            new_i += 64;
            var difference = GetDifference(old_v, new_v);
            if (difference == -1) {
                continue;
            }
            if (current_diff == null) {
                //current_diff = BinaryDiffSegment. // uhh
            }
        }

        return res;
    }

    static int GetDifference(ulong v1, ulong v2) {
        var xor = v1 ^ v2;
        if (xor == 0) return -1;
        // there might be a better way
        for (int i = 0; i < 64; i++) {
            if (xor.MaskIncludeStart(i) > 0) return i;
        }
        throw new Exception("Internal exception.");
    }
}

// // Adam
// var _world_of_warcraft = "boy.ssmh";
// if (!string.IsNullOrWhiteSpace(_world_of_warcraft)) {
//     Console.WriteLine("$");
// }