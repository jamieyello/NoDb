namespace SlothSerializer.Internal;

public static class KnuthHash {
    public static ulong Calculate(BitBuilderBuffer buffer) {
        ulong hashedValue = 3074457345618258791ul;
        foreach (var b in buffer.EnumerateAsBytes()) {
            hashedValue += b;
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }

    // https://stackoverflow.com/a/9545731
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
}