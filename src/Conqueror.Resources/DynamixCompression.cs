using System.Buffers.Binary;

namespace Conqueror.Resources;

public enum DynamixBlockStorage : byte { Compressed = 0x40, Stored = 0x80 }
public sealed record DynamixCompressedBlock(int Index, int PayloadOffset, int PayloadSize, DynamixBlockStorage Storage);

/// <summary>Bounded readers and decoders for compression used by Dynamix resources.</summary>
public static class DynamixCompression
{
    public const int Kind1ExpandedBlockSize = 16 * 1024;
    private const int ClearCode = 256;
    private const int FirstDictionaryCode = 257;
    private const int MaximumCodeCount = 4096;
    private const int Kind2MaximumCodeCount = 1 << 14;
    public const int DefaultMaximumExpandedSize = 256 * 1024 * 1024;

    // FMT-RES-003 and RULE-RES-002. The original takes any marker other than 0x80 as compressed and
    // does not require 16 KiB blocks or copies inside the block; every shipped stream satisfies both.
    /// <summary>
    /// Inventories the framing observed in outer-archive compression kind 1.
    /// Each block has a two-byte little-endian payload length.
    /// </summary>
    public static IReadOnlyList<DynamixCompressedBlock> ReadKind1Blocks(ReadOnlySpan<byte> source)
    {
        var blocks = new List<DynamixCompressedBlock>();
        var position = 0;
        while (position < source.Length)
        {
            if (source.Length - position < 2)
                throw new InvalidDataException("Compressed resource ends inside a block length.");
            var payloadSize = BinaryPrimitives.ReadUInt16LittleEndian(source[position..]);
            position += 2;
            if (payloadSize == 0 || payloadSize > source.Length - position)
                throw new InvalidDataException("Compressed resource has an invalid block length.");
            var marker = source[position];
            if (marker is not ((byte)DynamixBlockStorage.Compressed) and not ((byte)DynamixBlockStorage.Stored))
                throw new InvalidDataException($"Compressed resource block uses unknown storage marker 0x{marker:X2}.");
            blocks.Add(new DynamixCompressedBlock(blocks.Count, position, payloadSize, (DynamixBlockStorage)marker));
            position += payloadSize;
        }
        return blocks;
    }

    public static byte[] DecodeKind1Block(ReadOnlySpan<byte> source, DynamixCompressedBlock block, int expectedSize)
    {
        if (expectedSize < 0) throw new ArgumentOutOfRangeException(nameof(expectedSize));
        if (block.PayloadOffset < 0 || block.PayloadSize < 1 || block.PayloadOffset > source.Length - block.PayloadSize)
            throw new InvalidDataException("Kind-1 block extent is outside its resource.");
        var payload = source.Slice(block.PayloadOffset, block.PayloadSize);
        if (block.Storage == DynamixBlockStorage.Stored)
        {
            if (payload.Length != expectedSize + 1)
                throw new InvalidDataException("Stored kind-1 block length does not match its expected output slice.");
            return payload[1..].ToArray();
        }
        return DecodeKind1CompressedPayload(payload, expectedSize);
    }

    public static byte[] DecodeKind1(ReadOnlySpan<byte> source, int expectedSize, int maximumExpandedSize = DefaultMaximumExpandedSize)
    {
        ValidateExpandedSize(expectedSize, maximumExpandedSize);
        var blocks = ReadKind1Blocks(source);
        if (blocks.Count != ExpectedKind1BlockCount(expectedSize))
            throw new InvalidDataException("Kind-1 block count does not match the declared expanded size.");
        var output = new byte[expectedSize];
        var outputPosition = 0;
        foreach (var block in blocks)
        {
            var decoded = DecodeKind1Block(source, block, ExpectedKind1BlockSize(expectedSize, block.Index));
            decoded.CopyTo(output, outputPosition);
            outputPosition += decoded.Length;
        }
        return output;
    }

