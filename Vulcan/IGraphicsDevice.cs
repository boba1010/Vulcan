using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace Vulcan;

public interface IGraphicsDevice : IDisposable
{
    void Initialize();

    IBuffer CreateBuffer(in BufferDescription description);
    ITexture CreateTexture(in TextureDescription description);
    ISampler CreateSampler(in SamplerDescription description);
    IShader CreateShader(in ShaderDescription description);
    IVertexLayout CreateVertexLayout(in VertexLayoutDescription description);
    IRasterizerState CreateRasterizerState(in RasterizerDescription description);
    IDepthStencilState CreateDepthStencilState(in DepthStencilDescription description);
    IBlendState CreateBlendState(in BlendDescription description);
    IPipeline CreatePipeline(in PipelineDescription description);

    ISwapchain CreateSwapchain(in SwapchainDescription description);

    ICommandQueue CreateCommandQueue();

    IFence CreateFence();
    ISemaphore CreateSemaphore();

    void WaitIdle();
}
