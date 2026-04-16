using Minecraft.Server.FourKit;
using System.Collections.Generic;

namespace VeinMiner_LCE;

internal static class ToolDurability
{
    private static readonly Dictionary<Material, short> _maxDurabilityById = new()
    {
        [Material.WOOD_SPADE] = 60,
        [Material.STONE_SPADE] = 132,
        [Material.IRON_SPADE] = 251,
        [Material.DIAMOND_SPADE] = 1562,
        [Material.GOLD_SPADE] = 33,

        [Material.WOOD_PICKAXE] = 60,
        [Material.STONE_PICKAXE] = 132,
        [Material.IRON_PICKAXE] = 251,
        [Material.DIAMOND_PICKAXE] = 1562,
        [Material.GOLD_PICKAXE] = 33,

        [Material.WOOD_AXE] = 60,
        [Material.STONE_AXE] = 132,
        [Material.IRON_AXE] = 251,
        [Material.DIAMOND_AXE] = 1562,
        [Material.GOLD_AXE] = 33,

        [Material.WOOD_SWORD] = 60,
        [Material.STONE_SWORD] = 132,
        [Material.IRON_SWORD] = 251,
        [Material.DIAMOND_SWORD] = 1562,
        [Material.GOLD_SWORD] = 33,

        [Material.WOOD_HOE] = 60,
        [Material.STONE_HOE] = 132,
        [Material.IRON_HOE] = 251,
        [Material.DIAMOND_HOE] = 1562,
        [Material.GOLD_HOE] = 33,

        [Material.BOW] = 385,
        [Material.FISHING_ROD] = 65,
        [Material.SHEARS] = 239,
        [Material.FLINT_AND_STEEL] = 65
    };

    public static bool TryGetMaxDurability(Material itemType, out short maxDurability)
    {
        return _maxDurabilityById.TryGetValue(itemType, out maxDurability);
    }

    public static short GetMaxDurabilityOrDefault(Material itemType, short defaultValue = -1)
    {
        return _maxDurabilityById.TryGetValue(itemType, out short maxDurability)
            ? maxDurability
            : defaultValue;
    }

    public static short ClampToSafeDurability(Material itemType, short nextDurability)
    {
        if (!_maxDurabilityById.TryGetValue(itemType, out short maxDurability) || maxDurability <= 0)
        {
            return nextDurability;
        }

        short safeMax = (short)(maxDurability - 1);
        return nextDurability > safeMax ? safeMax : nextDurability;
    }
}
