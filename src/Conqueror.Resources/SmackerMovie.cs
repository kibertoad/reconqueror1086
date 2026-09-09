using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record SmackerAudioTrack(
    int Index, int SampleRate, int MaximumDecodedBytes, bool IsCompressed, bool Is16Bit, bool IsStereo);

public sealed record SmackerFrame(int Index, int Offset, int Length, byte Flags, bool IsKeyFrame);

public sealed record SmackerMovie(
    int Version,
    int Width,
    int Height,
    TimeSpan FrameDuration,
    uint Flags,
    int TreeOffset,
    int TreeLength,
    IReadOnlyList<SmackerAudioTrack> AudioTracks,
    IReadOnlyList<SmackerFrame> Frames);

public static class SmackerMovieDecoder
{
    private const uint Smk2Magic = 0x324B4D53;
    private const uint Smk4Magic = 0x344B4D53;
    private const int HeaderSize = 104;
    private const int AudioTrackCount = 7;
    private const int MaximumMovieBytes = 256 * 1024 * 1024;
    private const int MaximumDimension = 4096;
    private const int MaximumFrames = 1_000_000;

    public static SmackerMovie Decode(ReadOnlySpan<byte> source)
    {
        if (source.Length < HeaderSize || source.Length > MaximumMovieBytes)
            throw new InvalidDataException("Smacker movie has an invalid length.");

        var magic = ReadUInt32(source, 0);
        var version = magic switch
        {
            Smk2Magic => 2,
            Smk4Magic => 4,
            _ => throw new InvalidDataException("Smacker movie magic is invalid.")
        };
        var width = ReadBoundedInt(source, 4, MaximumDimension, "width");
        var height = ReadBoundedInt(source, 8, MaximumDimension, "height");
        if ((width & 3) != 0 || (height & 3) != 0)
            throw new InvalidDataException("Smacker dimensions must align to four-pixel blocks.");

        var declaredFrames = ReadUInt32(source, 12);
        var rawFrameDuration = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(16, 4));
        var flags = ReadUInt32(source, 20);
        var frameCount = checked((long)declaredFrames + ((flags & 1) != 0 ? 1 : 0));
        if (frameCount is <= 0 or > MaximumFrames || rawFrameDuration is 0 or int.MinValue)
            throw new InvalidDataException("Smacker timing or frame count is invalid.");

        var durationTicks = rawFrameDuration > 0
            ? checked((long)rawFrameDuration * TimeSpan.TicksPerMillisecond)
            : checked(-(long)rawFrameDuration * TimeSpan.TicksPerSecond / 100_000);
        if (durationTicks <= 0)
            throw new InvalidDataException("Smacker frame duration is invalid.");

        var audioTracks = new List<SmackerAudioTrack>();
        for (var index = 0; index < AudioTrackCount; index++)
        {
            var maximumBytes = ReadUInt32(source, 24 + index * 4);
            var rateAndFlags = ReadUInt32(source, 72 + index * 4);
            var sampleRate = checked((int)(rateAndFlags & 0x00FF_FFFF));
            var audioFlags = (byte)(rateAndFlags >> 24);
            if (maximumBytes > MaximumMovieBytes)
                throw new InvalidDataException("Smacker audio buffer is excessive.");
            if (sampleRate == 0) continue;
            if (sampleRate is < 1_000 or > 192_000)
                throw new InvalidDataException("Smacker audio sample rate is invalid.");
            audioTracks.Add(new SmackerAudioTrack(index, sampleRate, checked((int)maximumBytes),
                (audioFlags & 0x80) != 0, (audioFlags & 0x20) != 0, (audioFlags & 0x10) != 0));
        }

        var treeLength = ReadUInt32(source, 52);
        var tableLength = checked(frameCount * 5);
        var treeOffset = checked((long)HeaderSize + tableLength);
        var frameDataOffset = checked(treeOffset + treeLength);
        if (frameDataOffset > source.Length)
            throw new InvalidDataException("Smacker frame tables or trees are truncated.");

        var frames = new List<SmackerFrame>(checked((int)frameCount));
        var offset = frameDataOffset;
        for (var index = 0; index < frameCount; index++)
        {
            var encodedLength = ReadUInt32(source, checked(HeaderSize + (int)index * 4));
            var length = encodedLength & 0xFFFF_FFFCu;
            if (length == 0 || length > int.MaxValue || offset + length > source.Length)
                throw new InvalidDataException("Smacker frame extent is invalid.");
            var frameFlags = source[checked(HeaderSize + (int)(frameCount * 4) + (int)index)];
            frames.Add(new SmackerFrame(index, checked((int)offset), checked((int)length), frameFlags,
                index == 0 || (encodedLength & 1) != 0));
            offset += length;
        }
        if (offset != source.Length)
            throw new InvalidDataException("Smacker frame extents do not consume the movie exactly.");

        return new SmackerMovie(version, width, height, new TimeSpan(durationTicks), flags,
            checked((int)treeOffset), checked((int)treeLength), audioTracks, frames);
    }

    private static int ReadBoundedInt(ReadOnlySpan<byte> source, int offset, int maximum, string field)
    {
        var value = ReadUInt32(source, offset);
        if (value is 0 || value > maximum)
            throw new InvalidDataException($"Smacker {field} is invalid.");
        return checked((int)value);
    }

    private static uint ReadUInt32(ReadOnlySpan<byte> source, int offset) =>
        BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset, 4));
}
