using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11CommandQueue : ICommandQueue
{
    internal ID3D11DeviceContext* Context { get; }
    internal ID3D11DeviceContext4* Context4 { get; }

    internal D3D11CommandQueue(ID3D11DeviceContext4* context4, ID3D11DeviceContext* context)
    {
        Context4 = context4;
        Context = context;
    }

    public ICommandBuffer CreateCommandBuffer()
    {
        return new D3D11CommandBuffer(Context);
    }

    public void Submit(ICommandBuffer commandBuffer)
    {
    }

    public void Signal(IFence fence)
    {
        var d3dFence = (D3D11Fence)fence;
        var value = d3dFence.NextValue();

        Context4->Signal(d3dFence.Handle, value);
    }

    public void Dispose()
    {
    }
}
