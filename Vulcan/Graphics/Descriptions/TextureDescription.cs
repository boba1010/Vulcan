namespace Vulcan.Graphics.Descriptions;

public readonly struct TextureDescription
{
    public uint Width { get; init; }
    public uint Height { get; init; }
    public uint Depth { get; init; }

    public uint MipLevels { get; init; }
    public uint ArrayLayers { get; init; }

    public TextureFormat Format { get; init; }
    public TextureUsage Usage { get; init; }
    public TextureType Type { get; init; }

    public SampleCount Samples { get; init; }
}
