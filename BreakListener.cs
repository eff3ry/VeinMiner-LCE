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

        ServerVeinMinerMode mode = Veinminer.PlayerData.GetEffectiveMode(player, Veinminer.CurrentConfig.DefaultMode);
        if (mode == ServerVeinMinerMode.Never)
        {
            return;
        }

        if (mode == ServerVeinMinerMode.Crouching && !player.isSneaking())
        {
            return;
        }

        //Apply a check here to avoid a wasted attempt
        if (!PlayerUtils.CanContinueVeinMining(player))
        {
            return;
        }

        Veinmine(block, Veinminer.CurrentConfig.MaxBlocks, player);
    }


    private int Veinmine(Block startBlock, int maxBlocks, Player player)
    {
        World world = startBlock.getWorld();

        //Check Tool
        ItemStack? tool = player.getItemInHand();
        if (tool == null)
        {
            return 0;
        }
        int startData = startBlock.getData();
        List<BlockEntry> matchingEntries = Veinminer.CurrentConfig.ResolveMatchingEntriesForTool(tool.getType(), startBlock.getType(), startData);
        if (matchingEntries.Count == 0)
        {
            return 0;
        }


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

            if (MatchesAnyEntry(currentBlock, matchingEntries))
            {
                if (!PlayerUtils.CanContinueVeinMining(player))
                {
                    //player.sendMessage("Your tool is too damaged to continue vein mining!");
                    break;
                }

                currentBlock.breakNaturally();

                if (Veinminer.CurrentConfig.UseDurability)
                {
                    PlayerUtils.ApplyToolDamage(player);
                }
                if (Veinminer.CurrentConfig.UseHunger)
                {
                    PlayerUtils.ApplyExhaustion(player);
                }

                blocksBroken++;            

                // Check adjacent blocks of the newly broken block
                CheckAdjacentBlocks(currentLocation, visited, toCheck);
            }
        }
        return blocksBroken;
    }

    private static bool MatchesAnyEntry(Block block, List<BlockEntry> entries)
    {
        string blockId = block.getType().ToString();
        int blockData = block.getData();

        foreach (BlockEntry entry in entries)
        {
            if (entry.Matches(blockId, blockData))
            {
                return true;
            }
        }

        return false;
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
}
