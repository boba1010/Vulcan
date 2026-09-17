using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Texture : ITexture
{
    internal ID3D11Resource* Handle;
    public ID3D11DepthStencilView* DepthStencilView;

    internal D3D11Texture(ID3D11Resource* handle, ID3D11DepthStencilView* depthStencilView = null)
    {
        Handle = handle;
        DepthStencilView = depthStencilView;
    }

    public void Dispose()
    {
        if (DepthStencilView is not null)
        {
            DepthStencilView->Release();
            DepthStencilView = null;
        }

        if (Handle is not null)
        {
            Handle->Release();
            Handle = null;
        }
    }
}
