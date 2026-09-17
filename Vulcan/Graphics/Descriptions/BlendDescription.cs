namespace Vulcan.Graphics.Descriptions;

public readonly struct BlendDescription
{
    public bool Enable { get; init; }

    public BlendFactor SourceColor { get; init; }
    public BlendFactor DestinationColor { get; init; }
    public BlendOperation ColorOperation { get; init; }

    public BlendFactor SourceAlpha { get; init; }
    public BlendFactor DestinationAlpha { get; init; }
    public BlendOperation AlphaOperation { get; init; }
}
