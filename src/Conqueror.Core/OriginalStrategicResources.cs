namespace Conqueror.Core;

/// <summary>
/// One signed route-space coordinate from an original strategic <c>.rat</c>
/// resource. The Core contract deliberately does not expose resource-layer
/// decoder types.
/// </summary>
public readonly record struct OriginalStrategicRoutePoint(int X, int Y);

/// <summary>
/// One projected cell from the immutable original <c>icon.jp</c> grid.
/// All four source bytes remain available so later <c>temp.jap</c>-equivalent
/// mutations can preserve fields whose meanings are not yet recovered.
/// </summary>
public readonly record struct OriginalStrategicTerrainCell(
    int Row,
    int Column,
    uint RawValue)
{
    public ushort TileId => (ushort)RawValue;
    public byte Auxiliary => (byte)(RawValue >> 16);
    public byte UpperByte => (byte)(RawValue >> 24);
}

/// <summary>
/// Supplies the executable-selected route population and camera-ordered world
/// projection without making the simulation assembly depend on import or
/// resource-decoder implementations.
/// </summary>
public interface IOriginalStrategicResources
{
    IReadOnlyList<OriginalStrategicRoutePoint> Route(string resourceName, bool reverse);

    bool TryTerrainCell(
        int worldX,
        int worldY,
        int cameraRow,
        int cameraColumn,
        out OriginalStrategicTerrainCell cell);
}
