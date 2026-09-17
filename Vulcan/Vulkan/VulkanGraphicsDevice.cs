using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice : IGraphicsDevice
{
    private readonly IWindow _window;
    private Vk _vk = null!;
    private uint _graphicsQueueFamily;
    private uint _presentQueueFamily;

    public VulkanGraphicsDevice(IWindow window)
    {
        _window = window;
        _window.Render += OnRender;
    }

    private void OnRender(double deltaTime)
    {

    }

    public void Initialize()
    {
        _vk = Vk.GetApi();
        CreateInstace();
        CreateSurface();
        PickPhysicalDevice();
        FindQueueFamilies();
        CreateLogicalDevice();
        GetQueues();
        CreateSwapchain();
    }

    public void Dispose()
    {
        _window.Render -= OnRender;

        if (_vk.TryGetInstanceExtension(_instance, out KhrSurface khrSurface))
        {
            khrSurface.DestroySurface(_instance, _surface, null);
        }

        _vk.DestroyInstance(_instance, null);

        _vk.Dispose();
        GC.SuppressFinalize(this);
    }

    public IBuffer CreateBuffer(in BufferDescription description)
    {
        throw new NotImplementedException();
    }

    public ITexture CreateTexture(in TextureDescription description)
    {
        throw new NotImplementedException();
    }

    public ISampler CreateSampler(in SamplerDescription description)
    {
        throw new NotImplementedException();
    }

    public IShader CreateShader(in ShaderDescription description)
    {
        throw new NotImplementedException();
    }

    public IPipeline CreatePipeline(in PipelineDescription description)
    {
        throw new NotImplementedException();
    }

    public ISwapchain CreateSwapchain(in SwapchainDescription description)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer CreateCommandBuffer()
    {
        throw new NotImplementedException();
    }

    public ICommandQueue CreateCommandQueue()
    {
        throw new NotImplementedException();
    }

    public IFence CreateFence()
    {
        throw new NotImplementedException();
    }

    public ISemaphore CreateSemaphore()
    {
        throw new NotImplementedException();
    }

    public void WaitIdle()
    {
        throw new NotImplementedException();
    }

    public IVertexLayout CreateVertexLayout(in VertexLayoutDescription description)
    {
        throw new NotImplementedException();
    }

    public IRasterizerState CreateRasterizerState(in RasterizerDescription description)
    {
        throw new NotImplementedException();
    }

    public IDepthStencilState CreateDepthStencilState(in DepthStencilDescription description)
    {
        throw new NotImplementedException();
    }

    public IBlendState CreateBlendState(in BlendDescription description)
    {
        throw new NotImplementedException();
    }
}
