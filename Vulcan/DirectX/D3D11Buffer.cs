using Silk.NET.Direct3D11;
using System.Runtime.InteropServices;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Buffer(ID3D11Buffer* handle, ID3D11DeviceContext* context) : IBuffer
{
    internal ID3D11Buffer* Handle = handle;

    private ID3D11DeviceContext* _context = context;

    public void Upload(ReadOnlySpan<byte> data)
    {
        fixed (byte* source = data)
        {
            MappedSubresource mapped = default;
            
            var result = _context->Map((ID3D11Resource*)Handle, 0, Map.WriteDiscard, 0, &mapped);
            
            if (result < 0)
                throw new InvalidOperationException($"Failed to map D3D11 buffer. HRESULT: 0x{result:X8}");
            
            Buffer.MemoryCopy(source, mapped.PData, data.Length, data.Length);

            _context->Unmap((ID3D11Resource*)Handle, 0);
        }
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
