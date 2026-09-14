namespace Vulcan.Graphics.Descriptions;

public readonly struct PipelineDescription
{
    public IShader VertexShader { get; init; }
    public IShader FragmentShader { get; init; }

    public IVertexLayout VertexLayout { get; init; }

    public PrimitiveTopology PrimitiveTopology { get; init; }

    public IRasterizerState Rasterizer { get; init; }
    public IDepthStencilState DepthStencil { get; init; }
    public IBlendState Blend { get; init; }
}
