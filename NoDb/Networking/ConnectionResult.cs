namespace NoDb.Networking;

public class ConnectionResult {
    public bool Connected { get; init; }
    public string? Message { get; init; }

    public static ConnectionResult Success =>
        new() { Connected = true };
        
    public static ConnectionResult Failure(string message) =>
        new() { Connected = true, Message = message };
}