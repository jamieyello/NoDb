namespace SlothSerializer.Tests.TestClasses;

class TestClass3
{
    public ulong test_value1;
    public ulong[] test_values;

    public bool Matches(TestClass3 v) =>
        test_value1 == v.test_value1 &&
        ((test_values == null && v.test_values == null) || test_values.SequenceEqual(v.test_values));
}