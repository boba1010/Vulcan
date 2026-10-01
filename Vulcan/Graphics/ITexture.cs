namespace Vulcan.Graphics;

public interface ITexture : IDisposable
{
    void Upload(ReadOnlySpan<byte> data, uint rowPitch);
}
