using Conqueror.Resources;
using System.Buffers.Binary;
using System.Text;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact] // Covers FMT-RES-002, RULE-RES-001, RULE-RES-002.
    public void KindOneEntryWithEqualSizesIsStillDecoded()
    {
        // "ABC", then six copies of three bytes from three back: 21 bytes stored for 21 expanded.
        byte[] stream =
        [
            19, 0, 0x40, 0x00, 0x1F, 0xC0, (byte)'A', (byte)'B', (byte)'C',
            0x00, 0x30, 0x00, 0x30, 0x00, 0x30, 0x00, 0x30, 0x00, 0x30, 0x00, 0x30,
        ];
        const int directoryOffset = 8 + 21;
        var bytes = new byte[directoryOffset + 4 + 52];
        ".RES"u8.CopyTo(bytes);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), directoryOffset);
        stream.CopyTo(bytes, 8);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(directoryOffset), 1);
        var record = bytes.AsSpan(directoryOffset + 4, 52);
        "Pal102"u8.CopyTo(record);
        BinaryPrimitives.WriteUInt32LittleEndian(record[32..], 1);
        BinaryPrimitives.WriteUInt32LittleEndian(record[40..], 21);
        BinaryPrimitives.WriteUInt32LittleEndian(record[44..], 21);
        BinaryPrimitives.WriteUInt32LittleEndian(record[48..], 8);

        var archive = new DynamixArchive(bytes);
        var entry = Assert.Single(archive.Entries);

        Assert.False(entry.IsStored);
        Assert.Equal(string.Concat(Enumerable.Repeat("ABC", 7)), Encoding.ASCII.GetString(archive.ReadDecoded(entry)));
    }
}
