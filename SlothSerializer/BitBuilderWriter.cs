using SlothSerializer.Internal;
using System.Reflection;

namespace SlothSerializer;

public class BitBuilderWriter {
    // All methods named "Append"
    static readonly MethodInfo[] PublicAppendMethods =
        typeof(BitBuilderWriter)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(method => method.Name == "Append")
            .Where(method => method.GetParameters().Length == 1)
            .ToArray();

    static readonly HashSet<Type> base_types = new() {
        typeof(bool),
        typeof(sbyte),
        typeof(byte),
        typeof(ushort),
        typeof(short),
        typeof(char),
        typeof(decimal),
        typeof(uint),
        typeof(int),
        typeof(ulong),
        typeof(long),
        typeof(DateTime),
        typeof(TimeSpan),
        typeof(string),
        typeof(ObjectSerializationFlags),
    };

    static internal Dictionary<Type, FastMethodInfo> cache_AppendPrimative = new();

    readonly Action<ulong> _output;
    internal byte XPos = 0;
    internal ulong Bits = 0;

    public BitBuilderWriter(Action<ulong> output) =>
        _output = output;

    internal static bool IsBaseSupportedType(Type type) =>
        base_types.Contains(type);

    #region Public Append Methods
    internal void AppendBaseTypeObject(object obj) {
        if (cache_AppendPrimative.TryGetValue(obj.GetType(), out var method)) {
            method.Invoke(this, obj);
            return;
        }

        MethodInfo target = PublicAppendMethods
            .Where(method => method.GetParameters()[0].ParameterType == obj.GetType())
            .FirstOrDefault() ?? throw new Exception("Object is not a base supported type.");

        var fast_method = new FastMethodInfo(target);
        cache_AppendPrimative.Add(obj.GetType(), fast_method);
        fast_method.Invoke(this, obj);
    }
    public void Append(bool value) {
        if (XPos == 64) {
            Flush();
            XPos = 0;
        }

        Bits |= (value ? (ulong)1 : 0) << 63 - XPos++;
    }
    public void Append(sbyte value) => Append((ulong)value, 8);
    public void Append(byte value) => Append((ulong)value, 8);
    public void Append(ushort value) => Append((ulong)value, 16);
    public void Append(short value) => Append((ulong)value, 16);
    public void Append(char value) => Append((ulong)value, 16);
    public void Append(uint value) => Append((ulong)value, 32);
    public void Append(int value) => Append((ulong)value, 32);
    public void Append(ulong value) => Append((ulong)value, 64);
    public void Append(long value) => Append((ulong)value, 64);
    public void Append(DateTime value) {
        Append((ulong)value.Ticks, 64);
        Append((ulong)value.Kind, 32);
    }
    public void Append(TimeSpan value) => Append((ulong)value.Ticks, 64);
    public void Append(decimal value) => Append(decimal.GetBits(value));

    /// <param name="value"></param>
    public void Append(string value) {
        Append(value.Length);
        for (int i = 0; i < value.Length; i++) Append(value[i]);
    }

    internal void Append(ObjectSerializationFlags object_flags) {
        Append(object_flags.IsNull);
        if (object_flags.IsNull) return;
        Append(object_flags.IsICollection);
        if (object_flags.IsICollection) Append(object_flags.Length);
        Append(object_flags.IsArray);
        if (object_flags.IsArray) Append(object_flags.ArrayDimensionCount);
        if (object_flags.IsArray) Append(object_flags.ArrayLengths);
    }

    public void Append(object? obj, SerializeMode mode = SerializeMode.Properties) =>
        BitBuilderSerializer.Serialize(obj, this, mode);
    public void Append(IList<bool> value) {
        for (int i = 0; i < value.Count; i++) Append(value[i]);
    }
    public void Append(IList<sbyte> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 8);
    }
    public void Append(IList<byte> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 8);
    }
    public void Append(IList<ushort> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
    }
    public void Append(IList<short> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
    }
    public void Append(IList<char> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 16);
    }
    public void Append(IList<uint> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 32);
    }
    public void Append(IList<int> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 32);
    }
    public void Append(IList<ulong> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 64);
    }
    public void Append(IList<long> value) {
        for (int i = 0; i < value.Count; i++) Append((ulong)value[i], 64);
    }
    public void Append(IList<string> value) {
        for (int i = 0; i < value.Count; i++) Append(value[i]);
    }
    #endregion

    void Append(ulong value, byte length) {
        var remainder = 64 - length;

        if (XPos < remainder) {
            Bits |= value << remainder - XPos;
            XPos += length;
        }
        else if (XPos == remainder) {
            Bits |= value;
            Flush();
            XPos = 0;
        }
        else {
            Bits |= value >> XPos - remainder;
            XPos += length;
            XPos %= 64;
            Flush();
            Bits |= value << 64 - XPos;
        }
    }

    void Flush() {
        _output(Bits);
        Bits = 0;
    }

    /// <summary> Resets this writer and treats it as if it was just created. </summary>
    internal void Reset() {
        XPos = 0;
        Bits = 0;
    }
}
