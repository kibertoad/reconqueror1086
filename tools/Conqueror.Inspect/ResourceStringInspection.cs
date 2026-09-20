using System.Text;

static class ResourceStringInspection
{
    public static IEnumerable<(long Offset, string Value)> PrintableStrings(string path)
    {
        using var stream = File.OpenRead(path);
        var bytes = new List<byte>();
        long start = 0;
        for (long offset = 0; offset < stream.Length; offset++)
        {
            var value = stream.ReadByte();
            if (value is >= 32 and <= 126)
            {
                if (bytes.Count == 0) start = offset;
                bytes.Add((byte)value);
            }
            else
            {
                if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
                bytes.Clear();
            }
        }
        if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
    }

    public static IEnumerable<(long Offset, string Value)> PrintableStrings(byte[] source)
    {
        var bytes = new List<byte>();
        long start = 0;
        for (var offset = 0; offset < source.Length; offset++)
        {
            var value = source[offset];
            if (value is >= 32 and <= 126)
            {
                if (bytes.Count == 0) start = offset;
                bytes.Add(value);
                continue;
            }
            if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
            bytes.Clear();
        }
        if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
    }
}
