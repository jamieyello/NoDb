namespace SlothSerializer.Tests.TestClasses;

public class TestUser {
    public int Id {get;set;}
    public string Name {get;set;}

    public bool Matches(TestUser? obj)
    {
        if (obj == null) return false;

        return obj.Id == Id && obj.Name == Name;
    }
}