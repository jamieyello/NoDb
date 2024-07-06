namespace SlothSerializer.Tests.TestClasses;

public class TestPropClass {
    public int TestValue1 { get; set; }
    public int TestValue2 { get; init; }
    int TestValue3 { get; set; }
    public int TestValue4 { get; private set; }

    public void SetTestValue3(int value) => 
        TestValue3 = value;

    public void SetTestValue4(int value) =>
        TestValue4 = value;

    public bool Matches(TestPropClass t) =>
        t.TestValue1 == TestValue1 &&
        t.TestValue2 == TestValue2 &&
        t.TestValue3 == TestValue3 &&
        t.TestValue4 == TestValue4;
}