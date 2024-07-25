# NoDb
A library that helps you sync objects in C# code with various sources. In active development, so it's not ready for use just yet.

## File Syncing
```
class User {
    public string? Name { get; set; }
    public int LoadCount { get; set; }
}

public class ProgramState {
    // Use a static item to keep your data loaded. Loads asynchronously on declaration.
    public static FileObject<User> UserData = new("SaveData.save", new() { Name = "Jamie"});

    public void Modify() {
        // Automatically waits for the data to load before accessing.
        UserData.LoadCount++;
        // Call whenever you want to save the data.
        UserData.Sync();
    }

    // It also supports using IDisposable to sync your files.
    public static void TestObjectFileSync() {
        using var user_data = new FileObject<User>("TestObjectFileSync.deleteme", new() { Name = "Jamie"});
        user_data.Value.LoadCount++;
    }
}
```
