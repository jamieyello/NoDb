using System.Text;
using SlothSerializer.Internal;

namespace SlothSerializer;

/// <summary> Reads data from a <see cref="BitBuilderBuffer"/>. Created from <see cref="BitBuilderBuffer.GetReader"/>. </summary>
/// <remarks> Starts from the beginning. Each reader created keeps its own independent position. <see cref="Position"/> can be set.
/// </remarks>
public class BitBuilderReader {
    readonly Func<int, ulong> _read_indexed;
    public long Position { get; set; }
    readonly Dictionary<Type, Func<long, object>> _read_methods;
    readonly Func<long> _get_total_length; // this should not be a long

    public ulong this[int i] => _read_indexed(i);
    public long Length => _get_total_length();

    // todo: make methods static
    public BitBuilderReader(Func<int, ulong> read_indexed, Func<long> get_total_length) {
        _read_indexed = read_indexed;
        _get_total_length = get_total_length;
        _read_methods = new() {
            { typeof(bool), (c) => ReadBool() },
            { typeof(sbyte), (c) => ReadSByte() },
            { typeof(byte), (c) => ReadByte() },
            { typeof(ushort), (c) => ReadUShort() },
            { typeof(short), (c) => ReadShort() },
            { typeof(char), (c) => ReadChar() },
            { typeof(decimal), (c) => ReadDecimal() },
            { typeof(uint), (c) => ReadUInt() },
            { typeof(int), (c) => ReadInt() },
            { typeof(ulong), (c) => ReadULong() },
            { typeof(long), (c) => ReadLong() },
            { typeof(DateTime), (c) => ReadDateTime() },
            { typeof(TimeSpan), (c) => ReadTimeSpan() },
            { typeof(string), (c) => ReadString() },

            { typeof(bool[]), ReadBools },
            { typeof(sbyte[]), ReadSBytes },
            { typeof(byte[]), ReadBytes },
            { typeof(ushort[]), ReadUShorts },
            { typeof(short[]), ReadShorts },
            { typeof(char[]), ReadChars },
            { typeof(decimal[]), ReadDecimals },
            { typeof(uint[]), ReadUInts },
            { typeof(int[]), ReadInts },
            { typeof(ulong[]), ReadULongs },
            { typeof(long[]), ReadLongs },
            { typeof(DateTime[]), ReadDateTimes },
            { typeof(TimeSpan[]), ReadTimeSpans },
            { typeof(string[]), ReadStrings },

            // unneeded
            //{ typeof(ObjectSerialationFlags), () => ReadObjectSerializationFlags() },
        };
    }

    // Debatable whether this should be here or not. (High level method in low level class)
    public T? Read<T>(SerializeMode mode = SerializeMode.Fields) =>
        (T?)BitBuilderSerializer.DeSerialize(typeof(T), this, mode);
    
    internal object Read(Type type, long? array_length = 0) =>
        _read_methods.TryGetValue(type, out var method) ?
            method.Invoke(array_length ?? 0) :
            throw new NotImplementedException();

    internal bool IsSupportedType(Type type) =>
        _read_methods.ContainsKey(type);

    void CheckCanReadAmount(long length) {
        if (Position + length > _get_total_length()) throw new Exception($"End of buffer reached.");
    }

    (byte XPos, int YPos) GetCoordinates() => 
        ((byte)(Position % 64), (int)Position / 64);

    public bool ReadBool() {
        var read = Read(1);
        var result = read & 1;
        return result > 0;
        // the following is broken but faster
        //CheckCanReadAmount(1);
        //var (x_pos, y_pos) = GetCoordinates();
        //Position++;
        //return (Bits.Bits[y_pos] >> x_pos & 1) > 0;
    }

    public sbyte ReadSByte() => (sbyte)Read(8);
    public byte ReadByte() => (byte)Read(8);

    public ushort ReadUShort() => (ushort)Read(16);
    public short ReadShort() => (short)Read(16);
    public char ReadChar() => (char)Read(16);

