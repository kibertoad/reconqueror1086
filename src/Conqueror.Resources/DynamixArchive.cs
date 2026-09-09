using System.Buffers.Binary;
using System.Collections.Frozen;
using System.Text;

namespace Conqueror.Resources;

public sealed record DynamixEntry(int Index, string Name, uint Flags, uint Reserved, uint StoredSize, uint ExpandedSize, uint Offset)
{
    public bool IsStored => StoredSize == ExpandedSize;
}

public sealed class DynamixArchive
{
    private const int HeaderSize = 8;
    private const int DirectoryEntrySize = 52;
    private static readonly FrozenDictionary<uint, Func<byte[], int, byte[]>> CompressionDecoders =
        new Dictionary<uint, Func<byte[], int, byte[]>>
        {
            [1] = static (source, expectedSize) => DynamixCompression.DecodeKind1(source, expectedSize),
            [2] = static (source, expectedSize) => DynamixCompression.DecodeKind2(source, expectedSize),
        }.ToFrozenDictionary();
    private readonly byte[] _data;
    public string SourceName { get; }
    public IReadOnlyList<DynamixEntry> Entries { get; }

    public DynamixArchive(string path) : this(File.ReadAllBytes(Path.GetFullPath(path)), Path.GetFullPath(path)) { }

    public DynamixArchive(byte[] data, string sourceName = "<memory>")
    {
        ArgumentNullException.ThrowIfNull(data);
        _data = data;
        SourceName = sourceName;
        using var stream = new MemoryStream(_data, writable: false);
        Span<byte> header = stackalloc byte[HeaderSize];
        stream.ReadExactly(header);
        if (!header[..4].SequenceEqual(".RES"u8)) throw new InvalidDataException("Not a Dynamix .RES archive.");
        var directoryOffset = BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if (directoryOffset < HeaderSize || directoryOffset > stream.Length - 4) throw new InvalidDataException("Invalid Dynamix archive directory offset.");
        stream.Position = directoryOffset;
        Span<byte> countBytes = stackalloc byte[4];
        stream.ReadExactly(countBytes);
        var count = BinaryPrimitives.ReadUInt32LittleEndian(countBytes);
        if (count > 1_000_000 || directoryOffset + 4L + count * DirectoryEntrySize > stream.Length)
            throw new InvalidDataException("Invalid Dynamix archive directory size.");

        var entries = new List<DynamixEntry>(checked((int)count));
        var buffer = new byte[DirectoryEntrySize];
        for (var index = 0; index < count; index++)
        {
            stream.ReadExactly(buffer);
            var zero = Array.IndexOf(buffer, (byte)0, 0, 32);
            var name = Encoding.ASCII.GetString(buffer, 0, zero < 0 ? 32 : zero);
            if (string.IsNullOrWhiteSpace(name)) throw new InvalidDataException("Dynamix archive contains an unnamed entry.");
            var flags = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(32, 4));
            var reserved = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(36, 4));
            var stored = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(40, 4));
            var expanded = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(44, 4));
            var offset = BinaryPrimitives.ReadUInt32LittleEndian(buffer.AsSpan(48, 4));
            if (offset < HeaderSize || (long)offset + stored > directoryOffset) throw new InvalidDataException($"Entry {index} points outside the archive data area.");
            entries.Add(new DynamixEntry(index, name, flags, reserved, stored, expanded, offset));
        }
        Entries = entries;
    }

    public byte[] ReadStored(DynamixEntry entry)
    {
        if (!Entries.Contains(entry)) throw new ArgumentException("Entry does not belong to this archive.", nameof(entry));
        var bytes = new byte[checked((int)entry.StoredSize)];
        _data.AsSpan(checked((int)entry.Offset), bytes.Length).CopyTo(bytes);
        return bytes;
    }

    public byte[] ReadDecoded(DynamixEntry entry)
    {
        if (entry.IsStored) return ReadStored(entry);
        if (!CompressionDecoders.TryGetValue(entry.Flags, out var decoder))
            throw new NotSupportedException($"Compression kind {entry.Flags} for '{entry.Name}' has not been verified.");
        return decoder(ReadStored(entry), checked((int)entry.ExpandedSize));
    }

    public static bool CanDecode(DynamixEntry entry) => entry.IsStored || CompressionDecoders.ContainsKey(entry.Flags);

    public static bool HasContainerExtension(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        var extension = Path.GetExtension(path);
        return extension.Equals(".RES", StringComparison.OrdinalIgnoreCase)
            || extension.Equals(".LOW", StringComparison.OrdinalIgnoreCase);
    }
}
