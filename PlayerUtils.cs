using Minecraft.Server.FourKit;
using Minecraft.Server.FourKit.Enchantments;
using Minecraft.Server.FourKit.Entity;
using Minecraft.Server.FourKit.Inventory;
using System.Collections.Generic;

namespace VeinMiner_LCE;

internal static class PlayerUtils
{
    #region VeinminingMode

    #endregion

    #region Exhaustion
    private static readonly Dictionary<Guid, float> _playerExhaustion = new();

    public static float GetExhaustion(Guid playerId)
    {
        return _playerExhaustion.TryGetValue(playerId, out float exhaustion) ? exhaustion : 0f;
    }

    public static void SetExhaustion(Guid playerId, float exhaustion)
    {
        _playerExhaustion[playerId] = exhaustion;
    }

    public static float AddExhaustion(Guid playerId, float amount)
    {
        float newValue = GetExhaustion(playerId) + amount;
        _playerExhaustion[playerId] = newValue;
        return newValue;
    }

    public static void ClearExhaustion(Guid playerId)
    {
        _playerExhaustion.Remove(playerId);
    }
    #endregion

    #region Veinmining
    public static void ApplyExhaustion(Player player, int blocksBroken = 1)
    {
        const float exhaustionPerBlock = 0.005f;
        const float sprintingMultiplier = 1.5f;
        const float exhaustionScale = 10.0f;

        //Veinmining exhaustion is 0.005 per block broken, multiplied by 1.5 if the player is sprinting
        float exhaustionAmount = exhaustionPerBlock * blocksBroken * exhaustionScale;
        if (player.isSprinting())
        {
            exhaustionAmount *= sprintingMultiplier;
        }
        float exhaustion = AddExhaustion(player.getUniqueId(), exhaustionAmount);

        while (exhaustion >= 4.0f)
        {
            exhaustion -= 4.0f;

            int foodLevel = player.getFoodLevel();
            float saturationLevel = player.getSaturation();

            if (saturationLevel > 0)
            {
                player.setSaturation(MathF.Max(0f, saturationLevel - 1.0f));
                //player.sendMessage("Saturation decreased by 1.0 due to exhaustion.");
            } else
            {
                player.setFoodLevel(Math.Max(0, foodLevel - 1));
                //player.sendMessage("Food level decreased by 1 due to exhaustion.");
            }
        }
        SetExhaustion(player.getUniqueId(), exhaustion);
    }

    public static void ApplyToolDamage(Player player, int blocksBroken = 1, bool ignoreEnchants = false)
    {
        ItemStack? toolItem = player.getItemInHand();

        //No tool to apply damage, break here
        if (toolItem == null)
        {
            return;
        }

        short durability = toolItem.getDurability();
        short newDurability = durability;

        Dictionary<EnchantmentType, int> enchants = toolItem.getItemMeta().getEnchants();

        for (int i = 0; i < blocksBroken; i++)
        {
            if (!ignoreEnchants && enchants.TryGetValue(EnchantmentType.DURABILITY, out int unbreakingLevel) && unbreakingLevel > 0)
            {
                // Simulate unbreaking enchantment, roll for durability chance
                if (RollUnbreakingChance(unbreakingLevel)) newDurability++;
            }
            else
            {
                newDurability++;
            }
        }

        //Clamp Durability taken if savetools is enabled
        if (Veinminer.CurrentConfig.SaveTools && ToolDurability.TryGetMaxDurability(toolItem.getType(), out short maxDurability))
        {
            short safeMax = (short)(maxDurability - 2); // safe needs to be -2 not -1 because of the initial block break?
            if (newDurability >= safeMax)
            {
                player.playSound(player.getLocation(), Sound.ITEM_BREAK, 1, 1);
                newDurability = safeMax;
            }
        }
        toolItem.setDurability(newDurability);

    }
    private static bool RollUnbreakingChance(int unbreakingLevel)
    {
        double chanceToNotDamage = 1.0 / (unbreakingLevel + 1);
        if (new Random().NextDouble() >= chanceToNotDamage)
        {
            return true;
        }
        return false;
    }

    public static bool CanContinueVeinMining(Player player)
    {
        ItemStack? toolItem = player.getItemInHand();

        // No tool or tool broke, break here and stop veinmining
        if (toolItem == null)
        {
            return false;
        }

        // If hunger is enabled and player is out of food, break here and stop veinmining
        if (Veinminer.CurrentConfig.UseHunger && player.getFoodLevel() == 0)
        {
            return false;
        }

        // Save tools is turned on, do other checks
        if (Veinminer.CurrentConfig.SaveTools)
        {
            // If durability is unknown for this material, do not block vein mining.
            if (!ToolDurability.TryGetMaxDurability(toolItem.getType(), out short maxDurability))
            {
                return true;
            }

            // Keep 1 real durability point left (effective safe cap for this flow is max - 2).
            short safeMax = (short)(maxDurability - 2);
            if (toolItem.getDurability() >= safeMax)
            {
                return false;
            }
        }

        return true;
    }

    #endregion

}
