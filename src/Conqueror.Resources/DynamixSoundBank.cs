using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record DynamixSoundSample(int Index, int SampleRate, byte[] Samples);
public sealed record DynamixSoundBank(IReadOnlyList<DynamixSoundSample> Samples);

public static class DynamixSoundBankDecoder
{
    public const uint Magic = 0x004A5031;
    private const int HeaderSize = 4;
    private const int SampleHeaderSize = 8;
    private const int MaximumBankBytes = 64 * 1024 * 1024;
    private const int MaximumSampleBytes = 16 * 1024 * 1024;
    private const int MaximumSamples = 4096;

    public static DynamixSoundBank Decode(ReadOnlySpan<byte> source)
    {
        if (source.Length < HeaderSize + SampleHeaderSize || source.Length > MaximumBankBytes)
            throw new InvalidDataException("Dynamix sound bank has an invalid length.");
        if (BinaryPrimitives.ReadUInt32LittleEndian(source) != Magic)
            throw new InvalidDataException("Dynamix sound bank magic is invalid.");

        var samples = new List<DynamixSoundSample>();
        var offset = HeaderSize;
        while (offset < source.Length)
        {
            if (samples.Count >= MaximumSamples || source.Length - offset < SampleHeaderSize)
                throw new InvalidDataException("Dynamix sound bank sample table is truncated or excessive.");
            var length = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset, 4));
            var rate = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset + 4, 4));
            offset += SampleHeaderSize;
            if (length is 0 or > MaximumSampleBytes || rate is < 1000 or > 192000 || length > source.Length - offset)
                throw new InvalidDataException("Dynamix sound bank sample metadata is invalid.");
            samples.Add(new DynamixSoundSample(samples.Count, checked((int)rate), source.Slice(offset, checked((int)length)).ToArray()));
            offset += checked((int)length);
        }
        return new DynamixSoundBank(samples);
    }
}
