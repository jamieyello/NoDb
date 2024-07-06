namespace SlothSerializer.Tests.TestClasses;

class TestClass5
{
    public byte test_value1;
    public byte[] test_values;

    public bool Matches(TestClass5 v) =>
        test_value1 == v.test_value1 &&
        ((test_values == null && v.test_values == null) || test_values.SequenceEqual(v.test_values));
}