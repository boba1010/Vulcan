using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Fence : IFence
{
    internal ID3D11Fence* Handle { get; }

    private ulong _value;
    public ulong Value => _value;

    internal D3D11Fence(ID3D11Fence* handle)
    {
        Handle = handle;
    }

    public void Dispose()
    {
        if (Handle is not null)
            Handle->Release();
    }

    public void Wait()
    {
        while (Handle->GetCompletedValue() < _value)
            Thread.Yield();
    }
    internal ulong NextValue()
    {
        return ++_value;
    }
}
