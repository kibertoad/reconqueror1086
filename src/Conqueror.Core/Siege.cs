namespace Conqueror.Core;

public enum Facing { North, East, South, West }
public enum SiegeTile { Floor, Wall, Door, SecretDoor, OpeningDoor, Barrel, Treasure }
public enum SiegeAction { None, Moved, Blocked, DoorOpened, Healed, Looted, Hit, Missed, WeaponBroke, Shot, NoAmmunition }
public enum SiegeEnemyVisualState { Walk, Attack, Hit, Dying }

public sealed class SiegeEnemy
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Health { get; set; }
    public bool Champion { get; init; }
    public int VisualId { get; init; } = -1;
    public Facing Facing { get; set; }
    public int WalkFrame { get; set; }
    public SiegeEnemyVisualState VisualState { get; internal set; }
    public int VisualFrame { get; internal set; }
    internal double VisualElapsed { get; set; }
}

public sealed record SiegeDefinition(int Width, int Height, int BaseEnemies, int GarrisonPerEnemy, int BaseChampionHealth, int FoodHealing, int WeaponBreakPercent);
public sealed record SiegeSpawn(int X, int Y, bool Champion, int VisualId = -1);

public sealed class SiegeLayout
{
    private readonly SiegeTile[,] _tiles;

    public SiegeLayout(SiegeTile[,] tiles, int playerX, int playerY, Facing facing, IReadOnlyList<SiegeSpawn> enemies)
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

        _tiles = (SiegeTile[,])tiles.Clone();
        PlayerX = playerX;
        PlayerY = playerY;
        Facing = facing;
        Enemies = enemies.ToArray();
    }

    public int PlayerX { get; }
    public int PlayerY { get; }
    public Facing Facing { get; }
    public IReadOnlyList<SiegeSpawn> Enemies { get; }
    public SiegeTile[,] CopyTiles() => (SiegeTile[,])_tiles.Clone();
}

