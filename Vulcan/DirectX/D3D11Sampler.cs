using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Sampler : ISampler
{
    internal ID3D11SamplerState* Handle;

    internal D3D11Sampler(ID3D11SamplerState* handle)
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
