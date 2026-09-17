namespace Vulcan.Graphics;

public readonly struct Scissor : IScissorRect
{
    public int X { get; init; }
    public int Y { get; init; }
    public uint Width { get; init; }
    public uint Height { get; init; }
}
