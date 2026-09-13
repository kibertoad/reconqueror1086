namespace Conqueror.Core;

public enum Facing { North, East, South, West }
public enum SiegeTile { Floor, Wall, Door, SecretDoor, Barrel, Treasure, Exit, Destructible }
public enum SiegeAction { None, Moved, Blocked, DoorOpened, Healed, Looted, Hit, Missed, WeaponBroke, Shot, NoAmmunition, Exited }
public enum SiegeEnemyVisualState { Walk, Attack, Hit, Dying }
public enum SiegePickupRewardKind { Wealth, Healing, Equipment, CrossbowBolts }
public enum SiegeRetainerCommand { Defend = 2, Attack = 6, Retreat = 10, Follow = 16 }

public class SiegeEnemy
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; }
    public int? OriginalArmor { get; init; }
    public int? OriginalAttackSkill { get; init; }
    public int? OriginalCombatRow { get; init; }
    public int? OriginalActorKind { get; init; }
    public SiegeActorAnimation? OriginalAnimation { get; init; }
    public SiegeActorMovement? OriginalMovement { get; init; }
    public int OffsetX8 { get; internal set; }
    public int OffsetY8 { get; internal set; }
    public bool Champion { get; init; }
    public int VisualId { get; init; } = -1;
    public Facing Facing { get; set; }
    public int WalkFrame { get; set; }
    public SiegeEnemyVisualState VisualState { get; internal set; }
    internal double VisualElapsed { get; set; }
}

public sealed class SiegeRetainer : SiegeEnemy
{
    public SiegeRetainerCommand Command { get; internal set; } = SiegeRetainerCommand.Attack;
    public bool Selected { get; internal set; }
    internal SiegeEnemy? OrderedTarget { get; set; }
    internal (int X, int Y)? OrderedDestination { get; set; }
    internal double MovementElapsed { get; set; }
    internal int MovementTick { get; set; }
}

public sealed record SiegeDefinition(int Width, int Height, int BaseEnemies, int GarrisonPerEnemy,
    int BaseChampionHealth, int WeaponBreakPercent);
public sealed record SiegeSpawn(int X, int Y, bool Champion, int VisualId = -1,
    int? OriginalArmor = null, int? OriginalHealth = null, int? OriginalCombatRow = null,
    int? OriginalAttackSkill = null, SiegeActorAnimation? OriginalAnimation = null,
    SiegeActorMovement? OriginalMovement = null, int InitialOffsetX8 = 0, int InitialOffsetY8 = 0,
    int? OriginalActorKind = null);
public sealed record SiegeActorAnimation(double AttackSeconds, double HitSeconds, double DeathSeconds);
public sealed record SiegeActorMovement(
    int TickCount, int IntervalMilliseconds, int FixedXDeltaPerTick, int FixedYDeltaPerTick, int Flags,
    int SurfaceIndexDeltaPerTick = 0)
{
    public double TickSeconds => IntervalMilliseconds / 1000d;
    public double NominalSeconds => checked((long)TickCount * IntervalMilliseconds) / 1000d;
}
public sealed record SiegePickupReward(SiegePickupRewardKind Kind, int Amount, int DieSides = 0,
    string? EquipmentName = null);
public sealed record SiegeObjectStage(
    int VisualId, SiegeTile Tile, SiegePickupReward? Pickup = null, bool? BlocksMovement = null)
{
    internal bool MovementBlocked => BlocksMovement ?? Tile != SiegeTile.Floor;
}
public sealed record SiegeObjectSpawn(int X, int Y, IReadOnlyList<SiegeObjectStage> Stages);

public sealed class SiegeObject
{
    private readonly SiegeObjectStage[] _stages;

    internal SiegeObject(SiegeObjectSpawn spawn)
    {
        X = spawn.X;
        Y = spawn.Y;
        _stages = spawn.Stages.ToArray();
    }

    public int X { get; }
    public int Y { get; }
    public int State { get; private set; }
    public int VisualId => _stages[State].VisualId;
    public SiegeTile Tile => _stages[State].Tile;
    public SiegePickupReward? Pickup => _stages[State].Pickup;
    internal bool MovementBlocked => _stages[State].MovementBlocked;
    internal bool Advance()
    {
        if (State + 1 >= _stages.Length) return false;
        State++;
        return true;
    }
}

public sealed class SiegeLayout
{
    private readonly SiegeTile[,] _tiles;
    private readonly bool[,] _movementBlocks;

