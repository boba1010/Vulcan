namespace Vulcan.UI;

public readonly struct Rect(float x, float y, float height, float width)
{
    public float X { get; } = x;
    public float Y { get; } = y;
    public float Height { get; } = height;
    public float Width { get; } = width;
}
