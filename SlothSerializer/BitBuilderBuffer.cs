using System.Diagnostics;
using System.Text;
using SlothSerializer.Internal;

namespace SlothSerializer;

/// <summary> Serializes data to a ulong[]. Grows in size automatically. Does not shrink. </summary>
public class BitBuilderBuffer {
    const string FILE_HEADER_TEXT = "BitBuilder";

    List<ulong> _bits { get; set; } = new(); // swap for lowmemlist when brave enough
    internal readonly BitBuilderWriter Writer; // note: the writer contains the final ulong.

    /// <summary> Total size in bits. Does not always divide by 8 evenly. </summary>
    public long TotalLengthBits => 
        _bits.Count * 64 + Writer.XPos;

    public long TotalStreamLengthBytes =>
        Encoding.ASCII.GetByteCount(FILE_HEADER_TEXT) + 
        8 + // header
        8 + // size
        TotalLengthBits / 8 + // bytes
        ((TotalLengthBits % 8) > 0 ? 1 : 0); // trailing byte

    ulong HeaderData => 0ul;

    public ulong this[int i] => 
        i == _bits.Count ? Writer.Bits : _bits[i];

    public BitBuilderBuffer() =>
        Writer = new(_bits.Add);

    public string GetDebugString() =>
        string.Join('\n', _bits.Select(ul => Convert.ToString((long)ul, 2).PadLeft(64, '0'))) +
        $"\nWriter (Xpos={Writer.XPos}):\n{Convert.ToString((long)Writer.Bits, 2).PadRight(64, '0')[..Writer.XPos].PadRight(64, '-')}";

    public BitBuilderReader GetReader() => 
        new(i => this[i], () => TotalLengthBits);

    public BitBuilderStream GetStream() => 
        new(this);

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

    // todo: change all these to readonlyspan.
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
        _bits.Clear();
        Writer.Reset();
    }

    public bool Matches(BitBuilderBuffer b) =>
        Writer.Bits == b.Writer.Bits && _bits.SequenceEqual(b._bits);

    public async Task WriteToDiskAsync(string file_path) =>
        await Task.Run(() => WriteToDisk(file_path));

    /// <summary> Creates a new file and writes the contents of this buffer to it. Overwrites existing file if it exists. </summary>
    public void WriteToDisk(string file_path) {
        var dir = Path.GetDirectoryName(file_path);
        if (!string.IsNullOrWhiteSpace(dir)) Directory.CreateDirectory(dir);
        if (File.Exists(file_path)) File.Delete(file_path);

        using var fs = new FileStream(file_path, FileMode.CreateNew);
        WriteToStream(fs);
        fs.Close();
    }

    public void WriteToStream(Stream stream) {
        stream.Write(Encoding.ASCII.GetBytes(FILE_HEADER_TEXT));
        stream.Write(BitConverter.GetBytes(HeaderData));
        stream.Write(BitConverter.GetBytes(TotalLengthBits));
        var bytes_count = TotalLengthBits / 8;
        var bits_count = TotalLengthBits - bytes_count * 8;

        var r = GetReader();
        stream.Write(r.ReadBytes(bytes_count));

        var hanging_bits = r.ReadBools(bits_count);
        byte hanging_bits_byte = 0;
        foreach (var hb in hanging_bits) {
            hanging_bits_byte <<= 1;
            hanging_bits_byte |= hb ? (byte)1 : (byte)0;
        }
        hanging_bits_byte <<= (byte)(8 - bits_count);
        stream.WriteByte(hanging_bits_byte);
        stream.Flush();
    }

    public async Task ReadFromDiskAsync(string file_path) =>
        await Task.Run(() => ReadFromDisk(file_path));

    public void ReadFromDisk(string file_path) {
        Clear();

        using var fs = new FileStream(file_path, FileMode.Open);
        ReadFromStream(fs);
        fs.Close();
    }

    public void ReadFromStream(Stream stream) {
        var text_header = new byte[FILE_HEADER_TEXT.Length];
        stream.Read(text_header);
        var text_header_str = Encoding.ASCII.GetString(text_header);

        var header = new byte[8];
        stream.Read(header);

        var total_length_bits_arr = new byte[8];
        stream.Read(total_length_bits_arr);
        var total_length_bits = BitConverter.ToInt64(total_length_bits_arr);

        if (text_header_str != FILE_HEADER_TEXT) throw new DataMisalignedException("Invalid header from data stream.");

        var bytes_count = total_length_bits / 8;
        var bits_count = total_length_bits - bytes_count * 8;

        var buffer = new byte[bytes_count];
        stream.Read(buffer);
        Append(buffer); // why do interfaces have to be slow? rather speed this up instead of exposing something internal

        if (bits_count > 0) {
            var hanging_bits = (byte)stream.ReadByte();
            for (int i = 0; i < bits_count; i++) {
                Append((hanging_bits & (128 >> i)) > 0);
            }
        }
    }

    // Todo: throw exception if a write method is accessed while this is happening
    public IEnumerable<byte> EnumerateAsBytes() {
        foreach (var b in Encoding.ASCII.GetBytes(FILE_HEADER_TEXT)) yield return b;
        foreach (var b in BitConverter.GetBytes(HeaderData)) yield return b;
        foreach (var b in BitConverter.GetBytes(TotalLengthBits)) yield return b;

        foreach (var ul in _bits) {
            yield return (byte)(ul >> 56);
            yield return (byte)(ul >> 48);
            yield return (byte)(ul >> 40);
            yield return (byte)(ul >> 32);
            yield return (byte)(ul >> 24);
            yield return (byte)(ul >> 16);
            yield return (byte)(ul >> 8);
            yield return (byte)ul;
        }

        var hanging_bit_count = Writer.XPos % 8 > 0 ? 1 : 0;
        for (int i = 0; i < Writer.XPos / 8 + hanging_bit_count; i++) {
            yield return (byte)(Writer.Bits >> 56 - i * 8);
        }
    }
}
