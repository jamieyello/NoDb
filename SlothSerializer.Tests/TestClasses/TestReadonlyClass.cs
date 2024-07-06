namespace SlothSerializer.Tests.TestClasses;

[Serializable]
public struct TestReadOnlyClass
{
    private readonly int value;

    public TestReadOnlyClass()
    {

    }
    public TestReadOnlyClass(int value)
    {
        this.value = value;
    }
}