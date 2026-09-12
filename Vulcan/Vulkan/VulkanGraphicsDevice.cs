using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Silk.NET.Windowing;
using System.Numerics;

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

    public void Draw()
    {
    }

    public void DrawPoint(Vector2 position, Vector4 color)
    {
        throw new NotImplementedException();
    }

    public void DrawLine(Vector2 start, Vector2 end, Vector4 color)
    {
        throw new NotImplementedException();
    }

    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 color)
    {
        throw new NotImplementedException();
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Vector4 color)
    {
        throw new NotImplementedException();
    }

    public void DrawCircle(Vector2 center, float radius, Vector4 color, int segments = 64)
    {
        throw new NotImplementedException();
    }

    public void DrawPoint(Vector3 position)
    {
        throw new NotImplementedException();
    }

    public void DrawLine(Vector3 start, Vector3 end)
    {
        throw new NotImplementedException();
    }

    public void DrawTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        throw new NotImplementedException();
    }

    public void DrawCube(Vector3 position, Vector3 size)
    {
        throw new NotImplementedException();
    }

    public void DrawSphere(Vector3 position, float radius)
    {
        throw new NotImplementedException();
    }

    public void DrawPlane(Vector3 position, Vector2 size)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void Present()
    {
        throw new NotImplementedException();
    }
}