    private static byte[] DecodeKind1CompressedPayload(ReadOnlySpan<byte> payload, int expectedSize)
    {
        if (payload.Length < 4 || payload[0] != (byte)DynamixBlockStorage.Compressed)
            throw new InvalidDataException("Compressed kind-1 block has an invalid header.");

        // Byte 1 is metadata unused by the original decoder. Control words are
        // big-endian and consumed most-significant bit first; token data follows.
        var control = (ushort)((payload[2] << 8) | payload[3]);
        var controlBits = 16;
        var inputPosition = 4;
        var output = new byte[expectedSize];
        var outputPosition = 0;

        while (inputPosition < payload.Length)
        {
            if (outputPosition >= output.Length)
                throw new InvalidDataException("Compressed kind-1 block expands beyond its declared size.");
            if (controlBits == 0)
            {
                if (payload.Length - inputPosition < 2)
                    throw new InvalidDataException("Compressed kind-1 block ends inside a control word.");
                control = (ushort)((payload[inputPosition] << 8) | payload[inputPosition + 1]);
                inputPosition += 2;
                controlBits = 16;
            }

            if ((control & 0x8000) == 0)
            {
                output[outputPosition++] = payload[inputPosition++];
            }
            else
            {
                if (payload.Length - inputPosition < 2)
                    throw new InvalidDataException("Compressed kind-1 block ends inside a copy token.");
                var first = payload[inputPosition++];
                var second = payload[inputPosition++];
                var distance = (first << 4) | (second >> 4);
                if (distance != 0)
                {
                    var length = (second & 0x0f) + 3;
                    if (distance > outputPosition)
                        throw new InvalidDataException("Compressed kind-1 block copies before the output buffer.");
                    if (length > output.Length - outputPosition)
                        throw new InvalidDataException("Compressed kind-1 block expands beyond its declared size.");
                    for (var index = 0; index < length; index++)
                        output[outputPosition + index] = output[outputPosition - distance + index];
                    outputPosition += length;
                }
                else
                {
                    if (payload.Length - inputPosition < 2)
                        throw new InvalidDataException("Compressed kind-1 block ends inside a run token.");
                    var length = (second << 8) + payload[inputPosition++] + 16;
                    var value = payload[inputPosition++];
                    if (length > output.Length - outputPosition)
                        throw new InvalidDataException("Compressed kind-1 block expands beyond its declared size.");
                    output.AsSpan(outputPosition, length).Fill(value);
                    outputPosition += length;
                }
            }

            control <<= 1;
            controlBits--;
        }

        if (outputPosition != expectedSize)
            throw new InvalidDataException($"Compressed kind-1 block produced {outputPosition} of {expectedSize} expected bytes.");
        return output;
    }

    public static int ExpectedKind1BlockCount(int expandedSize)
    {
        if (expandedSize < 0) throw new ArgumentOutOfRangeException(nameof(expandedSize));
        return expandedSize == 0 ? 0 : checked((expandedSize - 1) / Kind1ExpandedBlockSize + 1);
    }

    public static int ExpectedKind1BlockSize(int expandedSize, int blockIndex)
    {
        if (expandedSize < 0) throw new ArgumentOutOfRangeException(nameof(expandedSize));
        if (blockIndex < 0) throw new ArgumentOutOfRangeException(nameof(blockIndex));
        var remaining = (long)expandedSize - (long)blockIndex * Kind1ExpandedBlockSize;
        if (remaining <= 0) throw new ArgumentOutOfRangeException(nameof(blockIndex));
        return (int)Math.Min(Kind1ExpandedBlockSize, remaining);
    }

    // FND-RES-008 shows this is not the kind-1 codec; nothing in the original's containers uses it.
    /// <summary>
    /// Decodes the raw, least-significant-bit-first LZW stream used by
    /// documented inner Dynamix chunks. The exact expanded size is external.
    /// This is intentionally not used for outer GOB compression kind 1.
    /// </summary>
    public static byte[] DecodeLzw(ReadOnlySpan<byte> source, int expectedSize, int maximumExpandedSize = DefaultMaximumExpandedSize)
    {
        ValidateExpandedSize(expectedSize, maximumExpandedSize);
        if (expectedSize == 0) return [];

        var output = new byte[expectedSize];
        var prefixes = new ushort[MaximumCodeCount];
        var suffixes = new byte[MaximumCodeCount];
        var stack = new byte[MaximumCodeCount];
        for (var code = 0; code < 256; code++) suffixes[code] = (byte)code;

        var reader = new LsbBitReader(source);
        var outputPosition = 0;
        var codeWidth = 9;
        var nextCode = FirstDictionaryCode;
        var previousCode = -1;
        byte previousFirstByte = 0;

        while (outputPosition < expectedSize)
        {
            if (!reader.TryRead(codeWidth, out var code))
                throw new InvalidDataException($"Compressed resource ended after {outputPosition} of {expectedSize} expanded bytes.");
            if (code == ClearCode)
            {
                codeWidth = 9;
                nextCode = FirstDictionaryCode;
                previousCode = -1;
                continue;
            }
            if (code > nextCode)
                throw new InvalidDataException($"Compressed resource refers to undefined LZW code {code}.");

            var stackSize = 0;
            int current;
            if (code == nextCode)
            {
                if (previousCode < 0) throw new InvalidDataException("Compressed resource starts with an undefined LZW code.");
                current = previousCode;
                stack[stackSize++] = previousFirstByte;
            }
            else current = code;

            var chainLength = 0;
            while (current >= 256)
            {
                if (current >= nextCode || chainLength++ >= MaximumCodeCount)
                    throw new InvalidDataException("Compressed resource contains an invalid LZW dictionary chain.");
                stack[stackSize++] = suffixes[current];
                current = prefixes[current];
            }

            var firstByte = (byte)current;
            stack[stackSize++] = firstByte;
            if (stackSize > expectedSize - outputPosition)
                throw new InvalidDataException("Compressed resource expands beyond its declared size.");
            while (stackSize > 0) output[outputPosition++] = stack[--stackSize];

            if (previousCode >= 0 && nextCode < MaximumCodeCount)
            {
                prefixes[nextCode] = (ushort)previousCode;
                suffixes[nextCode++] = firstByte;
                if (nextCode == (1 << codeWidth) && codeWidth < 12) codeWidth++;
            }
            previousCode = code;
            previousFirstByte = firstByte;
        }
        return output;
    }

