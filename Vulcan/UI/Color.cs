namespace Vulcan.UI;

public readonly struct Color(float r, float g, float b, float a)
{
    public float R { get; } = r;
    public float G { get; } = g;
    public float B { get; } = b;
    public float A { get; } = a;
}
