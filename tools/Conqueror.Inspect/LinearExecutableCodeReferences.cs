using Conqueror.Resources;
using Iced.Intel;
using System.Buffers.Binary;
using System.Globalization;
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

    public static string ReadDataTable(string path, uint offset, int rows, int columns)
    {
        if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
        if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
        var bytes = File.ReadAllBytes(path);
        var header = FindHeader(bytes);
        var module = FindModuleStart(bytes, header);
        var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x28, 4));
        var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x40, 4));
        var objectCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x44, 4));
        if (objectCount < 2) throw new InvalidDataException("Linear Executable has no data object.");
        var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x80, 4)));
        var descriptor = checked(header + (int)objectTable + 24);
        var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
        var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
        var length = checked(rows * columns * sizeof(int));
        if (offset > virtualSize || length > virtualSize - offset)
            throw new InvalidDataException("Requested table is outside the data object.");
        var fileOffset = checked(dataPages + (pageIndex - 1) * pageSize + offset);
        if (fileOffset > bytes.Length || length > bytes.Length - fileOffset)
            throw new InvalidDataException("Requested table is outside the executable file.");
        var report = new StringBuilder("# 32-bit LE data table (derived numeric metadata; original bytes omitted)\n");
        report.AppendLine($"# object2+0x{offset:X}; {rows} rows; {columns} signed dwords per row");
        for (var row = 0; row < rows; row++)
        {
            var values = Enumerable.Range(0, columns).Select(column => BinaryPrimitives.ReadInt32LittleEndian(
                bytes.AsSpan(checked((int)fileOffset + (row * columns + column) * sizeof(int)), sizeof(int))));
            report.AppendLine($"{row,2}  {string.Join(' ', values)}");
        }
        return report.ToString();
    }

    public static string ReadStrategicTerrainMovement(string path)
    {
        const int limitOffset = 0x7389;
        const int terrainKindOffset = 0xAF78;
        const int terrainTileKindCount = 331;
        const int strategicGridScratchNameOffset = 0x5CBC;
        const int selectedProfileOffset = 0xB610;
        const int primarySpeedOffset = 0xB614;
        const int reducedSpeedOffset = 0xB68C;
        const int profilePointerOffset = 0xB704;
        const int monthProfileOffset = 0xB720;
        const int seasonMovieOffset = 0xB750;
        const int seasonAtlasOffset = 0xB760;
        const int startingPersonOffset = 0xB8C8;
        const int personTableOffset = 0xBA50;
        const int startingSelectorOffset = 0xC9D0;
        const int startingRouteOffset = 0xCA98;
        const int terrainKindCount = 30;
        const int profileCount = 4;
        const int monthCount = 12;
        const int startingRouteCount = 7;
        const int personCount = 176;
        const int personRecordSize = 0x12;

        var bytes = File.ReadAllBytes(path);
        var header = FindHeader(bytes);
        var module = FindModuleStart(bytes, header);
        var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x28, 4));
        var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x40, 4));
        var objectCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x44, 4));
        if (objectCount < 2) throw new InvalidDataException("Linear Executable has no data object.");
        var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(header + 0x80, 4)));
        var descriptor = checked(header + (int)objectTable + 24);
        var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
        var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
        var objectOffset = checked((int)(dataPages + (pageIndex - 1) * pageSize));
        if (startingRouteOffset + startingRouteCount * sizeof(int) > virtualSize ||
            profilePointerOffset + profileCount * sizeof(int) > virtualSize ||
            reducedSpeedOffset + terrainKindCount * sizeof(float) > virtualSize)
            throw new InvalidDataException("Strategic terrain tables are outside the data object.");

        int ReadInt32(int offset) => BinaryPrimitives.ReadInt32LittleEndian(
            bytes.AsSpan(checked(objectOffset + offset), sizeof(int)));
        ushort ReadUInt16(int offset) => BinaryPrimitives.ReadUInt16LittleEndian(
            bytes.AsSpan(checked(objectOffset + offset), sizeof(ushort)));
        byte ReadByte(int offset) => bytes[checked(objectOffset + offset)];
        double ReadDouble(int offset) => BinaryPrimitives.ReadDoubleLittleEndian(
            bytes.AsSpan(checked(objectOffset + offset), sizeof(double)));
        float ReadSingle(int offset) => BinaryPrimitives.ReadSingleLittleEndian(
            bytes.AsSpan(checked(objectOffset + offset), sizeof(float)));
        string ReadAscii(int offset)
        {
            if (offset < 0 || offset >= virtualSize)
                throw new InvalidDataException($"Strategic string pointer 0x{offset:X} is outside the data object.");
            var end = offset;
            while (end < virtualSize && end - offset < 128 && ReadByte(end) != 0) end++;
            if (end >= virtualSize || end - offset == 128)
                throw new InvalidDataException($"Strategic string at 0x{offset:X} is not bounded.");
            return Encoding.ASCII.GetString(bytes, objectOffset + offset, end - offset);
        }

        var report = new StringBuilder("# Strategic movement metadata (derived values; original bytes omitted)\n");
        report.AppendLine($"# terrain-kind lookup object2+0x{terrainKindOffset:X}; 30 speed entries per table");
        report.AppendLine($"strategic-grid-scratch {ReadAscii(strategicGridScratchNameOffset)}");
        report.AppendLine("strategic-grid-layout cell 80x80 rows 200 columns 400 column-major-dwords");
        report.AppendLine("terrain-kinds-by-tile " + string.Join(' ', Enumerable.Range(0, terrainTileKindCount)
            .Select(index => ReadByte(terrainKindOffset + index))));
        report.AppendLine($"horizontal-application-limit {ReadDouble(limitOffset).ToString("R", CultureInfo.InvariantCulture)}");
        report.AppendLine($"initial-profile {ReadInt32(selectedProfileOffset)}");
        report.AppendLine("profile-pointers " + string.Join(' ', Enumerable.Range(0, profileCount)
            .Select(index => $"0x{ReadInt32(profilePointerOffset + index * sizeof(int)):X}")));
        foreach (var (name, offset) in new[] { ("primary", primarySpeedOffset), ("reduced", reducedSpeedOffset) })
            report.AppendLine(name + " " + string.Join(' ', Enumerable.Range(0, terrainKindCount)
                .Select(index => ReadSingle(offset + index * sizeof(float)).ToString("R", CultureInfo.InvariantCulture))));
        report.AppendLine("month-profiles " + string.Join(' ', Enumerable.Range(0, monthCount)
            .Select(index => ReadInt32(monthProfileOffset + index * sizeof(int)))));
        for (var profile = 0; profile < profileCount; profile++)
            report.AppendLine($"profile {profile} movie {ReadAscii(ReadInt32(seasonMovieOffset + profile * sizeof(int)))} " +
                $"atlas {ReadAscii(ReadInt32(seasonAtlasOffset + profile * sizeof(int)))}");

        report.AppendLine($"initial-starting-route-selector {ReadInt32(startingSelectorOffset)}");
        for (var selector = 0; selector < startingRouteCount; selector++)
        {
            var person = ReadInt32(startingPersonOffset + selector * sizeof(int));
            var personOffset = checked(personTableOffset + person * personRecordSize);
            if (person < 0 || personOffset + personRecordSize > virtualSize)
                throw new InvalidDataException($"Starting person index {person} is invalid.");
            report.AppendLine($"starting-route {selector} person {person} " +
                $"name {ReadAscii(ReadInt32(personOffset))} group {ReadByte(personOffset + 4)} " +
                $"assignment {ReadByte(personOffset + 7)} xy {ReadUInt16(personOffset + 8)},{ReadUInt16(personOffset + 10)} " +
                $"resource {ReadAscii(ReadInt32(startingRouteOffset + selector * sizeof(int)))}");
        }
        report.AppendLine("person-fields index name-address group state5 flags assignment x y rating list-next state14 state15 state16 state17");
        for (var person = 0; person < personCount; person++)
        {
            var personOffset = checked(personTableOffset + person * personRecordSize);
            report.AppendLine(FormattableString.Invariant(
                $"person {person} 0x{ReadInt32(personOffset):X} {ReadByte(personOffset + 4)} {ReadByte(personOffset + 5)} {ReadByte(personOffset + 6)} {ReadByte(personOffset + 7)} {ReadUInt16(personOffset + 8)} {ReadUInt16(personOffset + 10)} {ReadByte(personOffset + 12)} {ReadByte(personOffset + 13)} {ReadByte(personOffset + 14)} {ReadByte(personOffset + 15)} {ReadByte(personOffset + 16)} {ReadByte(personOffset + 17)}"));
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
