using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Vulcan.Vulkan;

public unsafe partial class VulkanGraphicsDevice
{
	private Instance _instance;

	private void CreateInstace()
	{
		var extensions = _window.VkSurface!.GetRequiredExtensions(out uint count);
		if (extensions is null || count == 0)
			throw new InvalidOperationException("Failed to get required Vulkan instance extensions.");

		var appInfo = new ApplicationInfo
		{
			SType = StructureType.ApplicationInfo,
			PApplicationName = (byte*)SilkMarshal.StringToPtr("Crafty"),
			ApplicationVersion = new Version32(26, 10, 1),
			PEngineName = (byte*)SilkMarshal.StringToPtr("Vulcan"),
			EngineVersion = new Version32(0, 1, 0),
			ApiVersion = Vk.Version12
		};

		var instanceInfo = new InstanceCreateInfo
		{
			SType = StructureType.InstanceCreateInfo,
			PApplicationInfo = &appInfo,
			EnabledExtensionCount = count,
			PpEnabledExtensionNames = extensions
		};

		if (_vk.CreateInstance(&instanceInfo, null, out _instance) != Result.Success)
			throw new Exception("Failed to create Vulkan instance.");

		SilkMarshal.Free((nint)extensions);
		SilkMarshal.Free((nint)appInfo.PApplicationName);
		SilkMarshal.Free((nint)appInfo.PEngineName);
	}
}
