namespace Conqueror.Core;

public enum CharacterAttribute
{
    None,
    Strength,
    Dexterity,
    Intelligence,
    Piety,
    Stamina,
    Honor,
    SwordExperience,
    Fame,
    Age,
    Wealth
}

public enum YouthDilemmaOutcome { Win, Draw, Lose }

public sealed record CharacterAttributeChange(CharacterAttribute Attribute, int Modifier);

public sealed record YouthDilemmaOutcomeDefinition(
    string Text,
    IReadOnlyList<CharacterAttributeChange> Changes);

public sealed record YouthDilemmaChoiceDefinition(
    string Text,
    CharacterAttribute ScoringAttribute,
    int LowBreakpoint,
    int HighBreakpoint,
    IReadOnlyDictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition> Outcomes);

public sealed record YouthDilemmaDefinition(
    int Number,
    int Age,
    string Title,
    string Prompt,
    IReadOnlyList<YouthDilemmaChoiceDefinition> Choices,
    string? SceneFile = null);

public sealed record YouthDilemmaResult(
    int DilemmaNumber,
    int ChoiceNumber,
    YouthDilemmaOutcome Outcome,
    string Text,
    IReadOnlyList<CharacterAttributeChange> Changes);

public sealed record YouthDilemmaPoolDefinition(int FirstAge, int LastAge, int VariantsPerAge)
{
    public int StageCount => checked(LastAge - FirstAge + 1);

    public int AgeAtStage(int stage) => stage >= 0 && stage < StageCount
        ? FirstAge + stage
        : throw new ArgumentOutOfRangeException(nameof(stage));

    public int NumberFor(int age, int variant) => age >= FirstAge && age <= LastAge
        && variant >= 0 && variant < VariantsPerAge
            ? checked((age - FirstAge) * VariantsPerAge + variant)
            : throw new ArgumentOutOfRangeException(age < FirstAge || age > LastAge ? nameof(age) : nameof(variant));

    public bool Contains(int number, int age) => number >= 0
        && number / VariantsPerAge == age - FirstAge
        && age >= FirstAge
        && age <= LastAge;
}

public static class YouthDilemmaRules
{
    public static YouthDilemmaOutcome Resolve(YouthDilemmaChoiceDefinition choice, int score)
    {
        if (choice.LowBreakpoint > choice.HighBreakpoint)
            throw new ArgumentException("A dilemma's low breakpoint cannot exceed its high breakpoint.", nameof(choice));

        return score >= choice.HighBreakpoint
            ? YouthDilemmaOutcome.Win
            : score >= choice.LowBreakpoint ? YouthDilemmaOutcome.Draw : YouthDilemmaOutcome.Lose;
    }
}

public static class CharacterAttributes
{
    public static int Read(Player player, CharacterAttribute attribute) => attribute switch
    {
        CharacterAttribute.None => 0,
        CharacterAttribute.Strength => player.Stats.Strength,
        CharacterAttribute.Dexterity => player.Stats.Dexterity,
        CharacterAttribute.Intelligence => player.Stats.Intelligence,
        CharacterAttribute.Piety => player.Stats.Piety,
        CharacterAttribute.Stamina => player.Stats.Stamina,
        CharacterAttribute.Honor => player.Stats.Honor,
        CharacterAttribute.SwordExperience => player.SwordExperience,
        CharacterAttribute.Fame => player.Fame,
        CharacterAttribute.Age => player.Age,
        CharacterAttribute.Wealth => player.Wealth,
        _ => throw new ArgumentOutOfRangeException(nameof(attribute))
    };

    public static void Apply(Player player, IEnumerable<CharacterAttributeChange> changes)
    {
        foreach (var change in changes) Apply(player, change);
        player.Stats = player.Stats.Clamp();
    }

    private static void Apply(Player player, CharacterAttributeChange change)
    {
        var value = Read(player, change.Attribute) + change.Modifier;
        switch (change.Attribute)
        {
            case CharacterAttribute.None: break;
            case CharacterAttribute.Strength: player.Stats = player.Stats with { Strength = value }; break;
            case CharacterAttribute.Dexterity: player.Stats = player.Stats with { Dexterity = value }; break;
            case CharacterAttribute.Intelligence: player.Stats = player.Stats with { Intelligence = value }; break;
            case CharacterAttribute.Piety: player.Stats = player.Stats with { Piety = value }; break;
            case CharacterAttribute.Stamina: player.Stats = player.Stats with { Stamina = value }; break;
            case CharacterAttribute.Honor: player.Stats = player.Stats with { Honor = value }; break;
            case CharacterAttribute.SwordExperience: player.SwordExperience = Math.Max(0, value); break;
            case CharacterAttribute.Fame: player.Fame = Math.Max(0, value); break;
            case CharacterAttribute.Age: player.Age = Math.Max(0, value); break;
            case CharacterAttribute.Wealth: player.Wealth = value; break;
            default: throw new ArgumentOutOfRangeException(nameof(change));
        }
    }
}
