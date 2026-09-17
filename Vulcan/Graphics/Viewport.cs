namespace Vulcan.Graphics;

public readonly struct Viewport : IViewport
{
    public float X { get; init; }
    public float Y { get; init; }
    public float Width { get; init; }
    public float Height { get; init; }
    public float MinDepth { get; init; }
    public float MaxDepth { get; init; }
}