    public uint ReadUInt() => (uint)Read(32);
    public int ReadInt() => (int)Read(32);

    public ulong ReadULong() => Read(64);
    public long ReadLong() => (long)Read(64);

    public DateTime ReadDateTime() => new((long)Read(64), (DateTimeKind)Read(32));
    public TimeSpan ReadTimeSpan() => TimeSpan.FromTicks((long)Read(64));

    public decimal ReadDecimal() => new(ReadArray<int>(32, 4));

    public string ReadString() {
        var length = ReadInt();
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++) sb.Append(ReadChar());
        return sb.ToString();
    }

    public bool[] ReadBools(long count) => ReadArray<bool>(1, count);

    public sbyte[] ReadSBytes(long count) => ReadArray<sbyte>(8, count);
    public byte[] ReadBytes(long count) => ReadArray<byte>(8, count);

    public ushort[] ReadUShorts(long count) => ReadArray<ushort>(16, count);
    public short[] ReadShorts(long count) => ReadArray<short>(16, count);
    public char[] ReadChars(long count) => ReadArray<char>(16, count);

    public uint[] ReadUInts(long count) => ReadArray<uint>(32, count);
    public int[] ReadInts(long count) => ReadArray<int>(32, count);

    public ulong[] ReadULongs(long count) => ReadArray<ulong>(64, count);
    public long[] ReadLongs(long count) => ReadArray<long>(64, count);

    public DateTime[] ReadDateTimes(long count) {
        var result = new DateTime[count];
        for (long i = 0; i < count; i++) result[i] = ReadDateTime();
        return result;
    }

    public TimeSpan[] ReadTimeSpans(long count) {
        var result = new TimeSpan[count];
        for (long i = 0; i < count; i++) result[i] = ReadTimeSpan();
        return result;
    }

    public decimal[] ReadDecimals(long count) {
        var result = new decimal[count];
        for (long i = 0; i < count; i++) result[i] = ReadDecimal();
        return result;
    }

    public string[] ReadStrings(long count) {
        var result = new string[count];
        for (long i = 0; i < count; i++) result[i] = ReadString();
        return result;
    }

    T[] ReadArray<T>(byte length, long count) {
        CheckCanReadAmount(length * count);
        var result = new T[count];

        // Todo: speedup here with a more complex re-implementation --
        if (typeof(T) == typeof(long)) {
            // Compiler is dumb with safety checks (specifically ulong to long)
            for (long i = 0; i < count; i++) result[i] = (T)(object)(long)Read(length);
        }
        else {
            for (long i = 0; i < count; i++) result[i] = (T)(object)Read(length);
        }
        // --

        return result;
    }

    internal ObjectSerializationFlags ReadObjectSerializationFlags() {
        var flags = new ObjectSerializationFlags();
        flags.IsNull = ReadBool();
        if (flags.IsNull) return flags;
        flags.IsICollection = ReadBool();
        if (flags.IsICollection) flags.Length = ReadLong();
        flags.IsArray = ReadBool();
        if (flags.IsArray) flags.ArrayDimensionCount = ReadUShort();
        if (flags.IsArray) flags.ArrayLengths = ReadLongs(flags.ArrayDimensionCount);
        return flags;
    }

    ulong Read(byte length) {
        CheckCanReadAmount(length);
        var (x_pos, y_pos) = GetCoordinates();
        Position += length;
        var remainder = 64 - length;

        if (x_pos <= remainder) {
            var r = _read_indexed(y_pos) >> remainder - x_pos;
            return r;
        }
        else if (x_pos == remainder) return _read_indexed(y_pos);
        else {
            var result = _read_indexed(y_pos) << x_pos - remainder;
            result |= _read_indexed(y_pos + 1) >> 64 - (x_pos + length) % 64;
            return result;
        }
    }

    public ulong[] ToArray() {
        var length = _get_total_length();
        var result = new ulong[length];
        var span = result.AsSpan();
        for (int i = 0; i < length; i++) span[i] = _read_indexed(i);
        return result;
    }
}
