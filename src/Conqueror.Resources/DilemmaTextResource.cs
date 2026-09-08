using System.Text;
using System.Text.RegularExpressions;

namespace Conqueror.Resources;

public enum DilemmaOutcome { Win, Draw, Lose }

public sealed record DilemmaAttributeChange(string Attribute, int Modifier);
public sealed record DilemmaOutcomeText(DilemmaOutcome Outcome, string Text, IReadOnlyList<DilemmaAttributeChange> Changes);
public sealed record DilemmaChoiceText(
    int Number,
    string ScoringAttribute,
    int HighBreakpoint,
    int LowBreakpoint,
    IReadOnlyList<DilemmaOutcomeText> Outcomes);
public sealed record DilemmaTextResource(
    int Number,
    int Age,
    string Title,
    string SceneFile,
    string Prompt,
    IReadOnlyList<DilemmaChoiceText> Choices);

/// <summary>Parses the marker-delimited ASCII DILEM*.DAT resources.</summary>
public static partial class DilemmaTextDecoder
{
    public const int DefaultMaximumSize = 1024 * 1024;

    public static DilemmaTextResource Decode(ReadOnlySpan<byte> source, int maximumSize = DefaultMaximumSize)
    {
        if (maximumSize < 0) throw new ArgumentOutOfRangeException(nameof(maximumSize));
        if (source.Length == 0 || source.Length > maximumSize)
            throw new InvalidDataException("Dilemma text resource has an invalid size.");
        for (var index = 0; index < source.Length; index++)
        {
            var value = source[index];
            if (value > 0x7f || (value == 0x1a && index != source.Length - 1) || value == 0)
                throw new InvalidDataException("Dilemma text resource is not bounded ASCII text.");
        }

        var length = source[^1] == 0x1a ? source.Length - 1 : source.Length;
        var lines = Encoding.ASCII.GetString(source[..length]).Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
        var identityHeader = Find(lines, 0, lines.Length, line => line.TrimStart().StartsWith('!'));
        var age = ReadHeaderInteger(lines, identityHeader, AgeHeader(), "age");
        var title = ReadHeaderText(lines, identityHeader, TitleHeader(), "title");
        var identity = NextData(lines, identityHeader + 1, lines.Length);
        var identityMatch = IdentityLine().Match(lines[identity]);
        if (!identityMatch.Success) throw new InvalidDataException("Dilemma identity row is malformed.");
        var number = ParseInt(identityMatch.Groups[1].Value, "dilemma number");
        var sceneFile = identityMatch.Groups[2].Value;

        var promptHeader = Find(lines, identity + 1, lines.Length, line => line.TrimStart().StartsWith('&'));
        var firstChoice = Find(lines, promptHeader + 1, lines.Length, line => line.TrimStart().StartsWith('@'));
        var prompt = ReadText(lines, promptHeader + 1, firstChoice);
        if (prompt.Length == 0) throw new InvalidDataException("Dilemma prompt is empty.");

        var choices = new List<DilemmaChoiceText>();
        var cursor = firstChoice;
        while (cursor < lines.Length)
        {
            var nextChoice = FindOrEnd(lines, cursor + 1, lines.Length, line => line.TrimStart().StartsWith('@'));
            choices.Add(ReadChoice(lines, cursor, nextChoice));
            cursor = nextChoice;
        }
        if (choices.Count != 3 || choices.Select(choice => choice.Number).Distinct().Count() != choices.Count)
            throw new InvalidDataException("Dilemma resource must contain three uniquely numbered choices.");
        return new DilemmaTextResource(number, age, title, sceneFile, prompt, choices.OrderBy(choice => choice.Number).ToArray());
    }

