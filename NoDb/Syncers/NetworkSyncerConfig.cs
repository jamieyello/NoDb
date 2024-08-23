namespace NoDb.Syncers;

public class NetworkSyncerConfig : SyncerConfig {
    /// <summary> Server or client. </summary>
    public enum ConnectionType {
        server,
        client
    }

    /// <summary> Server or client. </summary>
    public required ConnectionType Connection { get; set; }
    /// <summary> If server, the port to listen on. If client, the port to connect to. </summary>
    public required int Port;
    /// <summary> Not used if server. </summary>
    public required string IpAddress { get; set; }
}