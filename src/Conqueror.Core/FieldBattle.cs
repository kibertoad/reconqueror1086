namespace Conqueror.Core;

public enum UnitOrder { Hold, Advance, FlankLeft, FlankRight, Withdraw, Captains, MoveTo }
public enum FieldBattleOutcome { InProgress, Victory, Defeat, Withdrawn }
public sealed record FieldBattleDefinition(int Width, int Height, double CounterBonus, int BaseMorale, int WithdrawMoraleLoss);

public sealed class BattleSquad
{
    public UnitType Type { get; init; }
    public bool Friendly { get; init; }
    public int Count { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Morale { get; set; }
    public UnitOrder Order { get; set; }
    public int? DestinationX { get; set; }
    public int? DestinationY { get; set; }
}

public sealed class FieldBattleSession
{
    public static readonly FieldBattleDefinition Rules = new(14, 9, .75, 100, 20);
    private readonly Random _random;
    public List<BattleSquad> Squads { get; } = [];
    public FieldBattleOutcome Outcome { get; private set; }
    public int TickNumber { get; private set; }
    public string LastMessage { get; private set; } = "Issue orders. The battle is joined.";
    public IEnumerable<BattleSquad> Friendly => Squads.Where(x => x.Friendly && x.Count > 0);
    public IEnumerable<BattleSquad> Enemy => Squads.Where(x => !x.Friendly && x.Count > 0);

    public FieldBattleSession(Army friendly, Army enemy, int seed)
    {
        _random = new Random(seed);
        var lanes = new Dictionary<UnitType, int> { [UnitType.Swordsmen] = 2, [UnitType.Halberdiers] = 4, [UnitType.Knights] = 6 };
        foreach (var type in Enum.GetValues<UnitType>())
        {
            if (friendly.Units[type] > 0) Squads.Add(new BattleSquad { Type = type, Friendly = true, Count = friendly.Units[type], X = 1, Y = lanes[type], Morale = Rules.BaseMorale, Order = UnitOrder.Hold });
            if (enemy.Units[type] > 0) Squads.Add(new BattleSquad { Type = type, Friendly = false, Count = enemy.Units[type], X = Rules.Width - 2, Y = lanes[type], Morale = Rules.BaseMorale, Order = UnitOrder.Captains });
        }
    }

    public void Issue(UnitType type, UnitOrder order)
    {
        if (order == UnitOrder.MoveTo) throw new ArgumentException("Use IssueDestination for a map point.", nameof(order));
        var squad = Friendly.FirstOrDefault(x => x.Type == type);
        if (squad is null || Outcome != FieldBattleOutcome.InProgress) return;
        squad.Order = order;
        squad.DestinationX = null;
        squad.DestinationY = null;
        if (order == UnitOrder.Withdraw) squad.Morale = Math.Max(0, squad.Morale - Rules.WithdrawMoraleLoss);
        LastMessage = $"{type}: {order}.";
    }

    public bool IssueDestination(UnitType type, int x, int y)
    {
        if (x < 0 || x >= Rules.Width || y < 0 || y >= Rules.Height) return false;
        var squad = Friendly.FirstOrDefault(item => item.Type == type);
        if (squad is null || Outcome != FieldBattleOutcome.InProgress) return false;
        squad.DestinationX = x;
        squad.DestinationY = y;
        squad.Order = UnitOrder.MoveTo;
        LastMessage = $"{type}: move to {x + 1}, {y + 1}.";
        return true;
    }

    public void IssueAll(UnitOrder order)
    {
        foreach (var type in Enum.GetValues<UnitType>()) Issue(type, order);
    }

    public void Tick()
    {
        if (Outcome != FieldBattleOutcome.InProgress) return;
        TickNumber++;
        foreach (var squad in Squads.Where(x => x.Count > 0).ToArray()) MoveSquad(squad);
        ResolveContacts();
        Squads.RemoveAll(x => x.Count <= 0);
        if (!Enemy.Any()) { Outcome = FieldBattleOutcome.Victory; LastMessage = "The enemy army breaks and flees."; }
        else if (!Friendly.Any()) { Outcome = FieldBattleOutcome.Defeat; LastMessage = "Your army has been destroyed."; }
        else if (Friendly.All(x => x.Order == UnitOrder.Withdraw && x.X == 0)) { Outcome = FieldBattleOutcome.Withdrawn; LastMessage = "Your surviving formations leave the field."; }
    }

    public Army FriendlySurvivors()
    {
        var army = new Army();
        foreach (var squad in Friendly) army.Units[squad.Type] += squad.Count;
        return army;
    }

    public Army EnemySurvivors()
    {
        var army = new Army();
        foreach (var squad in Enemy) army.Units[squad.Type] += squad.Count;
        return army;
    }

