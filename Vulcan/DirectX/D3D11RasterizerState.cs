using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11RasterizerState : IRasterizerState
{
    internal ID3D11RasterizerState* Handle;

    internal D3D11RasterizerState(ID3D11RasterizerState* handle)
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