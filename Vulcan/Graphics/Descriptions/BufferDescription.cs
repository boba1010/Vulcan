namespace Vulcan.Graphics.Descriptions;

public readonly struct BufferDescription
{
    public ulong Size { get; init; }
    public BufferUsage Usage { get; init; }
    public MemoryUsage MemoryUsage { get; init; }
}
