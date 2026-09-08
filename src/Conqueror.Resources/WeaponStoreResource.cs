using System.Globalization;
using System.Text;

namespace Conqueror.Resources;

public sealed record WeaponStoreEntry(
    int RecordIndex,
    string MovieFile,
    int UnknownValue,
    int ImageFrame,
    int ItemId,
    int Price,
    string Description)
{
    public bool HasMovie => MovieFile != "#";
}

public sealed record WeaponStoreResource(IReadOnlyList<WeaponStoreEntry> Entries);

public static class WeaponStoreDecoder
{
    private const int FieldsPerRecord = 6;
    private const int MaximumRecords = 256;
    private const int MaximumResourceBytes = 1024 * 1024;
    private const int MaximumDescriptionLength = 4096;

    public static WeaponStoreResource Decode(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty || data.Length > MaximumResourceBytes)
            throw new InvalidDataException("Weapon-store data has an invalid length.");
        foreach (var value in data)
            if (value is not (0x0D or 0x0A) && (value < 0x20 || value > 0x7E))
                throw new InvalidDataException("Weapon-store data must contain bounded printable ASCII lines.");

        var text = Encoding.ASCII.GetString(data);
        if (text.Replace("\r\n", string.Empty, StringComparison.Ordinal).Contains('\r')
            || text.Replace("\r\n", string.Empty, StringComparison.Ordinal).Contains('\n'))
            throw new InvalidDataException("Weapon-store data must use CRLF line endings.");

        var lines = text.Split("\r\n", StringSplitOptions.None);
        if (lines.Length > 0 && lines[^1].Length == 0) lines = lines[..^1];
        if (lines.Length == 0 || lines.Length % FieldsPerRecord != 0 || lines.Length / FieldsPerRecord > MaximumRecords)
            throw new InvalidDataException("Weapon-store data does not contain complete six-line records.");

        var entries = new WeaponStoreEntry[lines.Length / FieldsPerRecord];
        for (var index = 0; index < entries.Length; index++)
        {
            var offset = index * FieldsPerRecord;
            var movie = lines[offset];
            var description = lines[offset + 5];
            if (movie.Length is 0 or > 64 || description.Length is 0 or > MaximumDescriptionLength)
                throw new InvalidDataException($"Weapon-store record {index} contains an invalid text field.");
            entries[index] = new WeaponStoreEntry(
                index,
                movie,
                ParseNonNegative(lines[offset + 1], index, "unknown value"),
                ParseNonNegative(lines[offset + 2], index, "image frame"),
                ParseNonNegative(lines[offset + 3], index, "item identifier"),
                ParseNonNegative(lines[offset + 4], index, "price"),
                description);
        }

        return new WeaponStoreResource(entries);
    }

    private static int ParseNonNegative(string text, int record, string field)
    {
        if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value < 0)
            throw new InvalidDataException($"Weapon-store record {record} has an invalid {field}.");
        return value;
    }
}
