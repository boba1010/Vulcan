namespace Vulcan.Graphics;

public interface IBuffer : IDisposable
{
    void Upload(ReadOnlySpan<byte> data);
}
