using System.Buffers.Binary;
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
    private readonly string _path;
    public IReadOnlyList<DynamixEntry> Entries { get; }

    public DynamixArchive(string path)
    {
        _path = Path.GetFullPath(path);
        using var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
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
        using var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        stream.Position = entry.Offset;
        var bytes = new byte[checked((int)entry.StoredSize)];
        stream.ReadExactly(bytes);
        return bytes;
    }

    public byte[] ReadDecoded(DynamixEntry entry)
    {
        if (!entry.IsStored) throw new NotSupportedException($"Compression for '{entry.Name}' has not been verified.");
        return ReadStored(entry);
    }
}
