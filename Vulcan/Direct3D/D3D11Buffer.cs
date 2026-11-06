using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Buffer(ID3D11Buffer* handle, ID3D11DeviceContext* context, ulong size) : IBuffer
{
    internal ID3D11Buffer* Handle = handle;

    private ID3D11DeviceContext* _context = context;
    private readonly ulong _size = size;

    public void Upload(ReadOnlySpan<byte> data)
    {
        if ((ulong)data.Length > _size)
            throw new ArgumentOutOfRangeException(nameof(data));

        fixed (byte* source = data)
        {
            MappedSubresource mapped = default;

            var result = _context->Map((ID3D11Resource*)Handle, 0, Map.WriteDiscard, 0, &mapped);

            if (result < 0)
                throw new InvalidOperationException($"Failed to map D3D11 buffer. HRESULT: 0x{result:X8}");

            try
            {
                Buffer.MemoryCopy(source, mapped.PData,data.Length, data.Length);
            }
            finally
            {
                _context->Unmap((ID3D11Resource*)Handle, 0);
            }
        }
    }

    public void UploadRange(ReadOnlySpan<byte> data, ulong destinationOffset)
    {
        if (destinationOffset > _size || (ulong)data.Length > _size - destinationOffset)
            throw new ArgumentOutOfRangeException(nameof(destinationOffset));

        if (data.IsEmpty)
            return;

        fixed (byte* source = data)
        {
            MappedSubresource mapped = default;

            var result = _context->Map((ID3D11Resource*)Handle, 0, Map.WriteNoOverwrite, 0, &mapped);

            if (result < 0)
                throw new InvalidOperationException($"Failed to map D3D11 buffer. HRESULT: 0x{result:X8}");

            try
            {
                byte* destination = (byte*)mapped.PData + checked((nint)destinationOffset);
                Buffer.MemoryCopy(source, destination, checked((long)(_size - destinationOffset)), data.Length);
            }
            finally
            {
                _context->Unmap((ID3D11Resource*)Handle, 0);
            }
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