public sealed class SiegeSession
{
    public static readonly SiegeDefinition Rules = new(12, 12, 5, 3, 3, 25, 2);
    public const double DoorOpeningSeconds = 0.36;
    public const double EnemyAttackFrameSeconds = 0.07;
    public const double EnemyHitSeconds = 0.20;
    public const double EnemyDeathFrameSeconds = 0.09;
    private readonly Player _player;
    private readonly Random _random;
    private readonly SiegeTile[,] _map;
    private readonly Dictionary<(int X, int Y), double> _openingDoors = [];
    public IReadOnlyList<SiegeEnemy> Enemies => _enemies;
    private readonly List<SiegeEnemy> _enemies = [];

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
        if (layout is not null)
        {
            PlayerX = layout.PlayerX;
            PlayerY = layout.PlayerY;
            Facing = layout.Facing;
        }
        MaxHealth = 80 + player.Stats.Stamina * 5;
        Health = MaxHealth;
        AlliesStarted = includeRetainers ? Math.Clamp(1 + army.Total / 10, 1, 5) : 0;
        AlliesAlive = AlliesStarted;
        if (layout is not null)
        {
            foreach (var spawn in layout.Enemies)
            {
                var enemy = new SiegeEnemy
                {
                    X = spawn.X,
                    Y = spawn.Y,
                    Health = spawn.Champion ? Rules.BaseChampionHealth : 1,
                    Champion = spawn.Champion,
                    VisualId = spawn.VisualId
                };
                enemy.Facing = DirectionToward(enemy.X, enemy.Y, PlayerX, PlayerY, Facing.South);
                _enemies.Add(enemy);
            }
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

    public SiegeTile TileAt(int x, int y) => x < 0 || y < 0 || x >= Width || y >= Height ? SiegeTile.Wall : _map[x, y];
    public SiegeEnemy? EnemyAt(int x, int y) => _enemies.FirstOrDefault(e => e.Health > 0 && e.X == x && e.Y == y);

    public double? DoorOpeningProgress(int x, int y) => _openingDoors.TryGetValue((x, y), out var elapsed)
        ? Math.Clamp(elapsed / DoorOpeningSeconds, 0, 1)
        : null;

    public void AdvanceDoorAnimations(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        foreach (var point in _openingDoors.Keys.ToArray())
        {
            var elapsed = _openingDoors[point] + elapsedSeconds;
            if (elapsed < DoorOpeningSeconds)
                _openingDoors[point] = elapsed;
            else
            {
                _openingDoors.Remove(point);
                _map[point.X, point.Y] = SiegeTile.Floor;
            }
        }
    }

    public void AdvanceEnemyAnimations(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        foreach (var enemy in _enemies.Where(enemy => enemy.VisualState != SiegeEnemyVisualState.Walk).ToArray())
        {
            enemy.VisualElapsed += elapsedSeconds;
            if (enemy.VisualState == SiegeEnemyVisualState.Attack)
            {
                enemy.VisualFrame = (int)(enemy.VisualElapsed / EnemyAttackFrameSeconds);
                if (enemy.VisualFrame < 9) continue;
            }
            else if (enemy.VisualState == SiegeEnemyVisualState.Dying)
            {
                enemy.VisualFrame = (int)(enemy.VisualElapsed / EnemyDeathFrameSeconds);
                if (enemy.VisualFrame < 8) continue;
                _enemies.Remove(enemy);
                continue;
            }
            else if (enemy.VisualElapsed < EnemyHitSeconds)
                continue;
            enemy.VisualState = SiegeEnemyVisualState.Walk;
            enemy.VisualFrame = 0;
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
        if (tile is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.OpeningDoor || EnemyAt(nx, ny) is not null)
        {
            LastMessage = "The way is blocked."; TickEnemies(); return SiegeAction.Blocked;
        }
        PlayerX = nx; PlayerY = ny;
        var result = CollectTile();
        TickEnemies();
        return result == SiegeAction.None ? SiegeAction.Moved : result;
    }

    public SiegeAction Interact()
    {
        var (dx, dy) = Direction(Facing); var x = PlayerX + dx; var y = PlayerY + dy;
        if (TileAt(x, y) is SiegeTile.Door or SiegeTile.SecretDoor)
        {
            _map[x, y] = SiegeTile.OpeningDoor;
            _openingDoors[(x, y)] = 0;
            LastMessage = "The door opens."; TickEnemies(); return SiegeAction.DoorOpened;
        }
        LastMessage = "Nothing happens."; TickEnemies(); return SiegeAction.None;
    }

    public SiegeAction Attack()
    {
        var weapon = Balance.Equipment.FirstOrDefault(x => x.Name.Equals(_player.Inventory.Weapon, StringComparison.OrdinalIgnoreCase));
        var reach = weapon is null ? 1 : Math.Clamp(weapon.Power / 70, 1, 2);
        var target = FirstEnemyAhead(reach);
        if (target is null) { LastMessage = "Your blow meets empty air."; TickEnemies(); return SiegeAction.Missed; }
        var chance = Math.Clamp((weapon?.Power ?? 35) + _player.Stats.Dexterity * 3 - (target.Champion ? 35 : 0), 15, 210);
        if (_random.Next(220) < chance)
        {
            target.Health -= 1 + _player.Stats.Strength / 16;
            if (target.Health > 0) StartVisual(target, SiegeEnemyVisualState.Hit);
            LastMessage = target.Champion ? "You strike the castle champion." : "Your weapon finds its mark.";
            RemoveDead();
        }
        else LastMessage = "The enemy turns your blow.";
        var broke = weapon is not null && weapon.BuyPrice > 0 && _random.Next(100) < Rules.WeaponBreakPercent;
        if (broke)
        {
            _player.Inventory.Items.Remove(weapon!.Name);
            _player.Inventory.Unequip(EquipmentSlot.Weapon);
            LastMessage = $"Your {weapon.Name} breaks!";
        }
        TickEnemies();
        return broke ? SiegeAction.WeaponBroke : target.Health <= 0 ? SiegeAction.Hit : SiegeAction.Hit;
    }

    public SiegeAction Shoot()
    {
        if (_player.Inventory.CrossbowBolts <= 0) { LastMessage = "You have no crossbow bolts."; return SiegeAction.NoAmmunition; }
        if (!_player.Inventory.Weapon.Contains("Crossbow", StringComparison.OrdinalIgnoreCase)) { LastMessage = "Equip a crossbow first."; return SiegeAction.NoAmmunition; }
        _player.Inventory.CrossbowBolts--;
        var target = FirstEnemyAhead(8);
        if (target is not null)
        {
            target.Health -= 2;
            if (target.Health > 0) StartVisual(target, SiegeEnemyVisualState.Hit);
            RemoveDead(); LastMessage = "The bolt strikes true.";
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
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.OpeningDoor) return 0;
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

    private SiegeAction CollectTile()
    {
        var tile = TileAt(PlayerX, PlayerY);
        if (tile == SiegeTile.Barrel)
        {
            Health = Math.Min(MaxHealth, Health + Rules.FoodHealing); _map[PlayerX, PlayerY] = SiegeTile.Floor;
            LastMessage = $"Food restores {Rules.FoodHealing} health."; return SiegeAction.Healed;
        }
        if (tile == SiegeTile.Treasure)
        {
            _map[PlayerX, PlayerY] = SiegeTile.Floor;
            var gold = _random.Next(20, 81); GoldFound += gold; _player.Wealth += gold;
            if (_random.Next(2) == 0)
            {
                var loot = Balance.Equipment.Where(x => !x.Shop || x.BuyPrice is > 0 and <= 1800).ElementAt(_random.Next(Balance.Equipment.Count(x => !x.Shop || x.BuyPrice is > 0 and <= 1800)));
                if (_player.Inventory.Items.Add(loot.Name)) ItemsFound.Add(loot.Name);
            }
            var bolts = _random.Next(2, 7); _player.Inventory.CrossbowBolts += bolts;
            LastMessage = $"Found {gold}s and {bolts} crossbow bolts."; return SiegeAction.Looted;
        }
        return SiegeAction.None;
    }

    private void TickEnemies()
    {
        if (Defeated || !_enemies.Any(enemy => enemy.Health > 0)) return;
        foreach (var enemy in _enemies.Where(enemy => enemy.Health > 0).ToArray())
        {
            var distance = Math.Abs(enemy.X - PlayerX) + Math.Abs(enemy.Y - PlayerY);
            if (distance == 1)
            {
                enemy.Facing = DirectionToward(enemy.X, enemy.Y, PlayerX, PlayerY, enemy.Facing);
                enemy.WalkFrame = 0;
                if (enemy.VisualState != SiegeEnemyVisualState.Hit)
                    StartVisual(enemy, SiegeEnemyVisualState.Attack);
                if (AlliesAlive > 0 && _random.Next(100) < 18) { AlliesAlive--; LastMessage = "A retainer falls defending you."; continue; }
                var hitChance = Math.Clamp(70 - ArmorRating(), 5, 70);
                if (_random.Next(100) < hitChance) Health -= Math.Max(2, 12 - _player.Stats.Stamina / 3);
                continue;
            }
            if (distance > 6 || _random.Next(100) >= 55) continue;
            var dx = Math.Sign(PlayerX - enemy.X); var dy = Math.Sign(PlayerY - enemy.Y);
            if (Math.Abs(PlayerX - enemy.X) < Math.Abs(PlayerY - enemy.Y)) dx = 0; else dy = 0;
            var nx = enemy.X + dx; var ny = enemy.Y + dy;
            if (TileAt(nx, ny) is SiegeTile.Floor or SiegeTile.Barrel or SiegeTile.Treasure && EnemyAt(nx, ny) is null && (nx != PlayerX || ny != PlayerY))
            {
                enemy.Facing = DirectionToward(enemy.X, enemy.Y, nx, ny, enemy.Facing);
                enemy.X = nx; enemy.Y = ny;
                enemy.WalkFrame = (enemy.WalkFrame + 1) % 3;
            }
        }
        var vulnerable = _enemies.Where(enemy => enemy.Health > 0 && !enemy.Champion).ToArray();
        if (AlliesAlive > 0 && vulnerable.Length > 0 && _random.Next(100) < AlliesAlive * 7)
        {
            var victim = vulnerable[0]; victim.Health--; RemoveDead(); LastMessage = "Your retainers bring down a defender.";
        }
        Health = Math.Max(0, Health);
    }

    private SiegeEnemy? FirstEnemyAhead(int range)
    {
        var (dx, dy) = Direction(Facing);
        for (var i = 1; i <= range; i++)
        {
            var x = PlayerX + dx * i; var y = PlayerY + dy * i;
            if (TileAt(x, y) is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.OpeningDoor) return null;
            if (EnemyAt(x, y) is { } enemy) return enemy;
        }
        return null;
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
        enemy.VisualFrame = 0;
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
