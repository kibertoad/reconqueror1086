using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void ResourceHashIsTheCanonicalXxh3Of128Bits()
    {
        // Reference values from Python's xxhash package, which prints what `xxhsum -H2` prints.
        Assert.Equal("99aa06d3014798d86001c324468d497f", ResourceHash.Xxh3([]));
        Assert.Equal("06b05ab6733a618578af5f94892f3950", ResourceHash.Xxh3("abc"u8));
        using var stream = new MemoryStream("abc"u8.ToArray());
        Assert.Equal("06b05ab6733a618578af5f94892f3950", ResourceHash.Xxh3(stream));
        Assert.True(ResourceHash.IsXxh3("06b05ab6733a618578af5f94892f3950"));
        Assert.False(ResourceHash.IsXxh3(new string('a', 64)));
    }
}