    // FMT-RES-004 and RULE-RES-003. A code above the next free code is rejected, where the original
    // expands it as the next one.
    /// <summary>Decodes outer-archive kind 2 using the executable-confirmed MSB-first 9-to-14-bit LZW variant.</summary>
    public static byte[] DecodeKind2(ReadOnlySpan<byte> source, int expectedSize, int maximumExpandedSize = DefaultMaximumExpandedSize)
    {
        const int endCode = 257;
        const int firstDictionaryCode = 258;
        ValidateExpandedSize(expectedSize, maximumExpandedSize);
        if (expectedSize == 0) return [];

        var prefixes = new ushort[Kind2MaximumCodeCount];
        var suffixes = new byte[Kind2MaximumCodeCount];
        var stack = new byte[Kind2MaximumCodeCount];
        for (var code = 0; code < 256; code++) suffixes[code] = (byte)code;
        var reader = new MsbBitReader(source);
        var output = new byte[expectedSize];
        var outputPosition = 0;
        var width = 9;
        var nextCode = firstDictionaryCode;
        var previousCode = -1;
        byte previousFirst = 0;
        var ended = false;

        while (reader.TryRead(width, out var code))
        {
            if (code == endCode) { ended = true; break; }
            if (code == ClearCode)
            {
                width = 9;
                nextCode = firstDictionaryCode;
                previousCode = -1;
                continue;
            }
            if (code > nextCode || code >= Kind2MaximumCodeCount)
                throw new InvalidDataException($"Kind-2 resource refers to undefined LZW code {code}.");

            var stackSize = 0;
            var current = code;
            if (code == nextCode)
            {
                if (previousCode < 0) throw new InvalidDataException("Kind-2 resource starts with an undefined LZW code.");
                current = previousCode;
                stack[stackSize++] = previousFirst;
            }
            while (current >= 256)
            {
                if (current >= nextCode || stackSize >= stack.Length)
                    throw new InvalidDataException("Kind-2 resource contains an invalid LZW dictionary chain.");
                stack[stackSize++] = suffixes[current];
                current = prefixes[current];
            }
            var first = (byte)current;
            stack[stackSize++] = first;
            if (stackSize > output.Length - outputPosition)
                throw new InvalidDataException("Kind-2 resource expands beyond its declared size.");
            while (stackSize > 0) output[outputPosition++] = stack[--stackSize];

            if (previousCode >= 0 && nextCode < Kind2MaximumCodeCount)
            {
                prefixes[nextCode] = (ushort)previousCode;
                suffixes[nextCode++] = first;
                if (width < 14 && nextCode == (1 << width) - 1) width++;
            }
            previousCode = code;
            previousFirst = first;
        }
        if (!ended)
            throw new InvalidDataException("Kind-2 resource is truncated before its end code.");
        if (outputPosition != expectedSize)
            throw new InvalidDataException($"Kind-2 resource produced {outputPosition} of {expectedSize} expected bytes before its end code.");
        return output;
    }

    private static void ValidateExpandedSize(int expectedSize, int maximumExpandedSize)
    {
        if (expectedSize < 0) throw new ArgumentOutOfRangeException(nameof(expectedSize));
        if (maximumExpandedSize < 0) throw new ArgumentOutOfRangeException(nameof(maximumExpandedSize));
        if (expectedSize > maximumExpandedSize)
            throw new InvalidDataException($"Expanded resource size {expectedSize} exceeds the configured limit {maximumExpandedSize}.");
    }

    private ref struct LsbBitReader(ReadOnlySpan<byte> source)
    {
        private readonly ReadOnlySpan<byte> _source = source;
        private int _bitPosition;

        public bool TryRead(int width, out int value)
        {
            if (_bitPosition > _source.Length * 8 - width) { value = 0; return false; }
            value = 0;
            for (var bit = 0; bit < width; bit++)
            {
                var position = _bitPosition + bit;
                value |= ((_source[position >> 3] >> (position & 7)) & 1) << bit;
            }
            _bitPosition += width;
            return true;
        }
    }

    private ref struct MsbBitReader(ReadOnlySpan<byte> source)
    {
        private readonly ReadOnlySpan<byte> _source = source;
        private int _bitPosition;

        public bool TryRead(int width, out int value)
        {
            if (_bitPosition > _source.Length * 8 - width) { value = 0; return false; }
            value = 0;
            for (var bit = 0; bit < width; bit++)
            {
                var position = _bitPosition + bit;
                value = (value << 1) | ((_source[position >> 3] >> (7 - (position & 7))) & 1);
            }
            _bitPosition += width;
            return true;
        }
    }
}