    private static DilemmaChoiceText ReadChoice(string[] lines, int start, int end)
    {
        var attributeLine = NextData(lines, start + 1, end);
        var attribute = lines[attributeLine].Trim();
        if (!attribute.StartsWith('~') || attribute.Length == 1)
            throw new InvalidDataException("Dilemma choice scoring attribute is malformed.");
        attribute = attribute[1..].Trim();

        var breakpointHeader = Find(lines, attributeLine + 1, end, line => line.TrimStart().StartsWith('%'));
        var breakpointLine = NextData(lines, breakpointHeader + 1, end);
        var breakpointMatch = TwoIntegers().Match(lines[breakpointLine]);
        if (!breakpointMatch.Success) throw new InvalidDataException("Dilemma breakpoints are malformed.");
        var high = ParseInt(breakpointMatch.Groups[1].Value, "high breakpoint");
        var low = ParseInt(breakpointMatch.Groups[2].Value, "low breakpoint");

        var outcomes = new List<DilemmaOutcomeText>();
        var cursor = breakpointLine + 1;
        var choiceNumber = -1;
        while (cursor < end)
        {
            var outcomeHeader = FindOrEnd(lines, cursor, end, line => line.TrimStart().StartsWith('?'));
            if (outcomeHeader == end) break;
            var match = OutcomeHeader().Match(lines[outcomeHeader]);
            if (!match.Success) throw new InvalidDataException("Dilemma outcome header is malformed.");
            var parsedChoice = ParseInt(match.Groups[1].Value, "choice number");
            if (choiceNumber < 0) choiceNumber = parsedChoice;
            if (parsedChoice != choiceNumber) throw new InvalidDataException("Dilemma outcome choice numbers disagree.");
            var outcome = Enum.Parse<DilemmaOutcome>(match.Groups[2].Value, true);

            var changesHeader = Find(lines, outcomeHeader + 1, end, line => line.TrimStart().StartsWith('*'));
            var text = ReadText(lines, outcomeHeader + 1, changesHeader);
            if (text.Length == 0) throw new InvalidDataException("Dilemma outcome text is empty.");
            var countLine = NextData(lines, changesHeader + 1, end);
            var count = ParseInt(lines[countLine].Trim(), "attribute-change count");
            if (count < 0 || count > 64) throw new InvalidDataException("Dilemma attribute-change count is outside its limit.");
            var tableHeader = Find(lines, countLine + 1, end, line => line.TrimStart().StartsWith('$'));
            var changes = new List<DilemmaAttributeChange>(count);
            cursor = tableHeader + 1;
            while (changes.Count < count)
            {
                cursor = NextData(lines, cursor, end);
                var change = AttributeChangeLine().Match(lines[cursor]);
                if (!change.Success) throw new InvalidDataException("Dilemma attribute-change row is malformed.");
                changes.Add(new DilemmaAttributeChange(change.Groups[1].Value, ParseInt(change.Groups[2].Value, "attribute modifier")));
                cursor++;
            }
            outcomes.Add(new DilemmaOutcomeText(outcome, text, changes));
        }

        if (choiceNumber < 1 || outcomes.Count != 3 || outcomes.Select(item => item.Outcome).Distinct().Count() != 3)
            throw new InvalidDataException("Dilemma choice must define win, draw, and lose outcomes.");
        return new DilemmaChoiceText(choiceNumber, attribute, high, low, outcomes);
    }

    private static string ReadText(string[] lines, int start, int end) => string.Join(' ', lines[start..end]
        .Select(line => line.Trim())
        .Where(line => line.StartsWith('^'))
        .Select(line => line[1..].Trim()));

    private static int NextData(string[] lines, int start, int end)
    {
        for (var index = start; index < end; index++)
        {
            var line = lines[index].Trim();
            if (line.Length > 0 && !line.StartsWith('#')) return index;
        }
        throw new InvalidDataException("Dilemma resource ends before an expected data row.");
    }

    private static int Find(string[] lines, int start, int end, Func<string, bool> predicate)
    {
        var result = FindOrEnd(lines, start, end, predicate);
        if (result == end) throw new InvalidDataException("Dilemma resource is missing a required marker.");
        return result;
    }

    private static int FindOrEnd(string[] lines, int start, int end, Func<string, bool> predicate)
    {
        for (var index = start; index < end; index++) if (predicate(lines[index])) return index;
        return end;
    }

    private static int ParseInt(string value, string field) => int.TryParse(value, out var result)
        ? result
        : throw new InvalidDataException($"Dilemma {field} is not an integer.");

    private static int ReadHeaderInteger(string[] lines, int end, Regex pattern, string field)
    {
        for (var index = 0; index < end; index++)
        {
            var match = pattern.Match(lines[index]);
            if (match.Success) return ParseInt(match.Groups[1].Value, field);
        }
        throw new InvalidDataException($"Dilemma resource is missing its {field} header.");
    }

    private static string ReadHeaderText(string[] lines, int end, Regex pattern, string field)
    {
        for (var index = 0; index < end; index++)
        {
            var match = pattern.Match(lines[index]);
            if (match.Success && match.Groups[1].Value.Trim() is { Length: > 0 } value) return value;
        }
        throw new InvalidDataException($"Dilemma resource is missing its {field} header.");
    }

    [GeneratedRegex(@"^\s*(\d+)\s+(\S+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex IdentityLine();

    [GeneratedRegex(@"^\s*\?\s*DILEMMA\s+CHOICE\s+(\d+)\s+(WIN|DRAW|LOSE)\s+TEXT\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OutcomeHeader();

    [GeneratedRegex(@"^\s*(-?\d+)\s+(-?\d+)\s*$", RegexOptions.CultureInvariant)]
    private static partial Regex TwoIntegers();

    [GeneratedRegex(@"^\s*([A-Z][A-Z0-9_]*)\s+(-?\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AttributeChangeLine();

    [GeneratedRegex(@"^#\s*AGE:?\s*(\d+)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex AgeHeader();

    [GeneratedRegex(@"^#\s*TITLE:?\s*(.+?)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TitleHeader();
}
