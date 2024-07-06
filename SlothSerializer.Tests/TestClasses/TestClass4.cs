namespace SlothSerializer.Tests.TestClasses;

public class TestClass4
{
    public enum TestEnum
    {
        first,
        second,
        third
    }

    public List<ulong> test_list;
    public KeyValuePair<ulong, ulong> test_kvp;
    public Dictionary<ulong, ulong> test_dictionary;
    public TestEnum test_enum;

    public bool Matches(TestClass4 t) =>
        test_list.SequenceEqual(t.test_list) &&
        test_kvp.Key == t.test_kvp.Key && test_kvp.Value == t.test_kvp.Value &&
        test_dictionary.SequenceEqual(t.test_dictionary) &&
        test_enum.Equals(t.test_enum);
}