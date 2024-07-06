namespace SlothSerializer.Tests.TestClasses;

class TestClass2
{
    public ulong test_value;
    public ulong[] test_array;
    public ulong[,] test_array2;
    public string test_string;
    public string test_string2;
    public TestClass1 test_class;

    public bool Matches(TestClass2 t) =>
        test_value == t.test_value &&
        (test_array == null && t.test_array == null) || test_array.SequenceEqual(t.test_array) &&
        Array2DMatches(test_array2, t.test_array2) &&
        test_string == t.test_string &&
        test_string2 == t.test_string2 &&
        test_class.Matches(t.test_class);

    static bool Array2DMatches<T>(T?[,] array1, T?[,] array2) {
        var null_match = (array1 == null) == (array2 == null);
        if (null_match && array1 == null) return true;
        if (!null_match) return false;

        // I know it's not null here damnit
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        if ((array1.GetLength(0) != array2.GetLength(0)) || (array1.GetLength(1) != array2.GetLength(1))) return false;
        for (int x = 0; x < array1.GetLength(0); x++)
        {
            for (int y = 0; y < array1.GetLength(1); y++)
            {
                if (!array1[x, y].Equals(array2[x, y])) return false;
            }
        }
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return true;
    }
}