﻿using SlothSerializer.Internal;

namespace SlothSerializer.DiffTracking;

public class BinaryDiff
{
    public enum DiffMethodType
    {
        replace
    }

    DiffMethodType Method { get; set; }
    MemoryStream PatchData { get; set; } = new(); // this is completely arbituary, data depends on the method used.

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
            PatchData.Position = 0;
            PatchData.SetLength(0);
            new_.WriteToStream(PatchData);
        }
        else throw new NotImplementedException();
    }

    // public BinaryDiff(byte[] old, byte[] new_, DiffMethodType method) {
    //     Method = method;
    //     TargetHash = KnuthHash.Calculate(old);
    //     TargetLength = old.Length;
    //     ResultHash = KnuthHash.Calculate(new_);
    //     ResultLength = new_.Length;

    //     if (Method == DiffMethodType.replace) {
    //         PatchData.Position = 0;
    //         PatchData.SetLength(0);
    //         PatchData.Write(new_);
    //     }
    //     else throw new NotImplementedException();
    // }

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

    static async Task ApplyReplace(Stream patch, Stream stream) {
        stream.Position = 0;
        stream.SetLength(patch.Length);
        patch.Position = 0;
        await patch.CopyToAsync(stream);
        stream.Position = 0;
    }

    static void CheckHash(Stream stream, long expected_length, ulong expected_hash, string patched_or_unpatched) {
        stream.Position = 0;
        if (stream.Length != expected_length) throw new Exception($"{patched_or_unpatched} length mismatch. Expected:{expected_length} Read:{stream.Length}");
        if (expected_hash != KnuthHash.Calculate(stream)) throw new Exception($"Hash check failure. {patched_or_unpatched} result differs from expected result.");
        stream.Position = 0;
    }

    // here for debugging
    public static T? FromBitBuilderStream<T>(MemoryStream stream, SerializeMode mode) {
        var bb = new BitBuilderBuffer();
        stream.Position = 0;
        bb.ReadFromStream(stream);
        stream.Position = 0;
        return bb.GetReader().Read<T>(mode);
    }
}