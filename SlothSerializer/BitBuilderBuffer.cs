using System.Diagnostics;
using System.Text;
using SlothSerializer.Internal;

namespace SlothSerializer;

/// <summary> Serializes data to a ulong[]. Grows in size automatically. Does not shrink. </summary>
public class BitBuilderBuffer {
    const string FILE_HEADER_TEXT = "BitBuilder";

    List<ulong> Bits { get; set; } = new(); // swap for lowmemlist when brave enough
    public readonly BitBuilderWriter Writer; // note: the writer contains the final ulong.

    /// <summary> Total size in bits. </summary>
    public long TotalLength => 
        Bits.Count * 64 + Writer.XPos;

    public long TotalStreamLengthBytes =>
        Encoding.ASCII.GetByteCount(FILE_HEADER_TEXT) + 
        8 + // header
        8 + // size
        TotalLength / 8 + // bytes
        ((TotalLength % 8) > 0 ? 1 : 0); // trailing byte

    public ulong this[int i] => 
        i == Bits.Count ? Writer.Bits : Bits[i];

    public BitBuilderBuffer() =>
        Writer = new(Bits.Add);

    public string GetDebugString() =>
        string.Join('\n', Bits.Select(ul => Convert.ToString((long)ul, 2).PadLeft(64, '0'))) +
        $"\nWriter (Xpos={Writer.XPos}):\n{Convert.ToString((long)Writer.Bits, 2).PadLeft(64, '0')}";

    public BitBuilderReader GetReader() => 
        new(i => this[i], () => TotalLength);

    // Idk how useful this is. (very, never want to use the Writer property)
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
        Bits.Clear();
        Writer.Reset();
    }

    public bool Matches(BitBuilderBuffer b) =>
        Writer.Bits == b.Writer.Bits && Bits.SequenceEqual(b.Bits);

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
        stream.Write(BitConverter.GetBytes((ulong)0)); // reserve the next 8 bytes

        stream.Write(BitConverter.GetBytes(TotalLength));
        var bytes_count = TotalLength / 8;
        var bits_count = TotalLength - bytes_count * 8;

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
        var header = new byte[8];
        stream.Read(header);
        if (Encoding.ASCII.GetString(text_header) != FILE_HEADER_TEXT) throw new FileLoadException("The specified file is not of the right type.");

        var length_bytes = new byte[8];
        stream.Read(length_bytes);
        var total_length = BitConverter.ToInt64(length_bytes);
        var bytes_count = total_length / 8;
        var bits_count = total_length - bytes_count * 8;

        var buffer = new byte[bytes_count];
        stream.Read(buffer);
        Append(buffer); // why do interfaces have to be slow? rather speed this up instead of exposing something internal

        if (bits_count > 0) {
            var hanging_bits = (byte)stream.ReadByte();
            Debug.WriteLine(GetDebugString());
            for (int i = 0; i < bits_count; i++) {
                Append((hanging_bits & (128 >> i)) > 0);
            }
            Debug.WriteLine(GetDebugString());
        }
    }
}
