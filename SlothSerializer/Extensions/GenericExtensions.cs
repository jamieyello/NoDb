namespace SlothSerializer;

public static class GenericExtensions<T> {
    public static IEnumerable<T> Prepend(IEnumerable<T> values, IEnumerable<T> prepend_values) {
        foreach (var v in prepend_values) yield return v;
        foreach (var v in values) yield return v;
    }

    public static IEnumerable<T> EnumerateParams(params T[] objects) {
        foreach (var o in objects) yield return o;
    }
}