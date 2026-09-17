namespace Vulcan.Graphics.Descriptions;

public readonly struct DepthStencilDescription
{
    public bool DepthTestEnable { get; init; }
    public bool DepthWriteEnable { get; init; }
    public CompareOperation DepthCompare { get; init; }
}
