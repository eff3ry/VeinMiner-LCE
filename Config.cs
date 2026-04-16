using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VeinMiner_LCE;

public sealed class Config
{
    public const string DefaultFileName = "veinminer.yml";

    public const int CurrentConfigVersion = 1;
    public int ConfigVersion { get; init; } = CurrentConfigVersion;

    private const string DefaultFileComments = "# VeinMiner-LCE configuration\n"
        + "# defaultMode: Always | Crouching | Never\n"
        + "# saveTools: Prevent tools from breaking (leave 1 durability)\n"
        + "# useDurability: Apply tool durability loss while vein mining\n"
        + "# useHunger: Consume player hunger/saturation while vein mining\n"
        + "# block metadata example:\n"
        + "#   - id: LOG\n"
        + "#     dataMask: 3\n"
        + "#     dataAnyOf: [0, 1, 2, 3]\n\n";

    private static readonly IDeserializer _deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new BlockEntryYamlTypeConverter())
        .IgnoreUnmatchedProperties()
        .Build();

    private static readonly ISerializer _serializer = new SerializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithTypeConverter(new BlockEntryYamlTypeConverter())
        .Build();

    public VeinMinerMode DefaultMode { get; init; } = VeinMinerMode.Crouching;

    public bool SaveTools { get; init; } = true;

    public bool UseDurability { get; init; } = true;

    public bool UseHunger { get; init; } = false;

    public int MaxBlocks { get; init; } = 64;

    public Dictionary<string, HashSet<string>> ToolTypes { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, VeinRule> Rules { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public static Config CreateDefault()
    {
        return new Config
        {
            DefaultMode = VeinMinerMode.Crouching,
            SaveTools = true,
            UseDurability = true,
            UseHunger = true,
            MaxBlocks = 64,
            ToolTypes = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["shovel"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "WOOD_SPADE", "STONE_SPADE", "IRON_SPADE", "DIAMOND_SPADE", "GOLD_SPADE" },
                ["pickaxe"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "WOOD_PICKAXE", "STONE_PICKAXE", "IRON_PICKAXE", "DIAMOND_PICKAXE", "GOLDEN_PICKAXE" },
                ["axe"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "WOOD_AXE", "STONE_AXE", "IRON_AXE", "DIAMOND_AXE", "GOLDEN_AXE" }
            },
            Rules = new Dictionary<string, VeinRule>(StringComparer.OrdinalIgnoreCase)
            {
                ["pickaxe"] = new VeinRule
                {
                    Blocks =
                    [
                        BlockEntry.From("COAL_ORE"),
                        BlockEntry.From("IRON_ORE"),
                        BlockEntry.From("GOLD_ORE"),
                        BlockEntry.From("DIAMOND_ORE"),
                        BlockEntry.From("LAPIS_ORE"),
                        BlockEntry.FromMany("REDSTONE_ORE", "GLOWING_REDSTONE_ORE"),
                        BlockEntry.From("EMERALD_ORE"),
                        BlockEntry.From("QUARTZ_ORE"),
                    ]

                },
                ["axe"] = new VeinRule
                {
                    Blocks =
                    [
                        BlockEntry.FromMasked("LOG", 3, 0),
                        BlockEntry.FromMasked("LOG", 3, 1),
                        BlockEntry.FromMasked("LOG", 3, 2),
                        BlockEntry.FromMasked("LOG", 3, 3),
                    ]
                }
            }
        };
    }

    public static Config LoadOrCreate(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Config created = CreateDefault();
            created.Save(filePath, includeComments: true);
            return created;
        }

        string yaml = File.ReadAllText(filePath);
        try
        {
            Config? config = _deserializer.Deserialize<Config>(yaml);
            if (config == null)
            {
                config = CreateDefault();
                config.Save(filePath, includeComments: true);
            }

            return config;
        }
        catch (YamlException)
        {
            Config fallback = CreateDefault();
            fallback.Save(filePath, includeComments: true);
            return fallback;
        }
    }

    public HashSet<string> ResolveRuleBlocks(string ruleName)
    {
        HashSet<string> all = new(StringComparer.OrdinalIgnoreCase);
        foreach (BlockEntry blockEntry in ResolveRuleEntries(ruleName))
        {
            all.UnionWith(blockEntry.Values);
        }

        return all;
    }

    public List<BlockEntry> ResolveRuleEntries(string ruleName)
    {
        if (!Rules.TryGetValue(ruleName, out VeinRule? rule))
        {
            return [];
        }

        return [.. rule.Blocks];
    }

    public HashSet<string> ResolveToolGroups(string toolId)
    {
        HashSet<string> groups = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, HashSet<string>> toolType in ToolTypes)
        {
            if (toolType.Value.Contains(toolId))
            {
                groups.Add(toolType.Key);
                continue;
            }

            foreach (string configuredTool in toolType.Value)
            {
                if (string.Equals(configuredTool, toolId, StringComparison.OrdinalIgnoreCase))
                {
                    groups.Add(toolType.Key);
                    break;
                }
            }
        }

        return groups;
    }

    public HashSet<string> ResolveBlocksForTool(string toolId)
    {
        HashSet<string> all = new(StringComparer.OrdinalIgnoreCase);

        foreach (BlockEntry blockEntry in ResolveEntriesForTool(toolId))
        {
            all.UnionWith(blockEntry.Values);
        }

        return all;
    }

    public List<BlockEntry> ResolveEntriesForTool(string toolId)
    {
        List<BlockEntry> all = [];

        foreach (string group in ResolveToolGroups(toolId))
        {
            all.AddRange(ResolveRuleEntries(group));
        }

        return all;
    }

    public void Save(string filePath, bool includeComments = false)
    {
        string? directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string yaml = _serializer.Serialize(this);
        if (includeComments)
        {
            yaml = DefaultFileComments + yaml;
        }

        File.WriteAllText(filePath, yaml);
    }
}

