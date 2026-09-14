using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Buffer(ID3D11Buffer* handle) : IBuffer
{
    internal ID3D11Buffer* Handle = handle;

    public void Dispose()
    {
        if (Handle is not null)
        {
            Handle->Release();
            Handle = null;
        }
    }
}
