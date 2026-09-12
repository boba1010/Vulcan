using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;
using System.Numerics;
using Vulcan.Maths;

namespace Vulcan.DirectX;

[VulcanGraphicsDevice(Backend = GraphicsBackend.D3D11, Debug = true)]
public unsafe partial class Direct3D11GraphicsDevice(IWindow window) : IGraphicsDevice
{
    private readonly IWindow _window = window;
    private D3D11? _d3d = null;
    private ID3D11Device* _device;
    private ID3D11DeviceContext* _context;
    private ComPtr<IDXGIFactory2> _factory;
    private IDXGISwapChain1* _swapChain;
    private ID3D11Texture2D* _backBuffer;
    private ID3D11RenderTargetView* _renderTargetView;
    private ID3D11Buffer* _vertexBuffer;
    private D3DCompiler? _compiler = null;
    private ID3D11VertexShader* _vertexShader;
    private ID3D11PixelShader* _pixelShader;
    private ID3D11InputLayout* _inputLayout;
    private Viewport _viewport;

    public void Clear()
    {
        fixed (ID3D11RenderTargetView** pRenderTarget = &_renderTargetView)
        {
            _context->OMSetRenderTargets(1, pRenderTarget, null);
        }

        var color = new float[] { 1f, 1f, 1f, 1f };

        fixed (float* pColor = color)
        {
            _context->ClearRenderTargetView(_renderTargetView, pColor);
        }
    }

    public void Present()
    {
        _swapChain->Present(1, 0);
    }

    private ID3D10Blob* CompileShader(string source, string entryPoint, string target)
    {
        ID3D10Blob* blob = null;
        ID3D10Blob* errors = null;

        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(source);

        fixed (byte* pSource = sourceBytes)
        {
            _compiler?.Compile(
                pSource,
                (nuint)sourceBytes.Length,
                (string)null!,
                null,
                null,
                entryPoint,
                target,
                0,
                0,
                &blob,
                &errors);
        }

        if (errors != null)
            Console.WriteLine(SilkMarshal.PtrToString((nint)errors->GetBufferPointer()));

        return blob;
    }

    public void Dispose()
    {
        _d3d?.Dispose();
    }

    public void DrawVertices(Vertex[] vertices, D3DPrimitiveTopology topology)
    {
        MappedSubresource mapped = default;

        _context->Map(
            (ID3D11Resource*)_vertexBuffer,
            0,
            Map.WriteDiscard,
            0,
            &mapped);

        fixed (Vertex* data = vertices)
        {
            System.Buffer.MemoryCopy(
                data,
                mapped.PData,
                sizeof(Vertex) * vertices.Length,
                sizeof(Vertex) * vertices.Length);
        }

        _context->Unmap((ID3D11Resource*)_vertexBuffer, 0);

        _context->IASetInputLayout(_inputLayout);

        _context->IASetPrimitiveTopology(topology);

        _context->VSSetShader(_vertexShader, null, 0);
        _context->PSSetShader(_pixelShader, null, 0);

        uint stride = (uint)sizeof(Vertex);
        uint offset = 0;

        fixed (ID3D11Buffer** vertexBuffer = &_vertexBuffer)
        {
            _context->IASetVertexBuffers(
                0,
                1,
                vertexBuffer,
                &stride,
                &offset);
        }

        _context->Draw((uint)vertices.Length, 0);
    }

    public void DrawPoint(Vector2 position, Vector4 color)
    {
        var vertex = new Vertex
        {
            Position = position,
            Color = color
        };

        MappedSubresource mapped = default;

        _context->IASetInputLayout(_inputLayout);

        _context->Map(
            (ID3D11Resource*)_vertexBuffer,
            0,
            Map.WriteDiscard,
            0,
            &mapped);

        *(Vertex*)mapped.PData = vertex;

        _context->Unmap((ID3D11Resource*)_vertexBuffer, 0);

        _context->IASetPrimitiveTopology(
            D3DPrimitiveTopology.D3D10PrimitiveTopologyPointlist);

        _context->VSSetShader(_vertexShader, null, 0);
        _context->PSSetShader(_pixelShader, null, 0);

        uint stride = (uint)sizeof(Vertex);
        uint offset = 0;

        fixed (ID3D11Buffer** vertexBuffer = &_vertexBuffer)
        {
            _context->IASetVertexBuffers(
                0,
                1,
                vertexBuffer,
                &stride,
                &offset);
        }

        _context->Draw(1, 0);
    }

    public void DrawLine(Vector2 start, Vector2 end, Vector4 color)
    {
        var vertices = new Vertex[]
        {
            new() { Position = start, Color = color },
            new() { Position = end, Color = color }
        };

        DrawVertices(vertices, D3DPrimitiveTopology.D3D10PrimitiveTopologyLinelist);
    }

    public void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 color)
    {
        var vertices = new Vertex[]
        {
            new() { Position = a, Color = color },
            new() { Position = c, Color = color },
            new() { Position = b, Color = color },
        };

        DrawVertices(vertices, D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);
    }

    public void DrawRectangle(Vector2 position, Vector2 size, Vector4 color)
    {
        float x = position.X;
        float y = position.Y;
        float width = size.X;
        float height = size.Y;

        var vertices = new Vertex[]
        {
            // Triangle 1
            new() { Position = new(x, y), Color = color },
            new() { Position = new(x, y + height), Color = color },
            new() { Position = new(x + width, y), Color = color },

            // Triangle 2
            new() { Position = new(x, y + height), Color = color },
            new() { Position = new(x + width, y + height), Color = color },
            new() { Position = new(x + width, y), Color = color }
        };

        DrawVertices(vertices, D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);
    }

    public void DrawCircle(Vector2 center, float radius, Vector4 color, int segments = 64)
    {
        var vertices = new Vertex[segments * 3];

        float step = MathF.Tau / segments;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * step;
            float angle2 = (i + 1) * step;
            float aspect = (float)_window.Size.X / _window.Size.Y;

            vertices[i * 3] = new Vertex
            {
                Position = center,
                Color = color
            };

            vertices[i * 3 + 1] = new Vertex
            {
                Position = center + new Vector2(
                    MathF.Cos(angle2) * radius / aspect,
                    MathF.Sin(angle2) * radius),
                Color = color
            };

            vertices[i * 3 + 2] = new Vertex
            {
                Position = center + new Vector2(
                    MathF.Cos(angle1) * radius / aspect,
                    MathF.Sin(angle1) * radius),
                Color = color
            };
        }

        DrawVertices(vertices, D3DPrimitiveTopology.D3D10PrimitiveTopologyTrianglelist);
    }

    public void DrawPoint(Vector3 position)
    {
        throw new NotImplementedException();
    }

    public void DrawLine(Vector3 start, Vector3 end)
    {
        throw new NotImplementedException();
    }

    public void DrawTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        throw new NotImplementedException();
    }

    public void DrawCube(Vector3 position, Vector3 size)
    {
        throw new NotImplementedException();
    }

    public void DrawSphere(Vector3 position, float radius)
    {
        throw new NotImplementedException();
    }

    public void DrawPlane(Vector3 position, Vector2 size)
    {
        throw new NotImplementedException();
    }
}
