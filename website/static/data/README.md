# Reward Struct
> This struct is shared across multiple exports.

[C# Example](../../../Export/SupabaseExporter/SupabaseExporter/Structures/Exports/Reward.cs)  
[TS Example](../../src/lib/structs/reward.ts)

```
Reward
│   Id: uint                    # Item sheet key
│   Amount: long                # Number of records this reward has been seen
│   Total: long                 # Number of all quantities combined
│   Pct: float                  # Chance to receive this reward, precision is truncated to 6 decimals
│   Min: long                   # Lowest quantity received
│   Max: long                   # Highest quantity received
```

# BnpcPairs.json
> A collection of all Battle-NPCs spotted by users.

[C# Example](../../../Export/SupabaseExporter/SupabaseExporter/Structures/Exports/BnpcPairing.cs)  
[TS Example](../../src/lib/structs/bnpc.ts)

```
Bnpc
└───BnpcPairings: Dict<ulong, BnpcPairing>              # key = (base id << 32) + name id
│   └───BnpcPairing
│       │   Records: uint                               # Number of uploaded records this bnpc has
│       │   Base: uint                                  # BNpcBase sheet key
│       │   Name: uint                                  # BNpcName sheet key
│       │   Kind: ushort                                #    ObjectKind
│       │   Battalion: uint                             # Exact meaning currently unknown
│       │   Locations: Dict<key, Location>  
│       └───Location
│       │       │   Territory: uint                     # TerritoryType sheet key
│       │       │   Map: uint                           # Map sheet key
│       │       │   Level: uint                         # level this bnpc has been spotted with
│       │       │   Positions: List<Vector3>            # precise world positions this bnpc has been spotted at
│       │       │   PositionCounts: Dict<int, uint>     # Number of records for each position, Positions index -> count
```

#### ObjectKind
> Most entries have type 2 (BattleNpc) but there may be a few exceptions.  
> See ClientStructs for a full list: [ObjectKind](https://github.com/aers/FFXIVClientStructs/blob/c8d20f04a13734dcd2f7a5019a9c4ae3f259c968/FFXIVClientStructs/FFXIV/Client/Game/Object/GameObject.cs#L330)

#### World position
> The game uses XYZ world position for all objects, but user facing map coordinates are XY  
> For conversion from world → map see [Conversion](../../src/lib/coordHelper.ts)


# BnpcPairsSimple.json
> Simplified collection with just Battle-NPCs base id → name id.  
> Base ids are unique but can have multiple name ids attached.  
> There is no guarantee that name ids aren't reused.

[C# Example](../../../Export/SupabaseExporter/SupabaseExporter/Structures/Exports/BnpcPairing.cs)

```
List<BnpcSimple>
└───BnpcPairings
│       │   Base: uint                    # BNpcBase sheet key
│       │   Names: HashSet<uint>          # List of BNpcName sheet keys
```

# ChestDrops.json
> A collection of drops from chests, this includes Dungeons, Trials, Open World, Treasure Hunt.  
> Records are only produced if the user was in a group and the "Loot Roll" window appeared.

[C# Example](../../../Export/SupabaseExporter/SupabaseExporter/Structures/Exports/ChestDrop.cs)  
[TS Example](../../src/lib/structs/chestDrop.ts)

```
ChestDrop
│   Id: uint                                        # Category id
│   Name: string                                    # Category name
│   Expansions: List<Expansion>   
└───Expansion
│   │   Id: uint                                    # Expansion id
│   │   Name: string                                # Expansion name
│   │   Headers: List<Header>
│   └───Header
│   │   │   Id: uint                                # Header id
│   │   │   Name: string                            # Header name
│   │   │   Duties: List<Duty>
│   │   │   └───Duty
│   │   │   │   Records: uint                       # TerritoryType sheet key
│   │   │   │   Id: uint                            # IF < 100_000 ContentFinderCondition sheet key ELSE TerritoryType sheet key - 100_000
│   │   │   │   Name: string                        # Map sheet key
│   │   │   │   SortKey: uint                       # Map sheet key
│   │   │   │   PatchRecords: Dict<string, uint>    # Number of records per patch
│   │   │   │   Chests: Dict<string, List<Chest>>   # key = patch string
│   │   │   │   └───Chest
│   │   │   │   │   Records: uint                   # Treasure sheet key
│   │   │   │   │   Id: uint                        # Treasure sheet key
│   │   │   │   │   Name: string                    # Treasure sheet key
│   │   │   │   │   TerritoryId: uint               # TerritoryType sheet key
│   │   │   │   │   MapId: uint                     # Map sheet key
│   │   │   │   │   PlaceNameSub: string            # TerritoryType sheet key
│   │   │   │   │   Position: Vector3               # Precise world position for this chest
│   │   │   │   │   Rewards: List<Reward>           # All rewards this chest can contain
```

#### World position
> The game uses XYZ world position for all objects, but user facing map coordinates are XY  
> For conversion from world → map see [Conversion](../../src/lib/coordHelper.ts)

# Coffers
> A shared interface for different type of coffer like collections.

[C# Example](../../../Export/SupabaseExporter/SupabaseExporter/Structures/Exports/Coffer.cs)  
[TS Example](../../src/lib/structs/coffer.ts)

```
Coffer
│   Name: string                                # Category name for this type, e.g Anemos
│   TerritoryId: uint                           # A unique id to separate categories, most times this is TerritoryType sheet key
│   Variants: List<CofferVariant>   
└───CofferVariant
│   │   Id: uint                                # A unique id to separate variations, most times this is Item sheet key
│   │   Name: string                            # Variant name for this coffer, e.g Anemos Lockbox
│   │   Patches: Dict<string, CofferContent>    # key = Patch string
│   └───CofferContent
│   │   │   Total: long                         # Number of records received for this coffer
│   │   │   Items: List<Reward>                 # All known drops from this coffer
```

#### RandomCoffers.json
> Contains all coffers not bound to instances itself.  
> Venture Coffers, Materiel Container 3.0, Materiel Container 4.0, and Sanctuary Materiel Container

#### DeepDungeonSacks.json
> Contains all treasures from deep dungeon instances.  
> PotD, HoH, EO, and PT

#### FieldOpLockboxes.json
> Contains all lockbox type of coffers.  
> Eureka, Bozja, Oizys, and Auxesia

#### TripleTriadPacks.json
> Contains all Triple Triad Card Packs.

#### FieldOpContainers.json
> Contains all Field Operation special containers.
> Logograms and Fragments

## Ventures.json
> A collection of venture related stats.
> Includes Quick Ventures, and all Class related Ventures