    public SiegeLayout(SiegeTile[,] tiles, int playerX, int playerY, Facing facing, IReadOnlyList<SiegeSpawn> enemies,
        IReadOnlyList<SiegeObjectSpawn>? objects = null, IReadOnlyList<SiegeSpawn>? retainers = null,
        bool[,]? movementBlocks = null)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        ArgumentNullException.ThrowIfNull(enemies);
        if (tiles.GetLength(0) < 1 || tiles.GetLength(1) < 1) throw new ArgumentException("Siege layout cannot be empty.", nameof(tiles));
        if (playerX < 0 || playerY < 0 || playerX >= tiles.GetLength(0) || playerY >= tiles.GetLength(1))
            throw new ArgumentOutOfRangeException(nameof(playerX), "Siege player start is outside the layout.");
        if (tiles[playerX, playerY] != SiegeTile.Floor) throw new ArgumentException("Siege player must start on a floor tile.", nameof(tiles));
        if (enemies.Any(enemy => enemy.X < 0 || enemy.Y < 0 || enemy.X >= tiles.GetLength(0) || enemy.Y >= tiles.GetLength(1)))
            throw new ArgumentException("Siege enemy starts outside the layout.", nameof(enemies));
        if (enemies.GroupBy(enemy => (enemy.X, enemy.Y)).Any(group => group.Count() > 1))
            throw new ArgumentException("Siege enemies cannot share a map cell.", nameof(enemies));
        retainers ??= [];
        if (retainers.Any(retainer => retainer.X < 0 || retainer.Y < 0 ||
                retainer.X >= tiles.GetLength(0) || retainer.Y >= tiles.GetLength(1)))
            throw new ArgumentException("Siege retainers start outside the layout.", nameof(retainers));
        if (retainers.GroupBy(retainer => (retainer.X, retainer.Y)).Any(group => group.Count() > 1) ||
            retainers.Any(retainer => enemies.Any(enemy => enemy.X == retainer.X && enemy.Y == retainer.Y)) ||
            retainers.Any(retainer => retainer.X == playerX && retainer.Y == playerY))
            throw new ArgumentException("Siege actors cannot share a map cell.", nameof(retainers));
        objects ??= [];
        if (objects.Any(item => item.X < 0 || item.Y < 0 || item.X >= tiles.GetLength(0) || item.Y >= tiles.GetLength(1)
                || item.Stages.Count == 0))
            throw new ArgumentException("Siege objects require a map cell and at least one state.", nameof(objects));
        if (movementBlocks is not null && (movementBlocks.GetLength(0) != tiles.GetLength(0) ||
                movementBlocks.GetLength(1) != tiles.GetLength(1)))
            throw new ArgumentException("Movement-block map dimensions must match the siege layout.", nameof(movementBlocks));

        _tiles = (SiegeTile[,])tiles.Clone();
        _movementBlocks = movementBlocks is null ? MovementBlocksFor(tiles) : (bool[,])movementBlocks.Clone();
        PlayerX = playerX;
        PlayerY = playerY;
        Facing = facing;
        Enemies = enemies.ToArray();
        Retainers = retainers.ToArray();
        Objects = objects.ToArray();
    }

    public int PlayerX { get; }
    public int PlayerY { get; }
    public Facing Facing { get; }
    public IReadOnlyList<SiegeSpawn> Enemies { get; }
    public IReadOnlyList<SiegeSpawn> Retainers { get; }
    public IReadOnlyList<SiegeObjectSpawn> Objects { get; }
    public SiegeTile[,] CopyTiles() => (SiegeTile[,])_tiles.Clone();
    public bool BlocksMovementAt(int x, int y) =>
        x < 0 || y < 0 || x >= _movementBlocks.GetLength(0) || y >= _movementBlocks.GetLength(1) ||
        _movementBlocks[x, y];
    internal bool[,] CopyMovementBlocks() => (bool[,])_movementBlocks.Clone();

    private static bool[,] MovementBlocksFor(SiegeTile[,] tiles)
    {
        var result = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        for (var x = 0; x < tiles.GetLength(0); x++)
        for (var y = 0; y < tiles.GetLength(1); y++)
            result[x, y] = tiles[x, y] != SiegeTile.Floor;
        return result;
    }
}

public sealed partial class SiegeSession
{
    public static readonly SiegeDefinition Rules = new(12, 12, 5, 3, 3, 2);
    public const double FallbackEnemyAttackSeconds = 0.63;
    public const double EnemyHitSeconds = 0.20;
    public const double FallbackEnemyDeathSeconds = 0.72;
    private readonly Player _player;
    private readonly Random _random;
    private readonly SiegeTile[,] _map;
    private readonly bool[,] _movementBlocks;
    public IReadOnlyList<SiegeEnemy> Enemies => _enemies;
    private readonly List<SiegeEnemy> _enemies = [];
    public IReadOnlyList<SiegeRetainer> Retainers => _retainers;
    private readonly List<SiegeRetainer> _retainers = [];
    public IReadOnlyList<SiegeObject> Objects => _objects;
    private readonly List<SiegeObject> _objects = [];

    public int PlayerX { get; private set; } = 1;
    public int PlayerY { get; private set; } = 1;
    public Facing Facing { get; private set; } = Facing.East;
    public int MaxHealth { get; }
    public int Health { get; private set; }
    public int AlliesStarted { get; }
    public int AlliesAlive { get; private set; }
    public int GoldFound { get; private set; }
    public List<string> ItemsFound { get; } = [];
    public string LastMessage { get; private set; } = "Enter the keep and defeat its champion.";
    public bool Won => _enemies.Count == 0;
    public bool Defeated => Health <= 0;
    public int RetainerLosses => AlliesStarted - AlliesAlive;
    public int Width => _map.GetLength(0);
    public int Height => _map.GetLength(1);

    public SiegeSession(Player player, int garrison, int seed)
        : this(player, player.Army, garrison, seed)
    {
    }

    public SiegeSession(Player player, Army army, int garrison, int seed)
        : this(player, army, garrison, seed, null)
    {
    }

