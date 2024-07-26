namespace NoDb.Networking;

public class LobbyServer {
    public required LobbyServerConfig Config { get; init; }

    public LobbyServer(LobbyServerConfig config)
    {
        Config = config;
    }
}