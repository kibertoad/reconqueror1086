namespace Conqueror.Core;

public sealed record WorldLocation(string Name, LocationKind Kind, int X, int Y, int Garrison, int Villages = 0);

public static class World
{
    public static readonly WorldLocation[] Locations =
    [
        new("Oxford", LocationKind.Home, 370, 430, 0, 1),
        new("London", LocationKind.London, 535, 470, 35, 3),
        new("Canterbury", LocationKind.Castle, 650, 510, 12, 2),
        new("Winchester", LocationKind.Castle, 330, 535, 10, 2),
        new("Bristol", LocationKind.Castle, 195, 485, 12, 2),
        new("Exeter", LocationKind.Castle, 125, 610, 9, 1),
        new("Cambridge", LocationKind.Castle, 535, 365, 13, 2),
        new("Norwich", LocationKind.Castle, 665, 315, 14, 2),
        new("Leicester", LocationKind.Castle, 385, 315, 15, 2),
        new("Nottingham", LocationKind.Castle, 390, 255, 16, 2),
        new("Shrewsbury", LocationKind.Castle, 215, 320, 14, 2),
        new("Chester", LocationKind.Castle, 205, 230, 17, 3),
        new("Lincoln", LocationKind.Castle, 475, 230, 18, 3),
        new("York", LocationKind.Castle, 405, 145, 22, 3),
        new("Lancaster", LocationKind.Castle, 245, 125, 20, 2),
        new("Durham", LocationKind.Castle, 390, 75, 24, 2),
        new("Carlisle", LocationKind.Castle, 220, 55, 23, 2),
        new("Dragon Moor", LocationKind.DragonLair, 590, 95, 0)
    ];

    // PLACEHOLDER: RULE-TOURNEY-001. The original draws a new site each month; this fixed circuit is a guess.
    public static int TournamentIndex(DateTime date)
    {
        int[] circuit = [4, 8, 12, 13, 6, 3, 11, 7];
        return circuit[(date.Year * 12 + date.Month) % circuit.Length];
    }

    public static int TravelDays(int from, int to)
    {
        if (from == to) return 0;
        var a = Locations[Math.Clamp(from, 0, Locations.Length - 1)];
        var b = Locations[Math.Clamp(to, 0, Locations.Length - 1)];
        return Math.Max(1, (int)Math.Ceiling(Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2)) / 22d));
    }
}

public sealed record Dilemma(string Title, string Prompt, DilemmaChoice[] Choices);
public sealed record DilemmaChoice(string Text, CharacterStats Delta, int Wealth = 0, string? Item = null);

public static class Youth
{
    public static readonly YouthDilemmaPoolDefinition OriginalPool = new(12, 17, 5);

    public static readonly Dilemma[] Dilemmas =
    [
        new("A false account", "A lord lies about his income. What do you do?",
        [
            new("Tell your overlord", new(0,0,1,0,1)), new("Blackmail the lord", new(0,0,-1,0,0), 20), new("Say nothing", new(0,0,0,0,0))
        ]),
        new("The dying swordsman", "A dying man asks you to return his blade.",
        [
            new("Return it", new(0,0,1,1,1), 0, "Stiletto Dagger"), new("Take the sword", new(0,0,-1,0,-1)), new("Leave it", new(0,0,0,0,0))
        ]),
        new("The wild boar", "A boar charges from the brush.",
        [
            new("Attack it", new(1,0,0,0,1)), new("Rope it", new(0,2,0,0,2)), new("Run", new(0,0,0,0,0))
        ]),
        new("The bear", "A bear threatens travelers on the road.",
        [
            new("Fight it", new(2,0,1,1,2)), new("Scare it away", new(0,0,1,0,1)), new("Cower", new(0,0,0,0,0))
        ]),
        new("The hidden chalice", "You find a chalice hidden in the chapel.",
        [
            new("Keep it", new(0,0,-2,0,0)), new("Sell it", new(0,0,-2,0,0), 10), new("Return it", new(0,0,1,0,1))
        ]),
        new("The captured lord", "A captive lord begs you to free him.",
        [
            new("Free him", new(2,1,1,0,0), 0, "Thruster's Dagger"), new("Go back to sleep", new(0,0,-1,0,0)), new("Tell the authorities", new(0,0,0,0,0))
        ])
    ];
}