    public SiegeSession(Player player, Army army, int garrison, int seed, SiegeLayout? layout,
        bool includeRetainers = true)
    {
        _player = player;
        _random = new Random(seed);
        _map = layout?.CopyTiles() ?? GenerateMap(Rules.Width, Rules.Height);
        _movementBlocks = layout?.CopyMovementBlocks() ?? MovementBlocksFor(_map);
        if (layout is not null)
        {
            PlayerX = layout.PlayerX;
            PlayerY = layout.PlayerY;
            Facing = layout.Facing;
        }
        MaxHealth = OriginalWeaponCombat.PlayerHealth(player);
        Health = MaxHealth;
        var retainerCap = includeRetainers ? OriginalRetainerCombat.CampaignRetainerCapFor(army) : 0;
        AlliesStarted = layout is null ? retainerCap : Math.Min(retainerCap, layout.Retainers.Count);
        AlliesAlive = AlliesStarted;
        if (layout is not null)
        {
            var retainerSpawns = layout.Retainers.ToList();
            while (retainerSpawns.Count > AlliesStarted)
                retainerSpawns.RemoveAt(_random.Next(retainerSpawns.Count));
            _retainers.AddRange(retainerSpawns.Select(RetainerFor));
            foreach (var spawn in layout.Enemies)
            {
                var enemy = new SiegeEnemy
                {
                    X = spawn.X,
                    Y = spawn.Y,
                    Health = spawn.OriginalHealth ?? (spawn.Champion ? Rules.BaseChampionHealth : 1),
                    OriginalArmor = spawn.OriginalArmor,
                    OriginalAttackSkill = spawn.OriginalAttackSkill,
                    OriginalCombatRow = spawn.OriginalCombatRow,
                    OriginalActorKind = spawn.OriginalActorKind,
                    Champion = spawn.Champion,
                    VisualId = spawn.VisualId,
                    OriginalAnimation = spawn.OriginalAnimation,
                    OriginalMovement = spawn.OriginalMovement,
                    OffsetX8 = spawn.InitialOffsetX8,
                    OffsetY8 = spawn.InitialOffsetY8
                };
                enemy.Facing = DirectionToward(enemy.X, enemy.Y, PlayerX, PlayerY, Facing.South);
                _enemies.Add(enemy);
            }
            _objects.AddRange(layout.Objects.Select(spawn => new SiegeObject(spawn)));
        }
        else
        {
            var count = Rules.BaseEnemies + Math.Max(0, garrison / Rules.GarrisonPerEnemy);
            for (var i = 0; i < count; i++)
            {
                var point = EmptySpawn(i);
                _enemies.Add(new SiegeEnemy
                {
                    X = point.X,
                    Y = point.Y,
                    Health = i == count - 1 ? Rules.BaseChampionHealth : 1,
                    Champion = i == count - 1,
                    Facing = DirectionToward(point.X, point.Y, PlayerX, PlayerY, Facing.South)
                });
            }
        }
    }

    private SiegeRetainer RetainerFor(SiegeSpawn spawn) => new()
    {
        X = spawn.X,
        Y = spawn.Y,
        Health = spawn.OriginalHealth ?? 1,
        OriginalArmor = spawn.OriginalArmor,
        OriginalAttackSkill = spawn.OriginalAttackSkill,
        OriginalCombatRow = spawn.OriginalCombatRow,
        OriginalActorKind = spawn.OriginalActorKind,
        VisualId = spawn.VisualId,
        OriginalAnimation = spawn.OriginalAnimation,
        OriginalMovement = spawn.OriginalMovement,
        OffsetX8 = spawn.InitialOffsetX8,
        OffsetY8 = spawn.InitialOffsetY8,
        Facing = DirectionToward(spawn.X, spawn.Y, PlayerX, PlayerY, Facing.South)
    };

    public SiegeTile TileAt(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height ? SiegeTile.Wall : _map[x, y];
    public SiegeEnemy? EnemyAt(int x, int y) => _enemies.FirstOrDefault(e => e.Health > 0 && e.X == x && e.Y == y);
    public SiegeRetainer? RetainerAt(int x, int y) =>
        _retainers.FirstOrDefault(retainer => retainer.Health > 0 && retainer.X == x && retainer.Y == y);
    public SiegeObject? ObjectAt(int x, int y) => _objects.FirstOrDefault(item => item.X == x && item.Y == y);

    public void ToggleRetainerSelection(int index)
    {
        if ((uint)index >= (uint)_retainers.Count) throw new ArgumentOutOfRangeException(nameof(index));
        ToggleRetainerSelection(_retainers[index]);
    }

    public void ToggleRetainerSelection(SiegeRetainer retainer)
    {
        ArgumentNullException.ThrowIfNull(retainer);
        if (!_retainers.Contains(retainer)) throw new ArgumentException("Retainer does not belong to this battle.", nameof(retainer));
        if (retainer.Health <= 0) return;
        retainer.Selected = !retainer.Selected;
        LastMessage = retainer.Selected ? "Retainer selected." : "Retainer released.";
    }

    public void CommandRetainers(SiegeRetainerCommand command)
    {
        if (!Enum.IsDefined(command)) throw new ArgumentOutOfRangeException(nameof(command));
        var living = _retainers.Where(retainer => retainer.Health > 0).ToArray();
        var selected = living.Where(retainer => retainer.Selected).ToArray();
        var targets = selected.Length > 0 ? selected : living;
        foreach (var retainer in targets)
        {
            retainer.Command = command;
            retainer.OrderedTarget = null;
            retainer.OrderedDestination = null;
            ResetRetainerMovement(retainer);
        }
        foreach (var retainer in living) retainer.Selected = false;
        LastMessage = targets.Length == 0
            ? "No retainers can hear the order."
            : $"Retainers: {command}.";
    }

    public bool CommandSelectedRetainersAt(SiegeEnemy target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_enemies.Contains(target)) throw new ArgumentException("Target does not belong to this battle.", nameof(target));
        if (target.Health <= 0) return false;
        var living = _retainers.Where(retainer => retainer.Health > 0).ToArray();
        var selected = living.Where(retainer => retainer.Selected).ToArray();
        if (selected.Length == 0) return false;
        foreach (var retainer in selected)
        {
            retainer.Command = SiegeRetainerCommand.Attack;
            retainer.OrderedTarget = target;
            retainer.OrderedDestination = null;
            ResetRetainerMovement(retainer);
        }
        foreach (var retainer in living) retainer.Selected = false;
        LastMessage = "Retainers attack the selected defender.";
        return true;
    }

