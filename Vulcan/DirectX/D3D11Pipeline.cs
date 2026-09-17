using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Pipeline : IPipeline
{
    internal D3D11Shader VertexShader;
    internal D3D11Shader FragmentShader;
    internal D3D11VertexLayout VertexLayout;
    internal D3D11RasterizerState Rasterizer;
    internal D3D11DepthStencilState DepthStencil;
    internal D3D11BlendState Blend;
    internal PrimitiveTopology PrimitiveTopology;

    internal D3D11Pipeline(
        D3D11Shader vertexShader,
        D3D11Shader fragmentShader,
        D3D11VertexLayout vertexLayout,
        D3D11RasterizerState rasterizer,
        D3D11DepthStencilState depthStencil,
        D3D11BlendState blend,
        PrimitiveTopology primitiveTopology)
    {
        VertexShader = vertexShader;
        FragmentShader = fragmentShader;
        VertexLayout = vertexLayout;
        Rasterizer = rasterizer;
        DepthStencil = depthStencil;
        Blend = blend;
        PrimitiveTopology = primitiveTopology;
    }

    internal void Bind(ID3D11DeviceContext* context)
    {
        context->VSSetShader(
            (ID3D11VertexShader*)VertexShader.Handle,
            null,
            0);

        context->PSSetShader(
            (ID3D11PixelShader*)FragmentShader.Handle,
            null,
            0);

        context->IASetInputLayout(
            VertexLayout.Handle);

        context->IASetPrimitiveTopology(
            PrimitiveTopology switch
            {
                PrimitiveTopology.PointList =>
                    D3DPrimitiveTopology.D3D10PrimitiveTopologyPointlist,

                PrimitiveTopology.LineList =>
                    D3DPrimitiveTopology.D3D10PrimitiveTopologyLinelist,

                PrimitiveTopology.LineStrip =>
                    D3DPrimitiveTopology.D3D10PrimitiveTopologyLinestrip,

                PrimitiveTopology.TriangleList =>
                    D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist,

                PrimitiveTopology.TriangleStrip =>
                    D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglestrip,

                _ => throw new ArgumentOutOfRangeException()
            });

        context->RSSetState(
            Rasterizer.Handle);

        context->OMSetDepthStencilState(
            DepthStencil.Handle,
            0);

        context->OMSetBlendState(
            Blend.Handle,
            null,
            uint.MaxValue);
    }

    public void Dispose()
    {
    }
}
