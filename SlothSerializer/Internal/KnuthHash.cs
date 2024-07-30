namespace SlothSerializer.Internal;

// https://stackoverflow.com/a/9545731
public static class KnuthHash {
    public static ulong Calculate(BitBuilderBuffer buffer) {
        ulong hashedValue = 3074457345618258791ul;
        foreach (var b in buffer.EnumerateAsBytes()) {
            hashedValue += b;
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static ulong Calculate(Stream stream) {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < stream.Length; i++) {
            hashedValue += (byte)stream.ReadByte();
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static ulong Calculate(ReadOnlySpan<byte> span) {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < span.Length; i++) {
            hashedValue += span[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
    
    public static ulong Calculate(ReadOnlySpan<ulong> span) {
        ulong hashedValue = 3074457345618258791ul;
        for (int i = 0; i < span.Length; i++) {
            hashedValue += span[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static ulong Calculate(IEnumerable<ulong> items) {
        ulong hashedValue = 3074457345618258791ul;
        foreach (var item in items) {
            hashedValue += item;
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    public static ulong Calculate(IEnumerable<int> items) {
        ulong hashedValue = 3074457345618258791ul;
        foreach (var item in items) {
            hashedValue += (ulong)item;
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
}