using System.Buffers.Binary;
using System.Text;

namespace Conqueror.Resources;

public sealed record HatRegion(int Id, int X, int Y, int Width, int Height, int Enabled);

// FMT-UI-001, FMT-UI-002. The original counts regions from the file size and ignores the header count.
/// <summary>Bounded parser for the original fixed-header screen-layout descriptors.</summary>
public sealed class HatLayout
{
    private const int HeaderSize = 40;
    private const int RegionSize = 24;

    public int ScreenId { get; }
    public int OriginX { get; }
    public int OriginY { get; }
    public int Width { get; }
    public int Height { get; }
    public string BackgroundName { get; }
    public int UnknownTag { get; }
    public IReadOnlyList<HatRegion> Regions { get; }

    public HatLayout(ReadOnlySpan<byte> source)
    {
        if (source.Length < HeaderSize) throw new InvalidDataException("HAT header is truncated.");
        ScreenId = ReadInt(source, 0);
        OriginX = ReadInt(source, 4);
        OriginY = ReadInt(source, 8);
        Width = ReadDimension(source, 12, "width");
        Height = ReadDimension(source, 16, "height");
        var count = ReadInt(source, 20);
        if (count is < 0 or > 4096) throw new InvalidDataException("HAT region count is outside supported bounds.");
        var expected = checked(HeaderSize + count * RegionSize);
        var hasDosEof = source.Length == expected + 3 && source[expected] == 0x0d && source[expected + 1] == 0x0a && source[expected + 2] == 0x1a;
        if (source.Length != expected && !hasDosEof) throw new InvalidDataException("HAT length does not match its region count.");

        var nameBytes = source.Slice(24, 13);
        var terminator = nameBytes.IndexOf((byte)0);
        if (terminator < 0) terminator = nameBytes.Length;
        for (var i = 0; i < terminator; i++)
            if (nameBytes[i] is < 0x20 or > 0x7e) throw new InvalidDataException("HAT background name is not printable ASCII.");
        BackgroundName = Encoding.ASCII.GetString(nameBytes[..terminator]);
        UnknownTag = source[37] | source[38] << 8 | source[39] << 16;

        var regions = new HatRegion[count];
        for (var i = 0; i < count; i++)
        {
            var offset = HeaderSize + i * RegionSize;
            var id = ReadInt(source, offset);
            var x = ReadInt(source, offset + 4);
            var y = ReadInt(source, offset + 8);
            var width = ReadDimension(source, offset + 12, "region width");
            var height = ReadDimension(source, offset + 16, "region height");
            var enabled = ReadInt(source, offset + 20);
            if ((long)x >= Width || (long)y >= Height || (long)x + width <= 0 || (long)y + height <= 0)
                throw new InvalidDataException("HAT region does not intersect the declared screen.");
            regions[i] = new(id, x, y, width, height, enabled);
        }
        if (regions.Select(x => x.Id).Distinct().Count() != count) throw new InvalidDataException("HAT region identifiers are not unique.");
        Regions = regions;
    }

    public HatRegion? FindRegion(int id) => Regions.FirstOrDefault(x => x.Id == id);

    private static int ReadInt(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadInt32LittleEndian(source.Slice(offset, 4));

    private static int ReadDimension(ReadOnlySpan<byte> source, int offset, string label)
    {
        var value = ReadInt(source, offset);
        if (value is <= 0 or > 16_384) throw new InvalidDataException($"HAT {label} is outside supported bounds.");
        return value;
    }
}
