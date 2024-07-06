namespace SlothSerializer.Tests.TestClasses;

struct TestClass1
{
    public ulong test1;
    public ulong test2;
    public ulong test3;
    public ulong test4;
    public string test_string;

    public readonly bool Matches(TestClass1 t) =>
        test3 == t.test3 &&
        test_string == t.test_string;
}