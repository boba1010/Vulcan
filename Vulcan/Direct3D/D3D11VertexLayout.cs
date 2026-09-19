using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11VertexLayout : IVertexLayout
{
    internal ID3D11InputLayout* Handle;

    internal D3D11VertexLayout(ID3D11InputLayout* handle)
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