using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice
{
    private KhrSwapchain _khrSwapchain = null!;
    private SwapchainKHR _swapchain;

    private void CreateSwapchain()
    {
        //if (!_vk.TryGetInstanceExtension(_instance, out KhrSurface khrSurface))
        //    throw new Exception("VK_KHR_surface is not available.");

        //if (!_vk.TryGetDeviceExtension(_instance, _device, out KhrSwapchain khrSwapchain))
        //    throw new Exception("VK_KHR_swapchain is not available.");

        
    }
}
