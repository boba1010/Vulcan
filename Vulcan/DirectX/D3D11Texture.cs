using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Texture : ITexture
{
    internal ID3D11Resource* Handle;

    internal D3D11Texture(ID3D11Resource* handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        if (Handle is not null)
        {
            Handle->Release();
            Handle = null;
        }
    }
}