public enum VeinMinerMode
{
    Always,
    Crouching,
    Never
}

public sealed class VeinRule
{
    public List<BlockEntry> Blocks { get; init; } = new();
}

public sealed class BlockEntry
{
    public HashSet<string> Values { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public int? DataMask { get; init; }

    public HashSet<int> DataAnyOf { get; init; } = new();

    public static BlockEntry From(string value) => new() { Values = [value] };

    public static BlockEntry FromMany(params string[] values) => new() { Values = [.. values] };

    public static BlockEntry FromMasked(string value, int dataMask, params int[] dataAnyOf) => new()
    {
        Values = [value],
        DataMask = dataMask,
        DataAnyOf = [.. dataAnyOf]
    };

    public bool Matches(string blockId, int blockData)
    {
        bool idMatch = Values.Any(v => string.Equals(v, blockId, StringComparison.OrdinalIgnoreCase));
        if (!idMatch)
        {
            return false;
        }

        if (DataAnyOf.Count == 0)
        {
            return true;
        }

        int mask = DataMask ?? 0xFF;
        int masked = blockData & mask;
        return DataAnyOf.Contains(masked);
    }
}

public sealed class BlockEntryYamlTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(BlockEntry);
    }

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        if (parser.TryConsume<Scalar>(out Scalar? scalar))
        {
            return BlockEntry.From(scalar.Value);
        }

        if (parser.TryConsume<SequenceStart>(out _))
        {
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            while (!parser.TryConsume<SequenceEnd>(out _))
            {
                Scalar item = parser.Consume<Scalar>();
                values.Add(item.Value);
            }

            return new BlockEntry { Values = values };
        }

        if (parser.TryConsume<MappingStart>(out _))
        {
            HashSet<string> values = new(StringComparer.OrdinalIgnoreCase);
            int? dataMask = null;
            HashSet<int> dataAnyOf = new();

            while (!parser.TryConsume<MappingEnd>(out _))
            {
                Scalar key = parser.Consume<Scalar>();

                if (key.Value.Equals("id", StringComparison.OrdinalIgnoreCase))
                {
                    Scalar id = parser.Consume<Scalar>();
                    values.Add(id.Value);
                    continue;
                }

                if (key.Value.Equals("ids", StringComparison.OrdinalIgnoreCase))
                {
                    parser.Consume<SequenceStart>();
                    while (!parser.TryConsume<SequenceEnd>(out _))
                    {
                        Scalar id = parser.Consume<Scalar>();
                        values.Add(id.Value);
                    }

                    continue;
                }

                if (key.Value.Equals("dataMask", StringComparison.OrdinalIgnoreCase))
                {
                    Scalar mask = parser.Consume<Scalar>();
                    dataMask = int.Parse(mask.Value);
                    continue;
                }

                if (key.Value.Equals("dataAnyOf", StringComparison.OrdinalIgnoreCase))
                {
                    if (parser.TryConsume<SequenceStart>(out _))
                    {
                        while (!parser.TryConsume<SequenceEnd>(out _))
                        {
                            Scalar value = parser.Consume<Scalar>();
                            dataAnyOf.Add(int.Parse(value.Value));
                        }
                    }
                    else
                    {
                        Scalar value = parser.Consume<Scalar>();
                        dataAnyOf.Add(int.Parse(value.Value));
                    }

                    continue;
                }

                rootDeserializer(typeof(object));
            }

            return new BlockEntry
            {
                Values = values,
                DataMask = dataMask,
                DataAnyOf = dataAnyOf
            };
        }

        throw new YamlException("Unsupported block entry format.");
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        BlockEntry blockEntry = (BlockEntry)value!;

        if (blockEntry.DataAnyOf.Count > 0 || blockEntry.DataMask.HasValue)
        {
            emitter.Emit(new MappingStart());
            if (blockEntry.Values.Count <= 1)
            {
                emitter.Emit(new Scalar("id"));
                emitter.Emit(new Scalar(blockEntry.Values.FirstOrDefault() ?? string.Empty));
            }
            else
            {
                emitter.Emit(new Scalar("ids"));
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
                foreach (string id in blockEntry.Values)
                {
                    emitter.Emit(new Scalar(id));
                }
                emitter.Emit(new SequenceEnd());
            }

            if (blockEntry.DataMask.HasValue)
            {
                emitter.Emit(new Scalar("dataMask"));
                emitter.Emit(new Scalar(blockEntry.DataMask.Value.ToString()));
            }

            if (blockEntry.DataAnyOf.Count > 0)
            {
                emitter.Emit(new Scalar("dataAnyOf"));
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
                foreach (int dataValue in blockEntry.DataAnyOf)
                {
                    emitter.Emit(new Scalar(dataValue.ToString()));
                }
                emitter.Emit(new SequenceEnd());
            }

            emitter.Emit(new MappingEnd());
            return;
        }

        if (blockEntry.Values.Count == 1)
        {
            foreach (string item in blockEntry.Values)
            {
                emitter.Emit(new Scalar(item));
            }

            return;
        }

        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Flow));
        foreach (string item in blockEntry.Values)
        {
            emitter.Emit(new Scalar(item));
        }
        emitter.Emit(new SequenceEnd());
    }
}
