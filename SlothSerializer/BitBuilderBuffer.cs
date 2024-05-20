using SlothSerializer.Internal;

namespace SlothSerializer;

/// <summary> Serializes data to a ulong[]. Grows in size automatically. Does not shrink. </summary>
public class BitBuilderBuffer {
    List<ulong> Bits { get; set; } = new() { }; // swap for lowmemlist when brave enough
    public readonly BitBuilderWriter Writer; // note: the writer contains the final ulong.

    /// <summary> Total size in bits. </summary>
    public long TotalLength => 
        Bits.Count * 64 + Writer.XPos;

    public ulong this[int i] => 
        i == Bits.Count ? Writer.Bits : Bits[i];

    public BitBuilderBuffer() =>
        Writer = new(Bits.Add);

    public string GetDebugString() =>
        string.Join('\n', Bits.Select(ul => Convert.ToString((long)ul, 2).PadLeft(64, '0')));

    public BitBuilderReader GetReader() => 
        new(i => this[i], () => TotalLength);

    // Idk how useful this is.
    public void Append(bool value) => Writer.Append(value);
    public void Append(sbyte value) => Writer.Append(value);
    public void Append(byte value) => Writer.Append(value);
    public void Append(ushort value) => Writer.Append(value);
    public void Append(short value) => Writer.Append(value);
    public void Append(char value) => Writer.Append(value);
    public void Append(uint value) => Writer.Append(value);
    public void Append(int value) => Writer.Append(value);
    public void Append(ulong value) => Writer.Append(value);
    public void Append(long value) => Writer.Append(value);
    public void Append(DateTime value) => Writer.Append(value);
    public void Append(TimeSpan value) => Writer.Append(value);
    public void Append(decimal value) => Writer.Append(value);
    public void Append(string value) => Writer.Append(value);
    internal void Append(ObjectSerializationFlags object_flags) => Writer.Append(object_flags);
    public void Append(object? obj, SerializeMode mode = SerializeMode.Properties) => Writer.Append(obj, mode);
    public void Append(IList<bool> value) => Writer.Append(value);
    public void Append(IList<sbyte> value) => Writer.Append(value);
    public void Append(IList<byte> value) => Writer.Append(value);
    public void Append(IList<ushort> value) => Writer.Append(value);
    public void Append(IList<short> value) => Writer.Append(value);
    public void Append(IList<char> value) => Writer.Append(value);
    public void Append(IList<uint> value) => Writer.Append(value);
    public void Append(IList<int> value) => Writer.Append(value);
    public void Append(IList<ulong> value) => Writer.Append(value);
    public void Append(IList<long> value) => Writer.Append(value);
    public void Append(IList<string> value) => Writer.Append(value);

    public void Clear() {
        Bits.Clear();
        Bits.Add(0);
        Writer.XPos = 0;
    }

    public bool Matches(BitBuilderBuffer b) =>
        Writer.Bits == b.Writer.Bits && Bits.SequenceEqual(b.Bits);
}
