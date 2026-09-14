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
    IPipeline CreatePipeline(in PipelineDescription description);

    ISwapchain CreateSwapchain(in SwapchainDescription description);

    ICommandBuffer CreateCommandBuffer();
    ICommandQueue CreateCommandQueue();

    IFence CreateFence();
    ISemaphore CreateSemaphore();

    void WaitIdle();
}
