using System.Text;
using SlothSerializer.Internal;

namespace SlothSerializer;

/// <summary> Serializes data to a ulong[]. Uses space efficiently, a bool only takes 1 bit of space. </summary>
public class BitBuilderBuffer {
    const string FILE_HEADER_TEXT = "BBBuff__";

    internal readonly SegmentedList<ulong> _bits = new(); // swap for lowmemlist when brave enough
    internal readonly BitBuilderWriter _writer; // note: the writer contains the final ulong.

    internal long HeaderLengthBytes =>
        Encoding.ASCII.GetByteCount(FILE_HEADER_TEXT) + 
        8 + // header
        8 + // size
        8; // hash

    /// <summary> Total length of data in bits. Does not always divide by 8 evenly. </summary>
    public long DataLengthBits => 
        _bits.Count * 64 + _writer.XPos;

    /// <summary> Total size of serialized <see cref="BitBuilderBuffer"/>. </summary>
    public long SerializedLengthBytes =>
        HeaderLengthBytes +
        DataLengthBits / 8 + // bytes
        ((DataLengthBits % 8) > 0 ? 1 : 0); // trailing byte

    ulong HeaderData =>
        0ul;

    public ulong this[int i] => 
        i == _bits.Count ? _writer.Bits : _bits[i];

    public IEnumerable<ulong> this[Range range] {
        get {
            var end = Math.Min(_bits.Count, range.End.Value);
            for (int i = range.Start.Value; i < end; i++) {
                yield return _bits[i];
            }
            if (range.End.Value == _bits.Count) yield return _writer.Bits;
        }
    }

    public BitBuilderBuffer() =>
        _writer = new(_bits.Add);

    public string DebugString =>
        string.Join('\n', _bits.Select(ul => Convert.ToString((long)ul, 2).PadLeft(64, '0'))) +
        $"\nWriter (Xpos={_writer.XPos}):\n{Convert.ToString((long)_writer.Bits, 2).PadRight(64, '0')[.._writer.XPos].PadRight(64, '-')}";

    public BitBuilderReader GetReader() => 
        new(i => this[i], () => DataLengthBits);

    public void Append(bool value) => _writer.Append(value);
    public void Append(sbyte value) => _writer.Append(value);
    public void Append(byte value) => _writer.Append(value);
    public void Append(ushort value) => _writer.Append(value);
    public void Append(short value) => _writer.Append(value);
    public void Append(char value) => _writer.Append(value);
    public void Append(uint value) => _writer.Append(value);
    public void Append(int value) => _writer.Append(value);
    public void Append(ulong value) => _writer.Append(value);
    public void Append(long value) => _writer.Append(value);
    public void Append(DateTime value) => _writer.Append(value);
    public void Append(TimeSpan value) => _writer.Append(value);
    public void Append(decimal value) => _writer.Append(value);
    public void Append(string value) => _writer.Append(value);
    internal void Append(ObjectSerializationFlags object_flags) => _writer.Append(object_flags);
    public void Append(object? obj, SerializeMode mode = SerializeMode.Properties) => _writer.Append(obj, mode);

    // todo: change all these to readonlyspan.
    public void Append(IList<bool> value) => _writer.Append(value);
    public void Append(IList<sbyte> value) => _writer.Append(value);
    public void Append(IList<byte> value) => _writer.Append(value);
    public void Append(IList<ushort> value) => _writer.Append(value);
    public void Append(IList<short> value) => _writer.Append(value);
    public void Append(IList<char> value) => _writer.Append(value);
    public void Append(IList<uint> value) => _writer.Append(value);
    public void Append(IList<int> value) => _writer.Append(value);
    public void Append(IList<ulong> value) => _writer.Append(value);
    public void Append(IList<long> value) => _writer.Append(value);
    public void Append(IList<string> value) => _writer.Append(value);

    public void Clear() {
        _bits.Clear();
        _writer.Reset();
    }

    public bool Matches(BitBuilderBuffer b) =>
        _writer.Bits == b._writer.Bits && _bits.SequenceEqual(b._bits);

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

    public void WriteToStream(Stream stream, bool include_header = true) {
        if (include_header) {
            stream.Write(Encoding.ASCII.GetBytes(FILE_HEADER_TEXT));
            stream.Write(BitConverter.GetBytes(HeaderData));
            stream.Write(BitConverter.GetBytes(DataLengthBits));
            stream.Write(BitConverter.GetBytes(GetHash()));
        }
        var bytes_count = DataLengthBits / 8;
        var bits_count = DataLengthBits - bytes_count * 8;

        var r = GetReader();
        stream.Write(r.ReadBytes(bytes_count));

        if (bits_count > 0) {
            var hanging_bits = r.ReadBools(bits_count);
            byte hanging_bits_byte = 0;
            foreach (var hb in hanging_bits) {
                hanging_bits_byte <<= 1;
                hanging_bits_byte |= hb ? (byte)1 : (byte)0;
            }
            hanging_bits_byte <<= (byte)(8 - bits_count);
            stream.WriteByte(hanging_bits_byte);
        }

        stream.Flush();
    }

    public void ReadFromArray(byte[] serialized_bitbuilder) {
        using var ms = new MemoryStream(serialized_bitbuilder);
        ReadFromStream(ms);
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

        var hash = new byte[8];
        stream.Read(hash);

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

    internal IEnumerable<byte> EnumerateHeader() {
        foreach (var b in Encoding.ASCII.GetBytes(FILE_HEADER_TEXT)) yield return b;
        foreach (var b in BitConverter.GetBytes(HeaderData)) yield return b;
        foreach (var b in BitConverter.GetBytes(DataLengthBits)) yield return b;
        foreach (var b in BitConverter.GetBytes(GetHash())) yield return b;
    }

    // Todo: throw exception if a write method is accessed while this is happening
    public IEnumerable<byte> EnumerateAsBytes(bool include_header = true) {
        if (include_header) {
            foreach (var b in EnumerateHeader()) yield return b;
        }

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

        var hanging_bit_count = _writer.XPos % 8 > 0 ? 1 : 0;
        for (int i = 0; i < _writer.XPos / 8 + hanging_bit_count; i++) {
            yield return (byte)(_writer.Bits >> 56 - i * 8);
        }
    }

    public ulong GetHash() =>
        KnuthHash.Calculate(GenericExtensions<ulong>.EnumerateParams(
            _bits.GetHash(),
            _writer.Bits,
            _writer.XPos
        ));
}
