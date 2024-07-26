namespace NoDb.Syncers;

public class LobbySyncer : Syncer
{
    public LobbyServerConfig Config => (LobbyServerConfig)_config;
    readonly MemoryStream bb_shared_data = new();

    public LobbySyncer(LobbyServerConfig config) : base(config)
    {
    }
}