    public bool CommandSelectedRetainersTo(int x, int y)
    {
        if (TileAt(x, y) != SiegeTile.Floor) return false;
        var living = _retainers.Where(retainer => retainer.Health > 0).ToArray();
        var selected = living.Where(retainer => retainer.Selected).ToArray();
        if (selected.Length == 0) return false;
        foreach (var retainer in selected)
        {
            retainer.OrderedTarget = null;
            retainer.OrderedDestination = (x, y);
            ResetRetainerMovement(retainer);
        }
        foreach (var retainer in living) retainer.Selected = false;
        LastMessage = "Retainers move to the selected ground.";
        return true;
    }

    public void AdvanceEnemyAnimations(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        var actors = _enemies.Cast<SiegeEnemy>().Concat(_retainers);
        foreach (var enemy in actors.Where(enemy => enemy.VisualState != SiegeEnemyVisualState.Walk).ToArray())
        {
            enemy.VisualElapsed += elapsedSeconds;
            var duration = enemy.VisualState switch
            {
                SiegeEnemyVisualState.Attack => enemy.OriginalAnimation?.AttackSeconds is { } attack
                    ? attack * (enemy.OriginalCombatRow is >= 23 ? 2 : 1)
                    : FallbackEnemyAttackSeconds,
                SiegeEnemyVisualState.Hit => enemy.OriginalAnimation?.HitSeconds ?? EnemyHitSeconds,
                SiegeEnemyVisualState.Dying => enemy.OriginalAnimation?.DeathSeconds ?? FallbackEnemyDeathSeconds,
                _ => 0
            };
            if (enemy.VisualElapsed <= duration) continue;
            if (enemy.VisualState == SiegeEnemyVisualState.Dying && enemy is not SiegeRetainer)
            {
                _enemies.Remove(enemy);
                continue;
            }
            enemy.VisualState = SiegeEnemyVisualState.Walk;
            enemy.VisualElapsed = 0;
        }
    }

    public void TurnLeft() { Facing = (Facing)(((int)Facing + 3) % 4); LastMessage = $"Facing {Facing}."; }
    public void TurnRight() { Facing = (Facing)(((int)Facing + 1) % 4); LastMessage = $"Facing {Facing}."; }

    public SiegeAction Move(bool forward)
    {
        var (dx, dy) = Direction(Facing);
        if (!forward) { dx = -dx; dy = -dy; }
        var nx = PlayerX + dx; var ny = PlayerY + dy;
        var tile = TileAt(nx, ny);
        if (tile == SiegeTile.Exit)
        {
            LastMessage = "You leave the battle.";
            return SiegeAction.Exited;
        }
        if (tile is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Destructible || EnemyAt(nx, ny) is not null)
        {
            LastMessage = "The way is blocked."; TickEnemies(); return SiegeAction.Blocked;
        }
        PlayerX = nx; PlayerY = ny;
        TickEnemies();
        return SiegeAction.Moved;
    }

    public SiegeAction Interact()
    {
        var (dx, dy) = Direction(Facing);
        for (var distance = 1; distance <= 2; distance++)
        {
            var x = PlayerX + dx * distance;
            var y = PlayerY + dy * distance;
            var tile = TileAt(x, y);
            if (tile is SiegeTile.Barrel or SiegeTile.Treasure)
            {
                var result = CollectTile(x, y);
                TickEnemies();
                return result;
            }
            if (tile is SiegeTile.Door or SiegeTile.SecretDoor)
            {
                var door = ObjectAt(x, y);
                if (door is null)
                {
                    // Clean-room fallback doors have no resource state record.
                    _map[x, y] = SiegeTile.Floor;
                    LastMessage = "The door opens."; TickEnemies(); return SiegeAction.DoorOpened;
                }
                if (!AdvanceObject(door))
                {
                    LastMessage = "Nothing happens."; TickEnemies(); return SiegeAction.None;
                }
                var opened = TileAt(x, y) is not (SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Destructible);
                LastMessage = opened ? "The door opens." : "The way remains blocked.";
                TickEnemies(); return opened ? SiegeAction.DoorOpened : SiegeAction.Blocked;
            }
            if (tile is SiegeTile.Wall or SiegeTile.Destructible || EnemyAt(x, y) is not null)
                break;
        }
        LastMessage = "Nothing happens."; TickEnemies(); return SiegeAction.None;
    }

