using Silk.NET.Vulkan;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice
{
    private Device _device;
    private Queue _graphicsQueue;
    private Queue _presentQueue;

    private void CreateLogicalDevice()
    {
        float queuePriority = 1.0f;

        var queueCreateInfo = new DeviceQueueCreateInfo
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = _graphicsQueueFamily,
            QueueCount = 1,
            PQueuePriorities = &queuePriority
        };

        var deviceCreateInfo = new DeviceCreateInfo
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo
        };

        if (_vk.CreateDevice(_physicalDevice, &deviceCreateInfo, null, out _device) != Result.Success)
            throw new Exception("Failed to create logical device.");
    }

    private void GetQueues()
    {
        Span<Queue> graphicsQueues = stackalloc Queue[1];

        _vk.GetDeviceQueue(_device, _graphicsQueueFamily, 0, graphicsQueues);

        _graphicsQueue = graphicsQueues[0];

        Span<Queue> presentQueues = stackalloc Queue[1];

        _vk.GetDeviceQueue(_device, _presentQueueFamily, 0, presentQueues);

        _presentQueue = presentQueues[0];
    }
}
