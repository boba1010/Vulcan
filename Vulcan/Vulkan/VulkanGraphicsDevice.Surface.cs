using Silk.NET.Vulkan;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice
{
    private SurfaceKHR _surface;

    private void CreateSurface()
    {
        var surfaceExtension = _window.VkSurface ??
            throw new NotSupportedException("Vulkan surfaces are not supported on this platform.");

        var handle = surfaceExtension.Create<AllocationCallbacks>(_instance.ToHandle(), null);

        if (handle.Handle == 0)
            throw new Exception("Failed to create window surface.");

        _surface = handle.ToSurface();
    }
}
