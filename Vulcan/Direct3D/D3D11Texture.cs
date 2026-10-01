using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Texture : ITexture
{
    internal ID3D11Resource* Handle;
    internal ID3D11ShaderResourceView* ShaderResourceView;
    public ID3D11DepthStencilView* DepthStencilView;
    private readonly ID3D11DeviceContext* _context;

    internal D3D11Texture(ID3D11Resource* handle, ID3D11DeviceContext* context, ID3D11ShaderResourceView* shaderResourceView, ID3D11DepthStencilView* depthStencilView = null)
    {
        Handle = handle;
        DepthStencilView = depthStencilView;
        _context = context;
        ShaderResourceView = shaderResourceView;
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

    public void Upload(ReadOnlySpan<byte> data, uint rowPitch)
    {
        fixed (byte* pixels = data)
        {
            _context->UpdateSubresource(Handle, 0, null, pixels, rowPitch, 0);
        }
    }
}