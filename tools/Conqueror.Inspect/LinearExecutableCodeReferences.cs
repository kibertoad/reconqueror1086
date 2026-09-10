using Conqueror.Resources;
using Iced.Intel;
using System.Buffers.Binary;
using System.Text;

internal static class LinearExecutableCodeReferences
{
    public static string Find(string path, IReadOnlyList<uint> targets)
    {
        var bytes = File.ReadAllBytes(path);
        var header = FindHeader(bytes);
        var module = FindModuleStart(bytes, header);
        var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x28, 4));
        var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x40, 4));
        var objectCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x44, 4));
        if (objectCount == 0) throw new InvalidDataException("Linear Executable has no mapped objects.");
        var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x80, 4)));
        var report = new StringBuilder("# 32-bit LE code references (derived metadata; original bytes omitted)\n");
        report.AppendLine($"# requested: {string.Join(',', targets.Select(target => $"0x{target:X}"))}");

        foreach (var fixup in LinearExecutableFixupReader.ReadInternalFixups(bytes)
            .Where(fixup => fixup.TargetObject == 1 && targets.Contains(fixup.TargetOffset))
            .OrderBy(fixup => fixup.SourceAddress))
            report.AppendLine($"0x{fixup.SourceAddress:X8}  relocation code+0x{fixup.TargetOffset:X}  source-type 0x{fixup.SourceType:X2}"
                + $"{(fixup.Additive ? " additive" : "")}{(fixup.Chained ? " chained" : "")}");

        var descriptor = checked(header + (int)objectTable);
        var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
        var baseAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 4, 4));
        var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
        var fileOffset = checked(dataPages + (pageIndex - 1) * pageSize);
        var available = Math.Min(checked((int)virtualSize), bytes.Length - checked((int)fileOffset));
        var decoder = Iced.Intel.Decoder.Create(32,
            new ByteArrayCodeReader(bytes.AsSpan(checked((int)fileOffset), available).ToArray()));
        decoder.IP = baseAddress;
        var formatter = new IntelFormatter();
        while (decoder.IP < baseAddress + (uint)available)
        {
            decoder.Decode(out var instruction);
            if (instruction.IsInvalid || instruction.FlowControl is not
                (FlowControl.Call or FlowControl.ConditionalBranch or FlowControl.UnconditionalBranch)) continue;
            var target = instruction.NearBranchTarget;
            if (target > uint.MaxValue || !targets.Contains((uint)target)) continue;
            var formatted = new StringOutput();
            formatter.Format(instruction, formatted);
            report.AppendLine($"0x{instruction.IP:X8}  direct code+0x{target:X}  {formatted}");
        }
        return report.ToString();
    }

    public static string FindBlockFlags(string path, IReadOnlyList<uint> flags)
    {
        if (flags.Count == 0 || flags.Any(flag => flag is 0 or > byte.MaxValue))
            throw new ArgumentOutOfRangeException(nameof(flags), "Block flags must be bytes from 1 through 255.");
        var bytes = File.ReadAllBytes(path);
        var header = FindHeader(bytes);
        var module = FindModuleStart(bytes, header);
        var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x28, 4));
        var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x40, 4));
        var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x80, 4)));
        var descriptor = checked(header + (int)objectTable);
        var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
        var baseAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 4, 4));
        var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
        var fileOffset = checked(dataPages + (pageIndex - 1) * pageSize);
        var available = Math.Min(checked((int)virtualSize), bytes.Length - checked((int)fileOffset));
        var decoder = Iced.Intel.Decoder.Create(32,
            new ByteArrayCodeReader(bytes.AsSpan(checked((int)fileOffset), available).ToArray()));
        decoder.IP = baseAddress;
        var formatter = new IntelFormatter();
        var report = new StringBuilder("# References to byte flags in the scene block field at offset 4 (derived metadata)\n");
        report.AppendLine($"# requested: {string.Join(',', flags.Select(flag => $"0x{flag:X2}"))}");
        while (decoder.IP < baseAddress + (uint)available)
        {
            decoder.Decode(out var instruction);
            if (instruction.IsInvalid || instruction.MemoryDisplacement64 != 4 ||
                instruction.Mnemonic is not (Mnemonic.Test or Mnemonic.And or Mnemonic.Or or Mnemonic.Xor or Mnemonic.Cmp))
                continue;
            for (var operand = 0; operand < instruction.OpCount; operand++)
            {
                if (instruction.GetOpKind(operand) is not (OpKind.Immediate8 or OpKind.Immediate8_2nd)) continue;
                var immediate = (uint)instruction.GetImmediate(operand);
                if (!flags.Contains(immediate)) continue;
                var formatted = new StringOutput();
                formatter.Format(instruction, formatted);
                report.AppendLine($"0x{instruction.IP:X8}  block+4 flag 0x{immediate:X2}  {formatted}");
                break;
            }
        }
        return report.ToString();
    }

    private static int FindHeader(ReadOnlySpan<byte> source)
    {
        for (var index = 0; index <= source.Length - 0x84; index++)
            if (source[index] == (byte)'L' && source[index + 1] == (byte)'E'
                && source[index + 2] == 0 && source[index + 3] == 0
                && BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(index + 0x44, 4)) is > 0 and < 64
                && BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(index + 0x28, 4)) is >= 512 and <= 65536)
                return index;
        throw new InvalidDataException("Linear Executable header was not found.");
    }

    private static int FindModuleStart(ReadOnlySpan<byte> source, int header)
    {
        for (var offset = header; offset >= 0; offset--)
        {
            if (offset > source.Length - 0x40 || source[offset] != (byte)'M' || source[offset + 1] != (byte)'Z') continue;
            var relativeHeader = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset + 0x3c, 4));
            if (relativeHeader <= int.MaxValue && offset + (int)relativeHeader == header) return offset;
        }
        throw new InvalidDataException("The Linear Executable header is not owned by a bounded MZ module.");
    }
}
