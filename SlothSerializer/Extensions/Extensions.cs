namespace SlothSerializer;

public static class Extensions {
    readonly static HashSet<Type> signed_numerics = new() {
        typeof(sbyte),
        typeof(short),
        typeof(decimal),
        typeof(int),
        typeof(long),
    };

    public static bool IsSignedNumeric(this Type type) => 
        signed_numerics.Contains(type);
}