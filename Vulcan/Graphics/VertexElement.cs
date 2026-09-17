namespace Vulcan.Graphics;

public readonly struct VertexElement
{
    public string Semantic { get; init; }
    public uint Location { get; init; }
    public TextureFormat Format { get; init; }
    public uint Offset { get; init; }
}
