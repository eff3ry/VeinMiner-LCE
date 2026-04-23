using Minecraft.Server.FourKit;
using Minecraft.Server.FourKit.Block;
using Minecraft.Server.FourKit.Enchantments;
using Minecraft.Server.FourKit.Entity;
using Minecraft.Server.FourKit.Event;
using Minecraft.Server.FourKit.Event.Block;
using Minecraft.Server.FourKit.Inventory;
using Minecraft.Server.FourKit.Inventory.Meta;
using System;
using System.Collections.Generic;
using System.Text;

namespace VeinMiner_LCE;
public class BreakListener : Listener
{
    [EventHandler]
    public void onBlockBreak(BlockBreakEvent e)
    {
        Player player = e.getPlayer();
        Block block = e.getBlock();

        if (Veinminer.CurrentConfig.DefaultMode == ServerVeinMinerMode.Crouching && !player.isSneaking())
        ItemStack? itemInHand = player.getItemInHand();
        if (itemInHand != null)
        {
            return;
        }

        if (!player.isSneaking())
        {
            return;
        }

        Veinmine(block, 64, player);
    }


    private int Veinmine(Block startBlock, int maxBlocks, Player player)
    {
        World world = startBlock.getWorld();

        //Fetch BlockType Alias' from config file
        int[] BlockTypes = { startBlock.getTypeId() };

        HashSet<Location> visited = new HashSet<Location>();
        Queue<Location> toCheck = new Queue<Location>();

        Location startLocation = startBlock.getLocation();
        visited.Add(startLocation);

        // Add adjacent blocks to the initial block (don't re-break the initial block)
        CheckAdjacentBlocks(startLocation, visited, toCheck);

        int blocksBroken = 1;

        while (toCheck.Count > 0 && blocksBroken < maxBlocks)
        {
            Location currentLocation = toCheck.Dequeue();

            Block currentBlock = world.getBlockAt(currentLocation);

            if (Array.Exists(BlockTypes, type => type == currentBlock.getTypeId()))
            {
                if (!CanContinueVeinMining(player))
                {
                    player.sendMessage("Your tool is too damaged to continue vein mining!");
                    break;
                }

                currentBlock.breakNaturally();
                DamageCurrentTool(player, false);
                blocksBroken++;            

                // Check adjacent blocks of the newly broken block
                CheckAdjacentBlocks(currentLocation, visited, toCheck);
            }
        }
        return blocksBroken;
    }

    /// <summary>
    /// Checks all adjacent blocks and adds unvisited log blocks to the queue.
    /// </summary>
    /// <param name="coord">The coordinate to check around.</param>
    /// <param name="visited">Set of already visited coordinates.</param>
    /// <param name="toCheck">Queue of coordinates to check.</param>
    private void CheckAdjacentBlocks(Location coord, HashSet<Location> visited, Queue<Location> toCheck)
    {
        Location[] adjacentOffsets = GetAdjacentOffsets();

        foreach (Location offset in adjacentOffsets)
        {
            Location neighbor = new Location(
                coord.getWorld(),
                coord.getX() + offset.getX(),
                coord.getY() + offset.getY(),
                coord.getZ() + offset.getZ(),
                0, 0
            );

            if (!visited.Contains(neighbor))
            {
                visited.Add(neighbor);
                toCheck.Enqueue(neighbor);
            }
        }
    }

    /// <summary>
    /// Gets the offsets for all 26 adjacent blocks (including diagonals).
    /// </summary>
    /// <returns>Array of coordinate offsets.</returns>
    private Location[] GetAdjacentOffsets()
    {
        return
        [
                // Same Y level (8 blocks)
                new Location(-1, 0, 0), new Location(1, 0, 0),
                new Location(0, 0, -1), new Location(0, 0, 1),
                new Location(-1, 0, -1), new Location(-1, 0, 1),
                new Location(1, 0, -1), new Location(1, 0, 1),

                // Y+1 level (9 blocks)
                new Location(-1, 1, 0), new Location(1, 1, 0),
                new Location(0, 1, -1), new Location(0, 1, 1),
                new Location(-1, 1, -1), new Location(-1, 1, 1),
                new Location(1, 1, -1), new Location(1, 1, 1),
                new Location(0, 1, 0),

                // Y-1 level (9 blocks)
                new Location(-1, -1, 0), new Location(1, -1, 0),
                new Location(0, -1, -1), new Location(0, -1, 1),
                new Location(-1, -1, -1), new Location(-1, -1, 1),
                new Location(1, -1, -1), new Location(1, -1, 1),
                new Location(0, -1, 0)
        ];
    }

    private void DamageCurrentTool(Player player, bool ignoreEnchantments, int damageAttempts = 1)
    {
        ItemStack? toolItem = player.getItemInHand();
        if (toolItem == null)
        {
            return;
        }

        short durability = toolItem.getDurability();
        short newDurability = durability;

        if (ignoreEnchantments)
        {
            newDurability = (short)(durability + damageAttempts);
        } else
        {
            for (int i = 0; i < damageAttempts; i++)
            {
                if (toolItem.getItemMeta().getEnchants().TryGetValue(EnchantmentType.DURABILITY, out int unbreakingLevel) && unbreakingLevel > 0)
                {
                    // Simulate unbreaking enchantment
                    double chanceToNotDamage = 1.0 / (unbreakingLevel + 1);
                    if (new Random().NextDouble() >= chanceToNotDamage)
                    {
                        newDurability++;
                    }
                }
                else
                {
                    newDurability++;
                }
            }
        }

        if (Veinminer.CurrentConfig.SaveTools && ToolDurability.TryGetMaxDurability(toolItem.getType(), out short maxDurability))
        {
            short safeMax = (short)(maxDurability - 2); // safe needs to be -2 not -1 because of the initial block break
            if (newDurability >= safeMax)
            {
                if (durability < safeMax)
                {
                    player.playSound(player.getLocation(), Sound.ITEM_BREAK, 1, 1);
                }

                newDurability = safeMax;
            }
        }

        //for some reason setting durability clears itemmeta, so we need to store the enchantments and reapply them after setting durability
        //ItemMeta meta = toolItem.getItemMeta().clone();


        //player.sendMessage("" + toolItem.hasItemMeta());
        toolItem.setDurability(newDurability);



        //player.sendMessage($"Tool durability: {toolItem.getDurability()}, ToolMeta: {toolItem.getItemMeta().getEnchants()}");
        //toolItem.setItemMeta(meta);
    }

    private bool CanContinueVeinMining(Player player)
    {
        ItemStack? toolItem = player.getItemInHand();
        if (toolItem == null)
        {
            return false;
        }

        if (!Veinminer.CurrentConfig.SaveTools)
        {
            return true;
        }

        if (!ToolDurability.TryGetMaxDurability(toolItem.getType(), out short maxDurability))
        {
            return true;
        }

        short safeMax = (short)(maxDurability - 2);
        if (toolItem.getDurability() >= safeMax)
        {
            return false;
        }

        return true;
    }
}
