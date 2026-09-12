using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice
{
    private PhysicalDevice _physicalDevice;

    private void PickPhysicalDevice()
    {
        uint deviceCount = 0;
        _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, null);

        if (deviceCount == 0)
            throw new Exception("No Vulkan-compatible GPU found.");

        var devices = new PhysicalDevice[deviceCount];

        fixed (PhysicalDevice* devicesPtr = devices)
        {
            _vk.EnumeratePhysicalDevices(_instance, ref deviceCount, devicesPtr);

            _physicalDevice = devices[0];
        }
    }

    public void FindQueueFamilies()
    {
        uint queueFamilyCount = 0;

        _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, &queueFamilyCount, null);

        if (queueFamilyCount == 0)
            throw new Exception("GPU has no queue families.");

        var families = new QueueFamilyProperties[queueFamilyCount];

        fixed (QueueFamilyProperties* familiesPtr = families)
        {
            _vk.GetPhysicalDeviceQueueFamilyProperties(_physicalDevice, ref queueFamilyCount, familiesPtr);
        }

        if (!_vk.TryGetInstanceExtension(_instance, out KhrSurface khrSurface))
            throw new Exception("VK_KHR_surface is not available.");

        Console.WriteLine($"Surface: {_surface.Handle}");
        Console.WriteLine($"Physical device: {_physicalDevice.Handle}");

        bool foundGraphics = false;
        bool foundPresent = false;
        for (uint i = 0; i < queueFamilyCount; i++)
        {
            if ((families[i].QueueFlags & QueueFlags.GraphicsBit) != 0)
            {
                _graphicsQueueFamily = i;
                foundGraphics = true;
            }

            khrSurface.GetPhysicalDeviceSurfaceSupport(_physicalDevice, i, _surface, out var pSupported);

            if (pSupported)
            {
                _presentQueueFamily = i;
                foundPresent = true;
            }

            if (foundGraphics && foundPresent)
                break;
        }

        Console.WriteLine($"Graphics queue family: {_graphicsQueueFamily}");
        Console.WriteLine($"Present queue family: {_presentQueueFamily}");

        if (!foundGraphics)
            throw new Exception("No graphics queue family found.");

        if (!foundPresent)
            throw new Exception("No presentation queue family found.");
    }
}
