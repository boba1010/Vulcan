namespace Vulcan.Graphics;

public interface IViewport
{
    float X { get; }
    float Y { get; }
    float Width { get; }
    float Height { get; }
    float MinDepth { get; }
    float MaxDepth { get; }
}
