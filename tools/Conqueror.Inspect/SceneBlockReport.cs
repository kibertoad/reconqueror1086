using Conqueror.Resources;
using System.Buffers.Binary;
using System.Text;

internal static class SceneBlockReport
{
    internal static StringBuilder Create() => new(
        "# Archive  Index  Placed  Kind  Behavior  Flags  ColorMap  Field40  StateEffect  MoveEffect  MoveFrames  MoveMs  MoveFlags  MoveDX  MoveDY  Field48  Field4A  Field4C  Size  Surfaces  Name\n");

    internal static void Append(StringBuilder report, string archive, DynamixSceneBlock block,
        byte[] blockBytes, IReadOnlyList<DynamixSceneEffectDefinition> effects, int placements)
    {
        var record = blockBytes.AsSpan(block.Index * DynamixSceneDecoder.BlockSize, DynamixSceneDecoder.BlockSize);
        var movement = block.MovementEffectDefinitionIndex >= 0
            ? effects[block.MovementEffectDefinitionIndex]
            : null;
        report.AppendLine($"{archive}  {block.Index,5}  {placements,6}  "
            + $"{block.Kind,4}  {block.Behavior,8}  0x{block.Flags:X8}  {block.ColorMapOffset,8}  "
            + $"{BinaryPrimitives.ReadInt32LittleEndian(record.Slice(0x40, 4)),7}  {block.EffectDefinitionIndex,11}  {block.MovementEffectDefinitionIndex,10}  "
            + $"{movement?.FrameCount ?? -1,10}  {movement?.IntervalMilliseconds ?? -1,6}  0x{movement?.Flags ?? 0:X8}  {movement?.MapXDeltaPerTick ?? 0,6}  {movement?.MapYDeltaPerTick ?? 0,6}  "
            + $"{block.InteractionSelector,7}  {block.InteractionArgument,7}  {block.InteractionArgument2,7}  "
            + $"{block.Width}x{block.Height}  {block.Surface0},{block.Surface1},{block.Surface2},{block.Surface3}  "
            + block.Name.Replace('\r', ' ').Replace('\n', ' '));
    }
}
