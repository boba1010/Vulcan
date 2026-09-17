namespace Vulcan.Graphics.Descriptions;

public readonly struct RasterizerDescription
{
    public CullMode CullMode { get; init; }
    public FrontFace FrontFace { get; init; }
    public FillMode FillMode { get; init; }
    public bool DepthClipEnable { get; init; }
}
