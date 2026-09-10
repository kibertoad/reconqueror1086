namespace Conqueror.Game;

public sealed record TournamentConversationDefinition(string Lady, int RootNodeId, string PortraitSuffix);

public static class TournamentConversationDefinitions
{
    public static IReadOnlyList<TournamentConversationDefinition> Ladies { get; } =
    [
        new("Adela", 2200, ":adela.pcc"),
        new("Jane", 2900, ":jane.pcc"),
        new("Anna Lisa", 2400, ":annalisa.pcc"),
        new("Victoria", 2100, ":victoria.pcc"),
        new("Wendessa", 2000, ":wendessa.pcc"),
        new("Valetta", 2500, ":valletta.pcc")
    ];

    public static TournamentConversationDefinition For(string lady) =>
        Ladies.Single(definition => definition.Lady.Equals(lady, StringComparison.OrdinalIgnoreCase));
}
