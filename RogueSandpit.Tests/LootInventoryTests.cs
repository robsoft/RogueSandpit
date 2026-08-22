using RogueSandpit.Models;
using Xunit;

namespace RogueSandpit.Tests;

public class LootInventoryTests
{
    [Fact]
    public void InventoryRejectsItemsBeyondCapacity()
    {
        var inventory = new Inventory(2);

        Assert.True(inventory.TryAdd(ItemFactory.Create(ItemType.Key)));
        Assert.True(inventory.TryAdd(ItemFactory.Create(ItemType.Weapon)));
        Assert.False(inventory.TryAdd(ItemFactory.Create(ItemType.HealingPotion)));
        Assert.True(inventory.IsFull);
        Assert.Equal(2, inventory.Items.Count);
    }

    [Fact]
    public void MovingOntoLootPicksItUp()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        map.GroundItems.Add(new GroundItem(potion, x + 1, y));

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Contains(potion, player.Inventory.Items);
        Assert.Null(map.GetGroundItemAt(x + 1, y));
        Assert.Contains("PICKED UP HEALING POTION", gameState.EventLog.Entries);
    }

    [Fact]
    public void FirstPickedUpWeaponIsAutomaticallyEquipped()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        map.GroundItems.Add(new GroundItem(weapon, x + 1, y));

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Same(weapon, player.EquippedWeapon);
        Assert.Equal(player.BaseDamage + weapon.Power, player.Damage);
        Assert.Contains("AUTO-EQUIPPED IRON SWORD", gameState.EventLog.Entries);
    }

    [Fact]
    public void PickingUpAnotherWeaponDoesNotReplaceEquippedWeapon()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item equippedWeapon = ItemFactory.Create(ItemType.Weapon);
        Item newWeapon = new Item("STEEL AXE", ItemType.Weapon, 12);
        player.Inventory.TryAdd(equippedWeapon);
        player.Equip(equippedWeapon);
        map.GroundItems.Add(new GroundItem(newWeapon, x + 1, y));

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Same(equippedWeapon, player.EquippedWeapon);
        Assert.Contains(newWeapon, player.Inventory.Items);
        Assert.DoesNotContain("AUTO-EQUIPPED STEEL AXE", gameState.EventLog.Entries);
    }

    [Fact]
    public void FullInventoryLeavesLootOnGround()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        for (int i = 0; i < player.Inventory.Capacity; i++)
        {
            player.Inventory.TryAdd(ItemFactory.Create(ItemType.Key));
        }
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        map.GroundItems.Add(new GroundItem(potion, x + 1, y));

        gameState.Update(PlayerCommand.MoveRight);

        Assert.NotNull(map.GetGroundItemAt(x + 1, y));
        Assert.DoesNotContain(potion, player.Inventory.Items);
        Assert.Contains("INVENTORY FULL HEALING POTION", gameState.EventLog.Entries);
    }

    [Fact]
    public void HealingPotionRestoresHealthAndIsConsumed()
    {
        (_, Player player, GameState gameState, _, _) = CreateGameOnOpenFloor();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        player.Inventory.TryAdd(potion);
        player.TakeDamage(50);
        int injuredHealth = player.Health;

        gameState.Update(PlayerCommand.UsePotion);

        Assert.Equal(Math.Min(player.MaxHealth, injuredHealth + potion.Power), player.Health);
        Assert.DoesNotContain(potion, player.Inventory.Items);
        Assert.Contains(gameState.EventLog.Entries, entry => entry.StartsWith("HEALED "));
    }

    [Fact]
    public void FailedPotionUseDoesNotGiveNpcATurn()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        var npc = new Orc(map, x, y + 1, null)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.UsePotion);

        Assert.False(player.Dead);
        Assert.Equal(GameOutcome.Playing, gameState.Outcome);
        Assert.Contains("SELECT A HEALING POTION", gameState.EventLog.Entries);
    }

    [Fact]
    public void EquippingWeaponAddsItsPowerToDamage()
    {
        (_, Player player, GameState gameState, _, _) = CreateGameOnOpenFloor();
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        player.Inventory.TryAdd(weapon);
        int baseDamage = player.BaseDamage;

        gameState.Update(PlayerCommand.EquipItem);

        Assert.Same(weapon, player.EquippedWeapon);
        Assert.Equal(baseDamage + weapon.Power, player.Damage);
        Assert.Contains("EQUIPPED IRON SWORD", gameState.EventLog.Entries);
    }

    [Fact]
    public void KilledNpcDropsCarriedItem()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item key = ItemFactory.Create(ItemType.Key);
        var npc = new Goblin(map, x + 1, y, null)
        {
            State = NPCState.Active,
            HP = player.Damage,
            HeldItem = key
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.MoveRight);

        Assert.Equal(NPCState.Dead, npc.State);
        Assert.Same(key, map.GetGroundItemAt(x + 1, y)?.Item);
        Assert.Null(npc.HeldItem);
        Assert.Contains($"{npc.Name} DROPPED BRASS KEY", gameState.EventLog.Entries);
    }

    [Fact]
    public void DropPlacesMostRecentlyAcquiredItemOnCurrentCell()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        Item key = ItemFactory.Create(ItemType.Key);
        player.Inventory.TryAdd(potion);
        player.Inventory.TryAdd(key);
        player.Inventory.SelectNext();

        gameState.Update(PlayerCommand.DropItem);

        Assert.Same(key, map.GetGroundItemAt(x, y)?.Item);
        Assert.DoesNotContain(key, player.Inventory.Items);
        Assert.Contains(potion, player.Inventory.Items);
        Assert.Contains("DROPPED BRASS KEY", gameState.EventLog.Entries);
    }

    [Fact]
    public void DroppingEquippedWeaponUnequipsItAndRemovesDamageBonus()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        player.Inventory.TryAdd(weapon);
        player.Equip(weapon);

        gameState.Update(PlayerCommand.DropItem);

        Assert.Null(player.EquippedWeapon);
        Assert.Equal(player.BaseDamage, player.Damage);
        Assert.Same(weapon, map.GetGroundItemAt(x, y)?.Item);
    }

    [Fact]
    public void FailedDropDoesNotAdvanceNpcTurn()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        var npc = new Orc(map, x, y + 1, null)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.DropItem);

        Assert.False(player.Dead);
        Assert.Contains("INVENTORY EMPTY", gameState.EventLog.Entries);
    }

    [Fact]
    public void CannotDropOntoExistingGroundItem()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item carriedKey = ItemFactory.Create(ItemType.Key);
        Item groundPotion = ItemFactory.Create(ItemType.HealingPotion);
        player.Inventory.TryAdd(carriedKey);
        map.GroundItems.Add(new GroundItem(groundPotion, x, y));

        gameState.Update(PlayerCommand.DropItem);

        Assert.Contains(carriedKey, player.Inventory.Items);
        Assert.Same(groundPotion, map.GetGroundItemAt(x, y)?.Item);
        Assert.Contains("CANNOT DROP HERE", gameState.EventLog.Entries);
    }

    [Fact]
    public void InventorySelectionWrapsAndSurvivesRemoval()
    {
        var inventory = new Inventory();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        inventory.TryAdd(potion);
        inventory.TryAdd(weapon);

        Assert.Same(potion, inventory.SelectedItem);
        inventory.SelectPrevious();
        Assert.Same(weapon, inventory.SelectedItem);
        inventory.SelectNext();
        Assert.Same(potion, inventory.SelectedItem);

        inventory.Remove(potion);

        Assert.Same(weapon, inventory.SelectedItem);
        Assert.Equal(0, inventory.SelectedIndex);
    }

    [Fact]
    public void SelectionWithOneItemIsSilentAndDoesNotAdvanceTurn()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        player.Inventory.TryAdd(ItemFactory.Create(ItemType.Key));
        var npc = new Orc(map, x, y + 1, null)
        {
            State = NPCState.Active,
            Damage = player.Health
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.SelectNextItem);
        gameState.Update(PlayerCommand.SelectPreviousItem);

        Assert.False(player.Dead);
        Assert.Equal(0, gameState.TurnCount);
        Assert.Empty(gameState.EventLog.Entries);
        Assert.Equal(0, player.Inventory.SelectedIndex);
    }

    [Fact]
    public void SelectionWithEmptyInventoryIsSilent()
    {
        (_, _, GameState gameState, _, _) = CreateGameOnOpenFloor();

        gameState.Update(PlayerCommand.SelectNextItem);
        gameState.Update(PlayerCommand.SelectPreviousItem);

        Assert.Equal(0, gameState.TurnCount);
        Assert.Empty(gameState.EventLog.Entries);
    }

    [Fact]
    public void SelectionWithMultipleItemsStillWrapsAndReportsSelection()
    {
        (_, Player player, GameState gameState, _, _) = CreateGameOnOpenFloor();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        Item key = ItemFactory.Create(ItemType.Key);
        player.Inventory.TryAdd(potion);
        player.Inventory.TryAdd(key);

        gameState.Update(PlayerCommand.SelectPreviousItem);

        Assert.Same(key, player.Inventory.SelectedItem);
        Assert.Equal(0, gameState.TurnCount);
        Assert.Contains("SELECTED BRASS KEY", gameState.EventLog.Entries);
    }

    [Fact]
    public void DirectSlotSelectionRejectsEmptyAndAlreadySelectedSlots()
    {
        var inventory = new Inventory();
        Item potion = ItemFactory.Create(ItemType.HealingPotion);
        Item key = ItemFactory.Create(ItemType.Key);
        inventory.TryAdd(potion);
        inventory.TryAdd(key);

        Assert.False(inventory.SelectIndex(0));
        Assert.False(inventory.SelectIndex(7));
        Assert.True(inventory.SelectIndex(1));
        Assert.Same(key, inventory.SelectedItem);
    }

    [Fact]
    public void EquippingSelectedEquipmentAgainUnequipsIt()
    {
        (_, Player player, GameState gameState, _, _) = CreateGameOnOpenFloor();
        Item weapon = ItemFactory.Create(ItemType.Weapon);
        player.Inventory.TryAdd(weapon);

        gameState.Update(PlayerCommand.EquipItem);
        gameState.Update(PlayerCommand.EquipItem);

        Assert.Null(player.EquippedWeapon);
        Assert.Equal(player.BaseDamage, player.Damage);
        Assert.Equal(2, gameState.TurnCount);
        Assert.Contains("EQUIPPED IRON SWORD", gameState.EventLog.Entries);
        Assert.Contains("UNEQUIPPED IRON SWORD", gameState.EventLog.Entries);
    }

    [Fact]
    public void SelectedArmorCanBeEquipped()
    {
        (_, Player player, GameState gameState, _, _) = CreateGameOnOpenFloor();
        Item armor = ItemFactory.Create(ItemType.Armor);
        player.Inventory.TryAdd(armor);

        gameState.Update(PlayerCommand.EquipItem);

        Assert.Same(armor, player.EquippedArmor);
        Assert.Equal(armor.Power, player.Defence);
        Assert.Contains("EQUIPPED LEATHER ARMOR", gameState.EventLog.Entries);
    }

    [Fact]
    public void ArmorReducesIncomingDamageAndCombatEventReportsActualDamage()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item armor = ItemFactory.Create(ItemType.Armor);
        player.Inventory.TryAdd(armor);
        player.EquipArmor(armor);
        int initialHealth = player.Health;
        var npc = new Orc(map, x, y + 1, null)
        {
            State = NPCState.Active,
            Damage = armor.Power + 3
        };
        map.NPCs.Add(npc);

        gameState.Update(PlayerCommand.Wait);

        Assert.Equal(initialHealth - 3, player.Health);
        Assert.Contains($"{npc.Name} HIT PLAYER 3", gameState.EventLog.Entries);
    }

    [Fact]
    public void ArmorCannotReduceSuccessfulHitBelowOneDamage()
    {
        var player = new Player();
        Item armor = ItemFactory.Create(ItemType.Armor);
        player.Inventory.TryAdd(armor);
        player.EquipArmor(armor);
        int initialHealth = player.Health;

        int damage = player.TakeDamage(1);

        Assert.Equal(1, damage);
        Assert.Equal(initialHealth - 1, player.Health);
    }

    [Fact]
    public void DroppingEquippedArmorUnequipsIt()
    {
        (Map map, Player player, GameState gameState, int x, int y) = CreateGameOnOpenFloor();
        Item armor = ItemFactory.Create(ItemType.Armor);
        player.Inventory.TryAdd(armor);
        player.EquipArmor(armor);

        gameState.Update(PlayerCommand.DropItem);

        Assert.Null(player.EquippedArmor);
        Assert.Equal(0, player.Defence);
        Assert.Same(armor, map.GetGroundItemAt(x, y)?.Item);
    }

    [Fact]
    public void GeneratedLootUsesReachableUnoccupiedCells()
    {
        var map = new Map(123);
        var visited = ReachableCells(map);

        Assert.Equal(7, map.GroundItems.Count);
        Assert.Contains(map.GroundItems, item => item.Item.Type == ItemType.Armor);
        Assert.All(map.GroundItems, groundItem =>
        {
            Assert.Contains((groundItem.X, groundItem.Y), visited);
            Assert.False(map.IsOccupiedByLivingNPC(groundItem.X, groundItem.Y));
            Assert.NotEqual(MapCellType.Special, map.MapCells[groundItem.X, groundItem.Y].CellType);
        });
        Assert.Equal(map.GroundItems.Count,
            map.GroundItems.Select(item => (item.X, item.Y)).Distinct().Count());
    }

    private static (Map Map, Player Player, GameState GameState, int X, int Y) CreateGameOnOpenFloor()
    {
        var map = new Map(123);
        map.NPCs.Clear();
        map.GroundItems.Clear();

        for (int y = 1; y < map.Height - 1; y++)
        {
            for (int x = 1; x < map.Width - 2; x++)
            {
                if (map.IsWalkable(x, y) && map.IsWalkable(x + 1, y))
                {
                    var player = new Player { X = x, Y = y };
                    map.CurrentPlayerX = x;
                    map.CurrentPlayerY = y;
                    return (map, player, new GameState(map, player), x, y);
                }
            }
        }

        throw new InvalidOperationException("Generated map contained no adjacent floor cells.");
    }

    private static HashSet<(int X, int Y)> ReachableCells(Map map)
    {
        var visited = new HashSet<(int X, int Y)> { (map.StartPosX, map.StartPosY) };
        var frontier = new Queue<(int X, int Y)>();
        frontier.Enqueue((map.StartPosX, map.StartPosY));

        while (frontier.TryDequeue(out var current))
        {
            foreach ((int dx, int dy) in new[] { (0, -1), (0, 1), (-1, 0), (1, 0) })
            {
                var next = (X: current.X + dx, Y: current.Y + dy);
                bool traversableTerrain = next.X >= 0 && next.X < map.Width
                    && next.Y >= 0 && next.Y < map.Height
                    && map.MapCells[next.X, next.Y].CellType != MapCellType.Wall;
                if (traversableTerrain && visited.Add(next)) frontier.Enqueue(next);
            }
        }

        return visited;
    }
}
