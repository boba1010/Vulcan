using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Vulcan.Graphics;
using Viewport = Silk.NET.Direct3D11.Viewport;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11CommandBuffer : ICommandBuffer
{
    internal ID3D11DeviceContext* Context { get; }
    public ID3D11RenderTargetView* RenderTargetView { get; set; }
    public ID3D11DepthStencilView* DepthStencilView { get; set; }
    private bool _recording;

    internal D3D11CommandBuffer(ID3D11DeviceContext* context)
    {
        Context = context;
    }

    public void Begin()
    {
        if (_recording)
            throw new InvalidOperationException("Command buffer is already recording.");

        _recording = true;

        var renderTargetView = RenderTargetView;

        Context->OMSetRenderTargets(1, &renderTargetView, DepthStencilView);
    }

    public void End()
    {
        if (!_recording)
            throw new InvalidOperationException("Command buffer is not recording.");

        _recording = false;
    }

    public void SetPipeline(IPipeline pipeline)
    {
        var d3dPipeline = (D3D11Pipeline)pipeline;
        d3dPipeline.Bind(Context);
    }

    public void SetVertexBuffer(IBuffer buffer)
    {
        var vertexBuffer = (D3D11Buffer)buffer;

        var handle = vertexBuffer.Handle;

        uint stride = 12;
        uint offset = 0;

        Context->IASetVertexBuffers(0, 1, &handle, &stride, &offset);
    }

    public void SetIndexBuffer(IBuffer buffer)
    {
        var indexBuffer = (D3D11Buffer)buffer;

        var handle = indexBuffer.Handle;

        Context->IASetIndexBuffer(handle, Format.FormatR32Uint, 0);
    }

    public void SetViewport(in IViewport viewport)
    {
        var d3dViewport = new Viewport
        {
            TopLeftX = viewport.X,
            TopLeftY = viewport.Y,
            Width = viewport.Width,
            Height = viewport.Height,
            MinDepth = viewport.MinDepth,
            MaxDepth = viewport.MaxDepth
        };

        Context->RSSetViewports(1, &d3dViewport);
    }

    public void SetScissor(in IScissorRect scissor)
    {
        var rect = new Silk.NET.Maths.Box2D<int>(
            scissor.X,
            scissor.Y,
            (int)(scissor.X + scissor.Width),
            (int)(scissor.Y + scissor.Height));

        Context->RSSetScissorRects(1, &rect);
    }

    public void ClearColor(float r, float g, float b, float a)
    {
        float[] color = new[] { r, g, b, a };

        fixed (float* colorPtr = color)
        {
            Context->ClearRenderTargetView(RenderTargetView, colorPtr);
        }
    }

    public void ClearDepth(float depth)
    {
        Context->ClearDepthStencilView(DepthStencilView, (uint)ClearFlag.Depth, depth, 0);
    }

    public void Draw(uint vertexCount, uint instanceCount = 1, uint firstVertex = 0, uint firstInstance = 0)
    {
        Context->DrawInstanced(vertexCount, instanceCount, firstVertex, firstInstance);
    }

    public void DrawIndexed(uint indexCount, uint instanceCount = 1, uint firstIndex = 0, int vertexOffset = 0, uint firstInstance = 0)
    {
        Context->DrawIndexedInstanced(indexCount, instanceCount, firstIndex, vertexOffset, firstInstance);
    }

    public void SetUniformBuffer(IBuffer buffer, uint slot = 0)
    {
        var uniformBuffer = (D3D11Buffer)buffer;
        var handle = uniformBuffer.Handle;

        Context->VSSetConstantBuffers(slot, 1, &handle);
    }

    public void Dispose()
    {
    }
}
