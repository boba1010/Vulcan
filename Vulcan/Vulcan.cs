using Silk.NET.Windowing;
using System.Runtime.InteropServices;
using Vulcan.DirectX;
using Vulcan.Vulkan;

namespace Vulcan;

public static class Vulcan
{
    public static IGraphicsDevice CreateDevice(IWindow window)
    {
        if (OperatingSystem.IsWindows())
            return new Direct3D11GraphicsDevice(window);
        if (OperatingSystem.IsLinux())
            return new VulkanGraphicsDevice(window);
        if (OperatingSystem.IsAndroid())
            return new VulkanGraphicsDevice(window);

        throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not a supported OS");
    }

    public static I2DGraphicsDevice Create2DDevice(IWindow window)
    {
        throw new NotSupportedException($"{RuntimeInformation.OSDescription} is not a supported OS");
    }
}
