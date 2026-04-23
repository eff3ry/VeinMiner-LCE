using Minecraft.Server.FourKit.Entity;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VeinMiner_LCE;

public sealed class PlayerDataStore
{
    public const string DefaultFileName = "player-data.json";

    private const int CurrentVersion = 1;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public int Version { get; set; } = CurrentVersion;

    public Dictionary<Guid, PlayerDataEntry> Players { get; set; } = new();

    public static PlayerDataStore LoadOrCreate(string filePath)
    {
        if (!File.Exists(filePath))
        {
            PlayerDataStore created = new();
            created.Save(filePath);
            return created;
        }

        try
        {
            string json = File.ReadAllText(filePath);
            PlayerDataStore? loaded = JsonSerializer.Deserialize<PlayerDataStore>(json, _jsonOptions);
            if (loaded == null)
            {
                loaded = new PlayerDataStore();
                loaded.Save(filePath);
            }

            return loaded;
        }
        catch
        {
            PlayerDataStore fallback = new();
            fallback.Save(filePath);
            return fallback;
        }
    }

    public void Save(string filePath)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    public PlayerDataEntry GetOrCreate(Guid playerId)
    {
        if (!Players.TryGetValue(playerId, out PlayerDataEntry? entry))
        {
            entry = new PlayerDataEntry();
            Players[playerId] = entry;
        }

        return entry;
    }

    public PlayerVeinMinerMode GetPlayerMode(Guid playerId)
    {
        return GetOrCreate(playerId).Mode;
    }

    public void SetPlayerMode(Guid playerId, PlayerVeinMinerMode mode)
    {
        GetOrCreate(playerId).Mode = mode;
    }

    public ServerVeinMinerMode GetEffectiveMode(Guid playerId, ServerVeinMinerMode serverDefault)
    {
        PlayerVeinMinerMode playerMode = GetPlayerMode(playerId);

        return playerMode switch
        {
            PlayerVeinMinerMode.Always => ServerVeinMinerMode.Always,
            PlayerVeinMinerMode.Crouching => ServerVeinMinerMode.Crouching,
            PlayerVeinMinerMode.Never => ServerVeinMinerMode.Never,
            _ => serverDefault
        };
    }

    public ServerVeinMinerMode GetEffectiveMode(Player player, ServerVeinMinerMode serverDefault)
    {
        return GetEffectiveMode(player.getUniqueId(), serverDefault);
    }
}

public sealed class PlayerDataEntry
{
    public PlayerVeinMinerMode Mode { get; set; } = PlayerVeinMinerMode.Default;

    public Dictionary<string, string> Options { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
