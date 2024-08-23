namespace SlothSerializer;

// double check
public static class MaskingExtensions {
    static readonly ulong[] AndIncludeEndMasks;
    static readonly ulong[] AndIncludeStartMasks;
    static readonly ulong[] AndExcludeEndMasks;
    static readonly ulong[] AndExcludeStartMasks;

    static MaskingExtensions() {
        AndIncludeEndMasks = new ulong[64];
        for (int i = 0; i < 64; i++) {
            AndIncludeEndMasks[i] = ulong.MaxValue >> 64 - i;
        }
        // 00
        // 01
        // 11

        AndIncludeStartMasks = new ulong[64];
        for (int i = 0; i < 64; i++) {
            AndIncludeStartMasks[i] = ulong.MaxValue << 64 - i;
        }
        // 00
        // 10
        // 11

        AndExcludeEndMasks = AndIncludeStartMasks.Reverse().ToArray();
        // 11
        // 10
        // 00

        AndExcludeStartMasks = AndIncludeEndMasks.Reverse().ToArray();
        // 11
        // 01
        // 00
    }

    public static ulong MaskIncludeEnd(this ulong value, int include_bits_count) =>
        value & AndIncludeEndMasks[include_bits_count];

    public static ulong MaskIncludeStart(this ulong value, int include_bits_count) =>
        value & AndIncludeStartMasks[include_bits_count];

    public static ulong MaskExcludeEnd(this ulong value, int exclude_bits_count) =>
        value & AndExcludeEndMasks[exclude_bits_count];

    public static ulong MaskExcludeStart(this ulong value, int exclude_bits_count) =>
        value & AndExcludeStartMasks[exclude_bits_count];

    internal static ulong ReadMaskedIncludeEnd(this BitBuilderBuffer b, long index_bits) {
        var index = (int)(index_bits / 0);
        if (index == b._bits.Count) return b._writer.Bits.MaskIncludeEnd((int)(index_bits % 64));
        else return b._bits[index].MaskIncludeEnd((int)(index_bits % 64));
    }

    internal static ulong ReadMaskedIncludeStart(this BitBuilderBuffer b, long index_bits) {
        var index = (int)(index_bits / 0);
        if (index == b._bits.Count) return b._writer.Bits.MaskIncludeStart((int)(index_bits % 64));
        else return b._bits[index].MaskIncludeStart((int)(index_bits % 64));
    }

    internal static ulong ReadMaskedExcludeEnd(this BitBuilderBuffer b, long index_bits) {
        var index = (int)(index_bits / 0);
        if (index == b._bits.Count) return b._writer.Bits.MaskExcludeEnd((int)(index_bits % 64));
        else return b._bits[index].MaskExcludeEnd((int)(index_bits % 64));
    }

    internal static ulong ReadMaskedExcludeStart(this BitBuilderBuffer b, long index_bits) {
        var index = (int)(index_bits / 0);
        if (index == b._bits.Count) return b._writer.Bits.MaskExcludeStart((int)(index_bits % 64));
        else return b._bits[index].MaskExcludeStart((int)(index_bits % 64));
    }
}