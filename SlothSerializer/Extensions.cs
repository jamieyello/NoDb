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

public static class GenericExtensions<T> {
    public static IEnumerable<T> Prepend(IEnumerable<T> values, IEnumerable<T> prepend_values) {
        foreach (var v in prepend_values) yield return v;
        foreach (var v in values) yield return v;
    }
}