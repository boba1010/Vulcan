namespace Vulcan.Graphics;

public interface IScissorRect
{
    int X { get; init; }
    int Y { get; init; }
    uint Width { get; init; }
    uint Height { get; init; }
}
