namespace Vulcan.Graphics.Descriptions;

public readonly struct SwapchainDescription
{
    public nint WindowHandle { get; init; }

    public uint Width { get; init; }
    public uint Height { get; init; }

    public TextureFormat Format { get; init; }
    public PresentMode PresentMode { get; init; }

    public uint BufferCount { get; init; }
}
