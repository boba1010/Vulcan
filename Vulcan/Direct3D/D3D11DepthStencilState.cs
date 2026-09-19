using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11DepthStencilState : IDepthStencilState
{
    internal ID3D11DepthStencilState* Handle;

    internal D3D11DepthStencilState(ID3D11DepthStencilState* handle)
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