    public SiegeAction Interact(SiegeObject target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_objects.Contains(target)) throw new ArgumentException("Object does not belong to this battle.", nameof(target));
        var distance = Math.Sqrt(Math.Pow(target.X - PlayerX, 2) + Math.Pow(target.Y - PlayerY, 2));
        if (distance >= 2.5)
        {
            LastMessage = "That is too far away.";
            TickEnemies();
            return SiegeAction.None;
        }
        if (target.Pickup is not null)
        {
            var result = CollectTile(target.X, target.Y);
            TickEnemies();
            return result;
        }
        if (target.Tile is not (SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Destructible) ||
            !AdvanceObject(target))
        {
            LastMessage = "Nothing happens.";
            TickEnemies();
            return SiegeAction.None;
        }
        var opened = TileAt(target.X, target.Y) is not
            (SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Destructible);
        LastMessage = opened ? "The way opens." : "The way remains blocked.";
        TickEnemies();
        return opened ? SiegeAction.DoorOpened : SiegeAction.Blocked;
    }

    public SiegeAction Attack() => AttackCore(null);

    public SiegeAction Attack(SiegeEnemy target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_enemies.Contains(target)) throw new ArgumentException("Target does not belong to this battle.", nameof(target));
        return AttackCore(target);
    }

    private SiegeAction AttackCore(SiegeEnemy? requestedTarget)
    {
        var weapon = Balance.Equipment.FirstOrDefault(x => x.Name.Equals(_player.Inventory.Weapon, StringComparison.OrdinalIgnoreCase));
        var originalItemId = weapon?.OriginalWeaponItemId;
        var reach = originalItemId is { } reachItemId
            ? OriginalWeaponCombat.GridReachFor(reachItemId)
            : weapon is null ? 1 : Math.Clamp(weapon.Power / 70, 1, 2);
        var target = requestedTarget is null
            ? FirstEnemyAhead(reach)
            : PlayerCanReach(requestedTarget, originalItemId, reach) ? requestedTarget : null;
        if (target is null)
        {
            if (requestedTarget is null && FirstDestructibleAhead(reach) is { } obstacle)
            {
                AdvanceObject(obstacle);
                LastMessage = "You smash through the obstacle.";
                TickEnemies();
                return SiegeAction.Hit;
            }
            LastMessage = "Your blow meets empty air."; TickEnemies(); return SiegeAction.Missed;
        }
        var hit = weapon?.OriginalWeaponItemId is { } attackItemId && target.OriginalAttackSkill is { } targetSkill
            ? OriginalWeaponCombat.Hits(OriginalWeaponCombat.PlayerAttackSkill(_player), targetSkill,
                IsCloseRangedAttack(attackItemId, target), Facing == target.Facing, _random)
            : _random.Next(220) < Math.Clamp((weapon?.Power ?? 35) + _player.Stats.Dexterity * 3
                - (target.Champion ? 35 : 0), 15, 210);
        if (hit)
        {
            target.Health -= weapon?.OriginalWeaponItemId is { } damageItemId && target.OriginalArmor is { } armor
                ? OriginalWeaponCombat.DamageFor(damageItemId, armor, _random)
                : 1 + _player.Stats.Strength / 16;
            if (target.Health > 0) StartVisual(target, SiegeEnemyVisualState.Hit);
            LastMessage = target.Champion ? "You strike the castle champion." : "Your weapon finds its mark.";
            RemoveDead();
        }
        else LastMessage = "The enemy turns your blow.";
        var usesOriginalBreakRule = originalItemId is not null;
        var originalBreakRange = usesOriginalBreakRule
            ? OriginalWeaponCombat.BreakRollRangeFor(originalItemId!.Value)
            : null;
        var broke = !hit && weapon is not null && (usesOriginalBreakRule
            ? originalBreakRange is { } range && _random.Next(range) == 0
            : weapon.BuyPrice > 0 && _random.Next(100) < Rules.WeaponBreakPercent);
        if (broke)
        {
            _player.Inventory.Items.Remove(weapon!.Name);
            _player.Inventory.Unequip(EquipmentSlot.Weapon);
            LastMessage = $"Your {weapon.Name} breaks!";
        }
        TickEnemies();
        return broke ? SiegeAction.WeaponBroke : target.Health <= 0 ? SiegeAction.Hit : SiegeAction.Hit;
    }

    public SiegeAction Shoot() => ShootCore(null);

    public SiegeAction Shoot(SiegeEnemy target)
    {
        ArgumentNullException.ThrowIfNull(target);
        if (!_enemies.Contains(target)) throw new ArgumentException("Target does not belong to this battle.", nameof(target));
        return ShootCore(target);
    }

    private SiegeAction ShootCore(SiegeEnemy? requestedTarget)
    {
        if (_player.Inventory.CrossbowBolts <= 0) { LastMessage = "You have no crossbow bolts."; return SiegeAction.NoAmmunition; }
        if (!_player.Inventory.Weapon.Contains("Crossbow", StringComparison.OrdinalIgnoreCase)) { LastMessage = "Equip a crossbow first."; return SiegeAction.NoAmmunition; }
        var weapon = Balance.Equipment.FirstOrDefault(item =>
            item.Name.Equals(_player.Inventory.Weapon, StringComparison.OrdinalIgnoreCase));
        var range = weapon?.OriginalWeaponItemId is { } originalItemId
            ? OriginalWeaponCombat.GridReachFor(originalItemId)
            : 8;
        _player.Inventory.CrossbowBolts--;
        var target = requestedTarget is null
            ? FirstEnemyAhead(range)
            : PlayerCanReach(requestedTarget, weapon?.OriginalWeaponItemId, range) ? requestedTarget : null;
        if (target is not null)
        {
            var hit = weapon?.OriginalWeaponItemId is { } attackItemId && target.OriginalAttackSkill is { } targetSkill
                ? OriginalWeaponCombat.Hits(OriginalWeaponCombat.PlayerAttackSkill(_player), targetSkill,
                    closeRanged: IsCloseRangedAttack(attackItemId, target),
                    behindDefender: Facing == target.Facing, _random)
                : true;
            if (hit)
            {
                target.Health -= weapon?.OriginalWeaponItemId is { } damageItemId && target.OriginalArmor is { } armor
                    ? OriginalWeaponCombat.DamageFor(damageItemId, armor, _random)
                    : 2;
                if (target.Health > 0) StartVisual(target, SiegeEnemyVisualState.Hit);
                RemoveDead(); LastMessage = "The bolt strikes true.";
            }
            else LastMessage = "The enemy turns your shot.";
        }
        else LastMessage = "The bolt vanishes into the dark.";
        TickEnemies(); return SiegeAction.Shot;
    }

    public int VisibleEnemyDistance()
    {
        var (dx, dy) = Direction(Facing);
        for (var distance = 1; distance <= 8; distance++)
        {
            var x = PlayerX + dx * distance; var y = PlayerY + dy * distance;
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Exit or SiegeTile.Destructible) return 0;
            if (EnemyAt(x, y) is not null) return distance;
        }
        return 0;
    }

    public int ArmorRating()
    {
        var equipped = new[] { _player.Inventory.Armor, _player.Inventory.Shield, _player.Inventory.Helm };
        var armor = equipped.Sum(name => Balance.Equipment.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Armor ?? 0);
        if (!_player.Inventory.Armor.Equals("Gambeson", StringComparison.OrdinalIgnoreCase) && _player.Inventory.Items.Contains("Gambeson")) armor += 5;
        return armor;
    }

    private SiegeAction CollectTile(int x, int y)
    {
        var pickup = ObjectAt(x, y)?.Pickup ?? throw new InvalidOperationException(
            "A pickup tile is missing its decoded original reward metadata.");
        // CONQUER.EXE 0x51E60 dispatches on block+0x48. The four placed
        // pickup families mutate one resource, then the caller replaces
        // the scene cell through its block+0x40 target.
        var result = ApplyPickup(pickup);
        ConsumeObjectAt(x, y);
        return result;
    }

    private SiegeAction ApplyPickup(SiegePickupReward pickup)
    {
        switch (pickup.Kind)
        {
            case SiegePickupRewardKind.Wealth:
                GoldFound += pickup.Amount;
                _player.Wealth += pickup.Amount;
                LastMessage = $"Found {pickup.Amount}s.";
                break;
            case SiegePickupRewardKind.Healing:
                var healing = Roll(pickup.Amount, pickup.DieSides);
                Health = Math.Min(MaxHealth, Health + healing);
                LastMessage = $"Food restores {healing} health.";
                return SiegeAction.Healed;
            case SiegePickupRewardKind.Equipment:
                if (_player.Inventory.Items.Add(pickup.EquipmentName!))
                    ItemsFound.Add(pickup.EquipmentName!);
                LastMessage = $"Found {pickup.EquipmentName}.";
                break;
            case SiegePickupRewardKind.CrossbowBolts:
                _player.Inventory.CrossbowBolts += pickup.Amount;
                LastMessage = $"Found {pickup.Amount} crossbow bolts.";
                break;
        }
        return SiegeAction.Looted;
    }

    private int Roll(int count, int sides)
    {
        var result = 0;
        for (var die = 0; die < count; die++) result += _random.Next(sides) + 1;
        return result;
    }

    private void ConsumeObjectAt(int x, int y)
    {
        if (ObjectAt(x, y) is { } item && item.Advance())
        {
            _map[x, y] = item.Tile;
            _movementBlocks[x, y] = item.MovementBlocked;
        }
        else
        {
            _map[x, y] = SiegeTile.Floor;
            _movementBlocks[x, y] = false;
        }
    }

    private void TickEnemies()
    {
        if (Defeated || !_enemies.Any(enemy => enemy.Health > 0)) return;
        foreach (var enemy in _enemies.Where(enemy => enemy.Health > 0).ToArray())
        {
            var playerDistance = Distance(enemy.X, enemy.Y, PlayerX, PlayerY);
            var retainerTarget = _retainers.Where(retainer => retainer.Health > 0)
                .OrderBy(retainer => Distance(enemy.X, enemy.Y, retainer.X, retainer.Y))
                .FirstOrDefault();
            var targetRetainer = retainerTarget is not null &&
                Distance(enemy.X, enemy.Y, retainerTarget.X, retainerTarget.Y) < playerDistance
                    ? retainerTarget
                    : null;
            var targetX = targetRetainer?.X ?? PlayerX;
            var targetY = targetRetainer?.Y ?? PlayerY;
            var distance = Distance(enemy.X, enemy.Y, targetX, targetY);
            if (CanEnemyAttack(enemy, targetX, targetY, distance))
            {
                enemy.Facing = DirectionToward(enemy.X, enemy.Y, targetX, targetY, enemy.Facing);
                enemy.WalkFrame = 0;
                if (enemy.VisualState != SiegeEnemyVisualState.Hit)
                    StartVisual(enemy, SiegeEnemyVisualState.Attack);
                if (targetRetainer is not null)
                {
                    EnemyAttackRetainer(enemy, targetRetainer, distance);
                    continue;
                }
                var hit = enemy.OriginalCombatRow is { } enemyRow && enemy.OriginalAttackSkill is { } enemySkill
                    ? OriginalWeaponCombat.Hits(enemySkill, OriginalWeaponCombat.PlayerAttackSkill(_player),
                        enemyRow >= 23 && distance <= 1, enemy.Facing == Facing, _random)
                    : _random.Next(100) < Math.Clamp(70 - ArmorRating(), 5, 70);
                if (hit)
                    Health -= enemy.OriginalCombatRow is { } combatRow
                        ? OriginalWeaponCombat.DamageForCombatRow(combatRow, ArmorRating(), _random)
                        : Math.Max(2, 12 - _player.Stats.Stamina / 3);
                continue;
            }
            if (distance > 6 || _random.Next(100) >= 55) continue;
            MoveEnemyToward(enemy, targetX, targetY);
        }
        AdvanceRetainerOrders();
        Health = Math.Max(0, Health);
    }

    public void AdvanceRetainerOrders()
    {
        foreach (var retainer in _retainers.Where(retainer => retainer.Health > 0).ToArray())
        {
            if (retainer.OrderedDestination is { } destination)
            {
                if (retainer.X == destination.X && retainer.Y == destination.Y)
                    retainer.OrderedDestination = null;
                continue;
            }
            var target = RetainerOrderTarget(retainer);
            if (target is null) retainer.OrderedTarget = null;
            switch (retainer.Command)
            {
                case SiegeRetainerCommand.Defend:
                    if (target is not null && IsNeighbor(retainer.X, retainer.Y, target.X, target.Y))
                        RetainerAttack(retainer, target);
                    break;
                case SiegeRetainerCommand.Attack:
                    if (target is null) break;
                    RetainerAttack(retainer, target);
                    break;
                case SiegeRetainerCommand.Follow:
                    break;
                case SiegeRetainerCommand.Retreat:
                    break;
            }
        }
    }

    private bool RetainerAttack(SiegeRetainer retainer, SiegeEnemy target)
    {
        var distance = Distance(retainer.X, retainer.Y, target.X, target.Y);
        if (distance > RetainerAttackRange(retainer)) return false;
        retainer.Facing = DirectionToward(retainer.X, retainer.Y, target.X, target.Y, retainer.Facing);
        StartVisual(retainer, SiegeEnemyVisualState.Attack);
        var hit = retainer.OriginalAttackSkill is { } skill && target.OriginalAttackSkill is { } targetSkill
            ? OriginalWeaponCombat.Hits(skill, targetSkill,
                retainer.OriginalCombatRow is >= 23 && distance <= 1,
                retainer.Facing == target.Facing, _random)
            : _random.Next(100) < 50;
        if (!hit) return true;
        target.Health -= retainer.OriginalCombatRow is { } combatRow && target.OriginalArmor is { } armor
            ? OriginalWeaponCombat.DamageForCombatRow(combatRow, armor, _random)
            : 1;
        if (target.Health > 0) StartVisual(target, SiegeEnemyVisualState.Hit);
        RemoveDead();
        LastMessage = target.Health <= 0
            ? "Your retainer brings down a defender."
            : "Your retainer strikes a defender.";
        return true;
    }

    private SiegeEnemy? RetainerOrderTarget(SiegeRetainer retainer) =>
        retainer.OrderedTarget is { Health: > 0 } ordered && _enemies.Contains(ordered)
            ? ordered
            : _enemies.Where(enemy => enemy.Health > 0)
                .OrderBy(enemy => Distance(retainer.X, retainer.Y, enemy.X, enemy.Y))
                .FirstOrDefault();

    private static int RetainerAttackRange(SiegeRetainer retainer) =>
        retainer.OriginalCombatRow is { } row
            ? OriginalWeaponCombat.GridReachForCombatRow(row)
            : 1;

    private static void ResetRetainerMovement(SiegeRetainer retainer)
    {
        retainer.MovementElapsed = 0;
        retainer.MovementTick = 0;
    }

    private void EnemyAttackRetainer(SiegeEnemy enemy, SiegeRetainer retainer, int distance)
    {
        var hit = enemy.OriginalCombatRow is { } enemyRow && enemy.OriginalAttackSkill is { } enemySkill &&
                  retainer.OriginalAttackSkill is { } retainerSkill
            ? OriginalWeaponCombat.Hits(enemySkill, retainerSkill,
                enemyRow >= 23 && distance <= 1, enemy.Facing == retainer.Facing, _random)
            : _random.Next(100) < 50;
        if (!hit) return;
        retainer.Health -= enemy.OriginalCombatRow is { } combatRow && retainer.OriginalArmor is { } armor
            ? OriginalWeaponCombat.DamageForCombatRow(combatRow, armor, _random)
            : 1;
        if (retainer.Health > 0)
        {
            StartVisual(retainer, SiegeEnemyVisualState.Hit);
            LastMessage = "A defender strikes your retainer.";
            return;
        }
        retainer.Health = 0;
        AlliesAlive--;
        StartVisual(retainer, SiegeEnemyVisualState.Dying);
        LastMessage = "A retainer falls in battle.";
    }

    private SiegeEnemy? FirstEnemyAhead(int range)
    {
        var (dx, dy) = Direction(Facing);
        for (var i = 1; i <= range; i++)
        {
            var x = PlayerX + dx * i; var y = PlayerY + dy * i;
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Exit or SiegeTile.Destructible) return null;
            if (EnemyAt(x, y) is { } enemy) return enemy;
        }
        return null;
    }

    private bool PlayerCanReach(SiegeEnemy target, int? originalItemId, int fallbackReach)
    {
        if (target.Health <= 0) return false;
        var dx = target.X - PlayerX;
        var dy = target.Y - PlayerY;
        var fixedDistance = PlayerDistanceInFixedPoint(target);
        var fixedReach = originalItemId is { } itemId
            ? OriginalWeaponCombat.ContactDistanceFor(itemId)
            : fallbackReach * 256;
        if (fixedDistance >= fixedReach) return false;
        var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        for (var step = 1; step < steps; step++)
        {
            var x = PlayerX + (int)Math.Round(dx * step / (double)steps);
            var y = PlayerY + (int)Math.Round(dy * step / (double)steps);
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or
                SiegeTile.Exit or SiegeTile.Destructible)
                return false;
        }
        return true;
    }

    private bool IsCloseRangedAttack(int originalItemId, SiegeEnemy target) =>
        OriginalWeaponCombat.CombatRowFor(originalItemId) >= 23 && PlayerDistanceInFixedPoint(target) <= 256;

    private int PlayerDistanceInFixedPoint(SiegeEnemy target)
    {
        var dx = target.X - PlayerX;
        var dy = target.Y - PlayerY;
        return (int)Math.Round(Math.Sqrt(dx * dx + dy * dy) * 256.0);
    }

    private bool CanEnemyAttack(SiegeEnemy enemy, int targetX, int targetY, int distance)
    {
        var range = enemy.OriginalCombatRow is { } combatRow
            ? OriginalWeaponCombat.GridReachForCombatRow(combatRow)
            : 1;
        if (distance > range || enemy.X != targetX && enemy.Y != targetY) return false;
        var dx = Math.Sign(targetX - enemy.X);
        var dy = Math.Sign(targetY - enemy.Y);
        for (var step = 1; step < distance; step++)
        {
            var x = enemy.X + dx * step;
            var y = enemy.Y + dy * step;
            if (TileAt(x, y) is not (SiegeTile.Floor or SiegeTile.Barrel or SiegeTile.Treasure) ||
                EnemyAt(x, y) is not null || RetainerAt(x, y) is not null || x == PlayerX && y == PlayerY)
                return false;
        }
        return true;
    }

    private void MoveEnemyToward(SiegeEnemy enemy, int targetX, int targetY)
    {
        var destination = CardinalSteps(enemy.X, enemy.Y)
            .OrderBy(point => Distance(point.X, point.Y, targetX, targetY))
            .FirstOrDefault(point => EnemyCanEnter(enemy, point.X, point.Y));
        if (destination == default) return;
        enemy.Facing = DirectionToward(enemy.X, enemy.Y, destination.X, destination.Y, enemy.Facing);
        enemy.X = destination.X;
        enemy.Y = destination.Y;
        enemy.WalkFrame = (enemy.WalkFrame + 1) % 3;
    }

    private bool EnemyCanEnter(SiegeEnemy self, int x, int y) =>
        TileAt(x, y) is SiegeTile.Floor or SiegeTile.Barrel or SiegeTile.Treasure &&
        (x != PlayerX || y != PlayerY) && RetainerAt(x, y) is null &&
        _enemies.All(enemy => ReferenceEquals(enemy, self) || enemy.Health <= 0 ||
            enemy.X != x || enemy.Y != y);

    private SiegeObject? FirstDestructibleAhead(int range)
    {
        var (dx, dy) = Direction(Facing);
        for (var i = 1; i <= range; i++)
        {
            var x = PlayerX + dx * i; var y = PlayerY + dy * i;
            if (TileAt(x, y) == SiegeTile.Destructible) return ObjectAt(x, y);
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Exit)
                return null;
            if (EnemyAt(x, y) is not null) return null;
        }
        return null;
    }

    private bool AdvanceObject(SiegeObject item)
    {
        if (!item.Advance()) return false;
        _map[item.X, item.Y] = item.Tile;
        _movementBlocks[item.X, item.Y] = item.MovementBlocked;
        return true;
    }

    private void RemoveDead()
    {
        foreach (var enemy in _enemies.Where(enemy => enemy.Health <= 0 &&
                     enemy.VisualState != SiegeEnemyVisualState.Dying))
        {
            enemy.Facing = DirectionToward(enemy.X, enemy.Y, PlayerX, PlayerY, enemy.Facing);
            StartVisual(enemy, SiegeEnemyVisualState.Dying);
        }
    }

    private static void StartVisual(SiegeEnemy enemy, SiegeEnemyVisualState state)
    {
        enemy.VisualState = state;
        enemy.VisualElapsed = 0;
    }

    private Point EmptySpawn(int index)
    {
        var candidates = new[] { new Point(9,1), new Point(9,3), new Point(6,5), new Point(2,6), new Point(9,8), new Point(5,9), new Point(2,9), new Point(10,10), new Point(7,10), new Point(10,6), new Point(3,3) };
        return candidates[index % candidates.Length];
    }

    private static SiegeTile[,] GenerateMap(int width, int height)
    {
        var map = new SiegeTile[width, height];
        for (var x = 0; x < width; x++) for (var y = 0; y < height; y++) map[x, y] = x == 0 || y == 0 || x == width - 1 || y == height - 1 ? SiegeTile.Wall : SiegeTile.Floor;
        for (var y = 1; y < 10; y++) if (y != 5) map[4, y] = SiegeTile.Wall;
        map[4, 5] = SiegeTile.Door;
        for (var x = 5; x < 11; x++) if (x != 8) map[x, 7] = SiegeTile.Wall;
        map[8, 7] = SiegeTile.SecretDoor;
        map[2, 3] = SiegeTile.Barrel; map[7, 5] = SiegeTile.Barrel; map[9, 9] = SiegeTile.Barrel;
        map[2, 9] = SiegeTile.Treasure; map[10, 6] = SiegeTile.Treasure; map[7, 9] = SiegeTile.Treasure;
        return map;
    }

    private static (int X, int Y) Direction(Facing facing) => facing switch
    {
        Facing.North => (0, -1), Facing.East => (1, 0), Facing.South => (0, 1), _ => (-1, 0)
    };

    private static Facing DirectionToward(int fromX, int fromY, int toX, int toY, Facing fallback)
    {
        var dx = toX - fromX;
        var dy = toY - fromY;
        if (Math.Abs(dx) > Math.Abs(dy)) return dx > 0 ? Facing.East : Facing.West;
        if (dy != 0) return dy > 0 ? Facing.South : Facing.North;
        return fallback;
    }

    public readonly record struct Point(int X, int Y);
}
