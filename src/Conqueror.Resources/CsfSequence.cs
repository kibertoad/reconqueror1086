using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record CsfChunk(int Index, int Offset, int Size);
public sealed record CsfDimensionHeader(int Width, int Height);
public sealed record CsfFrame(int Width, int Height, byte[] Indices, byte[] Alpha, int LiteralSegments, int TransparentSegments, int FillSegments)
{
    public byte[] ToRgba(ReadOnlySpan<byte> paletteRgb)
    {
        if (paletteRgb.Length != 256 * 3) throw new ArgumentException("A CSF frame requires a 256-color RGB palette.", nameof(paletteRgb));
        if (Width <= 0 || Height <= 0 || (long)Width * Height != Indices.Length || Alpha.Length != Indices.Length)
            throw new InvalidDataException("CSF frame pixel buffers do not match its dimensions.");
        var rgba = new byte[checked(Indices.Length * 4)];
        for (var pixel = 0; pixel < Indices.Length; pixel++)
        {
            var palette = Indices[pixel] * 3;
            var target = pixel * 4;
            var alpha = Alpha[pixel];
            rgba[target] = (byte)(paletteRgb[palette] * alpha / 255);
            rgba[target + 1] = (byte)(paletteRgb[palette + 1] * alpha / 255);
            rgba[target + 2] = (byte)(paletteRgb[palette + 2] * alpha / 255);
            rgba[target + 3] = alpha;
        }
        return rgba;
    }
}

// FMT-MEDIA-001 file layout; RULE-MEDIA-001 builds the same frame offsets.
/// <summary>Structural reader for the observed CSF chunk-sequence container.</summary>
public sealed class CsfSequence
{
    public const ushort ObservedMagic = 0x4A32;
    private readonly byte[] _data;
    public IReadOnlyList<CsfChunk> Chunks { get; }

    public CsfSequence(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
        if (data.Length < 6) throw new InvalidDataException("CSF resource is too short.");
        if (BinaryPrimitives.ReadUInt16LittleEndian(data) != ObservedMagic) throw new InvalidDataException("CSF resource has an unknown signature.");
        var count = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(2));
        if (count > 1_000_000 || 6L + count * 4 > data.Length) throw new InvalidDataException("CSF chunk table is out of bounds.");

        var chunks = new List<CsfChunk>(checked((int)count));
        var payloadOffset = checked(6 + (int)count * 4);
        var position = payloadOffset;
        for (var index = 0; index < count; index++)
        {
            var size = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(6 + checked((int)index) * 4));
            if (size > int.MaxValue || size > data.Length - position) throw new InvalidDataException("CSF chunk extends past the resource.");
            chunks.Add(new CsfChunk(checked((int)index), position, (int)size));
            position += (int)size;
        }
        if (position != data.Length) throw new InvalidDataException("CSF chunk sizes do not consume the resource exactly.");
        Chunks = chunks;
    }

    public byte[] ReadChunk(CsfChunk chunk)
    {
        if (!Chunks.Contains(chunk)) throw new ArgumentException("Chunk does not belong to this CSF resource.", nameof(chunk));
        return _data.AsSpan(chunk.Offset, chunk.Size).ToArray();
    }

    public CsfDimensionHeader ReadDimensionHeader(CsfChunk chunk, int maximumWidth = 4096, int maximumHeight = 4096)
    {
        if (!Chunks.Contains(chunk)) throw new ArgumentException("Chunk does not belong to this CSF resource.", nameof(chunk));
        if (chunk.Size < 4) throw new InvalidDataException("CSF chunk is too short for a dimension header.");
        var width = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(chunk.Offset));
        var height = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(chunk.Offset + 2));
        if (width == 0 || height == 0 || width > maximumWidth || height > maximumHeight)
            throw new InvalidDataException("CSF chunk dimensions are outside the configured bounds.");
        return new CsfDimensionHeader(width, height);
    }

    // FMT-MEDIA-002 rows. The original treats any operation other than 0 and 2 as a skip (RULE-MEDIA-002).
    /// <summary>Decodes the observed scanline segment stream into palette indices and an opacity mask.</summary>
    public CsfFrame DecodeFrame(CsfChunk chunk, int maximumPixels = 16_777_216)
    {
        if (!Chunks.Contains(chunk)) throw new ArgumentException("Chunk does not belong to this CSF resource.", nameof(chunk));
        if (maximumPixels < 0) throw new ArgumentOutOfRangeException(nameof(maximumPixels));
        var dimensions = ReadDimensionHeader(chunk);
        var pixelCount = checked(dimensions.Width * dimensions.Height);
        if (pixelCount > maximumPixels) throw new InvalidDataException("CSF frame dimensions exceed the configured pixel limit.");

        var indices = new byte[pixelCount];
        var alpha = new byte[pixelCount];
        var literalSegments = 0;
        var transparentSegments = 0;
        var fillSegments = 0;
        var position = chunk.Offset + 4;
        var end = chunk.Offset + chunk.Size;
        for (var row = 0; row < dimensions.Height; row++)
        {
            if (position >= end) throw new InvalidDataException("CSF frame ended before its scanline segment count.");
            var segmentCount = _data[position++];
            var column = 0;
            for (var segment = 0; segment < segmentCount; segment++)
            {
                if (end - position < 3) throw new InvalidDataException("CSF frame ended inside a scanline segment header.");
                var operation = _data[position++];
                var length = BinaryPrimitives.ReadUInt16LittleEndian(_data.AsSpan(position));
                position += 2;
                if (length == 0 || length > dimensions.Width - column)
                    throw new InvalidDataException("CSF scanline segment has an invalid length.");
                var target = row * dimensions.Width + column;
                switch (operation)
                {
                    case 0:
                        if (length > end - position) throw new InvalidDataException("CSF literal segment extends past its chunk.");
                        _data.AsSpan(position, length).CopyTo(indices.AsSpan(target, length));
                        alpha.AsSpan(target, length).Fill(255);
                        position += length;
                        literalSegments++;
                        break;
                    case 1:
                        transparentSegments++;
                        break;
                    case 2:
                        if (position >= end) throw new InvalidDataException("CSF fill segment is missing its palette index.");
                        indices.AsSpan(target, length).Fill(_data[position++]);
                        alpha.AsSpan(target, length).Fill(255);
                        fillSegments++;
                        break;
                    default:
                        throw new InvalidDataException($"CSF frame uses unknown scanline operation {operation}.");
                }
                column += length;
            }
            if (column != dimensions.Width) throw new InvalidDataException("CSF scanline segments do not fill the declared width.");
        }
        if (position != end) throw new InvalidDataException("CSF frame has trailing bytes after its scanlines.");
        return new CsfFrame(dimensions.Width, dimensions.Height, indices, alpha, literalSegments, transparentSegments, fillSegments);
    }
}
