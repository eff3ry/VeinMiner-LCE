# Veinminer LCE
Server side plugin for Legacy Console Edition based on [Fourkit API](https://github.com/sylvessa/MinecraftConsoles)

## Current State
Be noted this project currently does not respect the tool groups or block groups just yet, all other config parameters and commands are currently implemented

## Setup
- Create a folder called `VeinMiner-LCE` inside the server `plugins` folder.
- Place the built `VeinMiner-LCE.dll` file and the dependency `YamlDotNet.dll` inside the newly created `VeinMiner-LCE` folder.
- Either create a new Config file using the following example below or let the plugin generate the default upon the first run.

## Config
```YML
configVersion: 1 # Auto-Generated

defaultMode: Crouching # Can be set to Crouching, Always or Never.
useDurability: true # When true, tool durability will be decremented for every block veinmined.
saveTools: true # When true, veinmining will be stopped when tools reach 1 remaining durability.
useHunger: true # When true, for every veinmined block saturation then hunger will be decremented, then when 0 hunger remains veinmining will be blocked.
ignoreEnchants: false # When true, Unbreaking enchantment is ignored, taking 1 durability per block, else when false the standard chance per .level will apply
maxBlocks: 64 # Max blocks to veinmine at once.

toolTypes: # Underneath here we can define tool groups
  pickaxe: # For example this is a new tool group called 'pickaxe' containing the below item definitions
  - WOOD_PICKAXE
  - STONE_PICKAXE
  - IRON_PICKAXE
  - DIAMOND_PICKAXE
  - GOLDEN_PICKAXE
  axe:
  - WOOD_AXE
  - STONE_AXE
  - IRON_AXE
  - DIAMOND_AXE
  - GOLDEN_AXE

rules: # Underneath here we define Rules for groups and which blocks they can effect
  pickaxe: # referencing the pickaxe tool group defined above
    blocks: 
    - COAL_ORE # Blocks can be defined on their own
    - IRON_ORE
    - GOLD_ORE
    - DIAMOND_ORE
    - LAPIS_ORE
    - [REDSTONE_ORE, GLOWING_REDSTONE_ORE] # Blocks can also be defined like an array, these blocks are treated as if they were the same, ie Veinmining glowing_redstone_ore will also break any connected redstone_ore
    - EMERALD_ORE
    - QUARTZ_ORE

  axe:
    blocks: # For more complicated BlockStates we can filter by blockData
    - id: LOG 
      dataMask: 3 # Mask off the data with 0x3, in this case we are masking to the log species
      dataAnyOf: [0] # Match for the remaining value of 0, in this case Oak
    - id: LOG
      dataMask: 3
      dataAnyOf: [1]
    - id: LOG
      dataMask: 3
      dataAnyOf: [2]
    - id: LOG
      dataMask: 3
      dataAnyOf: [3]
```

## Commands
There currently exists only 3 commands:
- /veinminer help
- /veinminer setmode <default | always | crouching | never>, This is per player and is stored persistently against the players UUID.
- /Veinminer reload, Only Accessible via Console, reloads the config and player data from file.