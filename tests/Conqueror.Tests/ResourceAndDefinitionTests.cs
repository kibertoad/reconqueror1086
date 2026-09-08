using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed class ResourceAndDefinitionTests
{
    [Fact]
    public void Kind1DecodesLiteralCopyAndRunTokens()
    {
        byte[] compressed = [13, 0, 0x40, 0, 0x18, 0, (byte)'A', (byte)'B', (byte)'C', 0, 0x30, 0, 0, 0, (byte)'Z'];

        var decoded = DynamixCompression.DecodeKind1(compressed, 22);

        Assert.Equal("ABCABC" + new string('Z', 16), System.Text.Encoding.ASCII.GetString(decoded));
    }

    [Fact]
    public void Kind1RejectsCopiesBeforeOutputStart()
    {
        byte[] compressed = [7, 0, 0x40, 0, 0x80, 0, 0, 0x10];

        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind1(compressed, 3));
    }

    [Fact]
    public void OriginalArtRolesAreDataDrivenAndUnique()
    {
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Title.Background", IdSuffix: ":fftitle.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Options", IdSuffix: ":char_ops.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Pregenerated", IdSuffix: ":pregen.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Load.Background", IdSuffix: ":loadgame.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Map.England", IdSuffix: ":engmap1.pcx" });
        Assert.Equal(ImportedArt.Definitions.Count, ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void CharacterCreationScreenIsDefinitionDriven()
    {
        Assert.Equal(Enum.GetValues<CharacterCreationAction>().Length, CharacterCreationDefinitions.Options.Count);
        Assert.Equal(CharacterCreationDefinitions.Options.Count, CharacterCreationDefinitions.Options.Select(x => x.Action).Distinct().Count());
        Assert.Equal(Balance.Templates.Length, CharacterCreationDefinitions.PregeneratedCharacters.Count);
        Assert.Equal(["Red", "Green", "Blue"], CharacterCreationDefinitions.HeraldicColors.Select(x => x.Name));
        Assert.All(CharacterCreationDefinitions.HeraldicColors, x => Assert.True(CharacterCreationDefinitions.Options[1].OriginalBounds.Contains(x.OriginalBounds.X, x.OriginalBounds.Y)));
    }

    [Fact]
    public void HatLayoutDecodesAndOverridesFallbackRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 0, 7); WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        System.Text.Encoding.ASCII.GetBytes("SCREEN.PCX").CopyTo(bytes, 24);
        bytes[37] = 0x6d; bytes[38] = 0xc0; bytes[39] = 0x45;
        WriteInt(bytes, 40, 0); WriteInt(bytes, 44, 69); WriteInt(bytes, 48, 18);
        WriteInt(bytes, 52, 119); WriteInt(bytes, 56, 133); WriteInt(bytes, 60, 1);

        var layout = new HatLayout(bytes);

        Assert.Equal((7, "SCREEN.PCX", 0x45c06d), (layout.ScreenId, layout.BackgroundName, layout.UnknownTag));
        Assert.Equal(new HatRegion(0, 69, 18, 119, 133, 1), layout.FindRegion(0));
        Assert.Equal(new UiBounds(69, 18, 119, 133), CharacterCreationDefinitions.PregeneratedFrom(layout)[0]);
    }

    [Fact]
    public void HatLayoutRejectsOutOfBoundsRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        WriteInt(bytes, 44, 650); WriteInt(bytes, 48, 10); WriteInt(bytes, 52, 20); WriteInt(bytes, 56, 20);
        Assert.Throws<InvalidDataException>(() => new HatLayout(bytes));
    }

    private static void WriteInt(byte[] target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, 4), value);

    [Fact]
    public void BalanceDefinitionsRemainInternallyConsistent()
    {
        Assert.All(Balance.Buildings, x => Assert.Equal(x.Key, x.Value.Kind));
        Assert.Equal(UnitType.Swordsmen, Balance.Counter(UnitType.Knights));
        Assert.Equal(UnitType.Halberdiers, Balance.Counter(UnitType.Swordsmen));
        Assert.Equal(UnitType.Knights, Balance.Counter(UnitType.Halberdiers));
    }
}
