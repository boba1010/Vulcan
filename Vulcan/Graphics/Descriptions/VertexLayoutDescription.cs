namespace Vulcan.Graphics.Descriptions;

public readonly struct VertexLayoutDescription
{
    public ReadOnlyMemory<VertexElement> Elements { get; init; }
    public IShader VertexShader { get; init; }
}