    private void MoveSquad(BattleSquad squad)
    {
        if (squad.Friendly && squad.Order == UnitOrder.MoveTo)
        {
            AdvanceToDestination(squad);
            return;
        }
        var captainDriven = squad.Order != UnitOrder.Withdraw && (!squad.Friendly || squad.Order == UnitOrder.Captains);
        var target = captainDriven ? ChooseCaptainTarget(squad) : null;
        var order = target is null ? squad.Order : ChooseCaptainOrder(squad, target);
        if (order == UnitOrder.Hold) return;
        if (order == UnitOrder.Withdraw) { squad.X = Math.Clamp(squad.X + (squad.Friendly ? -1 : 1), 0, Rules.Width - 1); return; }
        var direction = target is null ? squad.Friendly ? 1 : -1 : Math.Sign(target.X - squad.X);
        var movedLaterally = false;
        if (order == UnitOrder.FlankLeft && squad.Y > 1) { squad.Y--; movedLaterally = true; }
        else if (order == UnitOrder.FlankRight && squad.Y < Rules.Height - 2) { squad.Y++; movedLaterally = true; }
        if (!movedLaterally || captainDriven)
        {
            var nextX = Math.Clamp(squad.X + direction, 0, Rules.Width - 1);
            if (Squads.Any(x => x.Friendly != squad.Friendly && x.Count > 0 && x.X == nextX && x.Y == squad.Y)) return;
            squad.X = nextX;
        }
    }

    private void AdvanceToDestination(BattleSquad squad)
    {
        if (squad.DestinationX is not { } targetX || squad.DestinationY is not { } targetY)
        {
            squad.Order = UnitOrder.Hold;
            return;
        }
        if (squad.X == targetX && squad.Y == targetY)
        {
            CompleteDestination(squad);
            return;
        }

        var visited = new bool[Rules.Width, Rules.Height];
        var queue = new Queue<(int X, int Y, int FirstX, int FirstY, int Depth)>();
        queue.Enqueue((squad.X, squad.Y, squad.X, squad.Y, 0));
        visited[squad.X, squad.Y] = true;
        var bestDistance = Math.Abs(targetX - squad.X) + Math.Abs(targetY - squad.Y);
        var bestStep = (X: squad.X, Y: squad.Y);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var towardY = Math.Sign(targetY - current.Y);
            var towardX = Math.Sign(targetX - current.X);
            foreach (var (dx, dy) in new[]
                     {
                         (0, towardY), (towardX, 0), (0, -towardY), (-towardX, 0),
                         (0, -1), (0, 1), (-1, 0), (1, 0)
                     })
            {
                if (dx == 0 && dy == 0) continue;
                var nextX = current.X + dx;
                var nextY = current.Y + dy;
                if (nextX < 0 || nextX >= Rules.Width || nextY < 0 || nextY >= Rules.Height
                    || visited[nextX, nextY]
                    || Squads.Any(other => other != squad && other.Count > 0
                        && other.X == nextX && other.Y == nextY)) continue;
                visited[nextX, nextY] = true;
                var firstX = current.Depth == 0 ? nextX : current.FirstX;
                var firstY = current.Depth == 0 ? nextY : current.FirstY;
                var distance = Math.Abs(targetX - nextX) + Math.Abs(targetY - nextY);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestStep = (firstX, firstY);
                }
                if (nextX == targetX && nextY == targetY)
                {
                    squad.X = firstX;
                    squad.Y = firstY;
                    if (squad.X == targetX && squad.Y == targetY) CompleteDestination(squad);
                    return;
                }
                queue.Enqueue((nextX, nextY, firstX, firstY, current.Depth + 1));
            }
        }
        squad.X = bestStep.X;
        squad.Y = bestStep.Y;
    }

    private static void CompleteDestination(BattleSquad squad)
    {
        squad.Order = UnitOrder.Hold;
        squad.DestinationX = null;
        squad.DestinationY = null;
    }

    private BattleSquad? ChooseCaptainTarget(BattleSquad squad)
    {
        var targets = squad.Friendly ? Enemy : Friendly;
        return targets.OrderBy(x => x.Type == Balance.Counter(squad.Type) ? 0 : 1).ThenBy(x => Math.Abs(x.X - squad.X) + Math.Abs(x.Y - squad.Y)).FirstOrDefault();
    }

    private static UnitOrder ChooseCaptainOrder(BattleSquad squad, BattleSquad target)
    {
        if (target.Y < squad.Y) return UnitOrder.FlankLeft;
        if (target.Y > squad.Y) return UnitOrder.FlankRight;
        return UnitOrder.Advance;
    }

    private void ResolveContacts()
    {
        var contacts = Friendly.SelectMany(friendly => Enemy.Where(enemy => Math.Abs(friendly.X - enemy.X) + Math.Abs(friendly.Y - enemy.Y) <= 1).Select(enemy => (friendly, enemy))).ToArray();
        foreach (var (friendly, enemy) in contacts)
        {
            if (friendly.Count <= 0 || enemy.Count <= 0) continue;
            Strike(friendly, enemy); Strike(enemy, friendly);
        }
    }

    private void Strike(BattleSquad attacker, BattleSquad defender)
    {
        var advantage = Balance.Counter(attacker.Type) == defender.Type ? 1 + Rules.CounterBonus : 1;
        var pressure = Math.Min(1.5, attacker.Count / (double)Math.Max(1, defender.Count));
        var chance = Math.Clamp(.20 * advantage * pressure * attacker.Morale / 100d, .04, .80);
        if (_random.NextDouble() >= chance) return;
        defender.Count--;
        defender.Morale = Math.Max(0, defender.Morale - (attacker.Type == UnitType.Knights ? 8 : 5));
        if (defender.Morale == 0)
        {
            defender.Order = UnitOrder.Withdraw;
            defender.DestinationX = null;
            defender.DestinationY = null;
        }
        LastMessage = $"{attacker.Type} strike {defender.Type}.";
    }
}
