using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;

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
    private ID3D11VertexShader* _vertexShader;
    private ID3D11PixelShader* _pixelShader;
    private ID3D11InputLayout* _inputLayout;
    private Viewport _viewport;

    private const string ShaderSource = @"
        struct VSInput
        {
            float3 Position : POSITION;
            float4 Color : COLOR;
        };

        struct VSOutput
        {
            float4 Position : SV_POSITION;
            float4 Color : COLOR;
        };

        VSOutput vs_main(VSInput input)
        {
            VSOutput output;
            output.Position = float4(input.Position, 1);
            output.Color = input.Color;
            return output;
        }

        float4 ps_main(VSOutput input) : SV_TARGET
        {
            return input.Color;
        }
    ";

    public void Dispose()
    {
        _d3d?.Dispose();
    }

    public IBuffer CreateBuffer(in BufferDescription description)
    {
        if (description.Size > uint.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(description), "D3D11 buffers cannot exceed 4 GB.");

        var desc = new BufferDesc
        {
            ByteWidth = (uint)description.Size,
            Usage = description.MemoryUsage switch
            {
                MemoryUsage.DeviceLocal => Silk.NET.Direct3D11.Usage.Default,
                MemoryUsage.Upload => Silk.NET.Direct3D11.Usage.Dynamic,
                MemoryUsage.Readback => Silk.NET.Direct3D11.Usage.Staging,
                _ => throw new ArgumentOutOfRangeException()
            },
            BindFlags = description.Usage switch
            {
                BufferUsage.Vertex => (uint)BindFlag.VertexBuffer,
                BufferUsage.Index => (uint)BindFlag.IndexBuffer,
                BufferUsage.Uniform => (uint)BindFlag.ConstantBuffer,
                BufferUsage.Storage => (uint)BindFlag.UnorderedAccess,
                BufferUsage.Indirect => 0,
                BufferUsage.CopySource => 0,
                BufferUsage.CopyDestination => 0,
                _ => throw new ArgumentOutOfRangeException()
            },
            CPUAccessFlags = description.MemoryUsage switch
            {
                MemoryUsage.Upload => (uint)CpuAccessFlag.Write,
                MemoryUsage.Readback => (uint)CpuAccessFlag.Read,
                _ => 0
            },
            MiscFlags = description.Usage == BufferUsage.Indirect
            ? (uint)ResourceMiscFlag.DrawindirectArgs
            : 0
        };

        ID3D11Buffer* buffer = null;

        var result = _device->CreateBuffer(
            &desc,
            null,
            &buffer);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 buffer. HRESULT: 0x{result:X8}");

        return new D3D11Buffer(buffer);
    }

    private static Silk.NET.DXGI.Format GetFormat(TextureFormat format)
    {
        return format switch
        {
            TextureFormat.R8Unorm => Silk.NET.DXGI.Format.FormatR8Unorm,
            TextureFormat.R8G8Unorm => Silk.NET.DXGI.Format.FormatR8G8Unorm,
            TextureFormat.R8G8B8A8Unorm => Silk.NET.DXGI.Format.FormatR8G8B8A8Unorm,
            TextureFormat.R8G8B8A8Srgb => Silk.NET.DXGI.Format.FormatR8G8B8A8UnormSrgb,

            TextureFormat.R16Float => Silk.NET.DXGI.Format.FormatR16Float,
            TextureFormat.R16G16Float => Silk.NET.DXGI.Format.FormatR16G16Float,
            TextureFormat.R16G16B16A16Float => Silk.NET.DXGI.Format.FormatR16G16B16A16Float,

            TextureFormat.R32Float => Silk.NET.DXGI.Format.FormatR32Float,
            TextureFormat.R32G32Float => Silk.NET.DXGI.Format.FormatR32G32Float,
            TextureFormat.R32G32B32Float => Silk.NET.DXGI.Format.FormatR32G32B32Float,
            TextureFormat.R32G32B32A32Float => Silk.NET.DXGI.Format.FormatR32G32B32A32Float,

            TextureFormat.D16Unorm => Silk.NET.DXGI.Format.FormatD16Unorm,
            TextureFormat.D24UnormS8Uint => Silk.NET.DXGI.Format.FormatD24UnormS8Uint,
            TextureFormat.D32Float => Silk.NET.DXGI.Format.FormatD32Float,
            TextureFormat.D32FloatS8Uint => Silk.NET.DXGI.Format.FormatD32FloatS8X24Uint,

            TextureFormat.Unknown => throw new ArgumentException(
                "Texture format cannot be Unknown.", nameof(format)),

            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private static uint GetBindFlags(TextureUsage usage)
    {
        return usage switch
        {
            TextureUsage.Sampled => (uint)BindFlag.ShaderResource,
            TextureUsage.Storage => (uint)BindFlag.UnorderedAccess,
            TextureUsage.RenderTarget => (uint)BindFlag.RenderTarget,
            TextureUsage.DepthStencil => (uint)BindFlag.DepthStencil,
            TextureUsage.CopySource => 0,
            TextureUsage.CopyDestination => 0,
            _ => throw new ArgumentOutOfRangeException(nameof(usage))
        };
    }

    private ITexture CreateTexture1D(in TextureDescription description)
    {
        if (description.Type != TextureType.Texture1D)
            throw new InvalidDataException($"Texture type {description.Type} is invalid in this context.");

        var desc = new Texture1DDesc
        {
            Width = description.Width,
            MipLevels = description.MipLevels,
            ArraySize = description.ArrayLayers,
            Format = GetFormat(description.Format),
            Usage = Usage.Default,
            BindFlags = GetBindFlags(description.Usage)
        };

        ID3D11Texture1D* texture = null;

        var result = _device->CreateTexture1D(&desc, null, &texture);

        if (result < 0)
            throw new InvalidOperationException(
                $"Failed to create D3D11 texture. HRESULT: 0x{result:X8}");

        return new D3D11Texture((ID3D11Resource*)texture);
    }

    private ITexture CreateTexture2D(in TextureDescription description)
    {
        if (description.Type != TextureType.Texture2D)
            throw new InvalidDataException($"Texture type {description.Type} is invalid in this context.");

        var desc = new Texture2DDesc
        {
            Width = description.Width,
            Height = description.Height,
            MipLevels = description.MipLevels,
            ArraySize = description.ArrayLayers,
            Format = GetFormat(description.Format),
            SampleDesc = new SampleDesc
            {
                Count = (uint)description.Samples,
                Quality = 0
            },
            Usage = Usage.Default,
            BindFlags = GetBindFlags(description.Usage)
        };

        ID3D11Texture2D* texture = null;

        var result = _device->CreateTexture2D(&desc, null, &texture);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 texture. HRESULT: 0x{result:X8}");

        return new D3D11Texture((ID3D11Resource*)texture);
    }

    private ITexture CreateTexture3D(in TextureDescription description)
    {
        if (description.Type != TextureType.Texture2D)
            throw new InvalidDataException($"Texture type {description.Type} is invalid in this context.");

        var desc = new Texture3DDesc
        {
            Width = description.Width,
            Height = description.Height,
            Depth = description.Depth,
            MipLevels = description.MipLevels,
            Format = GetFormat(description.Format),
            Usage = Usage.Default,
            BindFlags = GetBindFlags(description.Usage)
        };

        ID3D11Texture3D* texture = null;

        var result = _device->CreateTexture3D(&desc, null, &texture);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 texture. HRESULT: 0x{result:X8}");

        return new D3D11Texture((ID3D11Resource*)texture);
    }

    private ITexture CreateTextureCube(in TextureDescription description)
    {
        if (description.Type != TextureType.Cube)
            throw new InvalidDataException($"Texture type {description.Type} is invalid in this context.");

        var desc = new Texture2DDesc
        {
            Width = description.Width,
            Height = description.Height,
            MipLevels = description.MipLevels,
            ArraySize = 6,
            Format = GetFormat(description.Format),
            SampleDesc = new SampleDesc
            {
                Count = 1,
                Quality = 0
            },
            Usage = Usage.Default,
            BindFlags = GetBindFlags(description.Usage),
            MiscFlags = (uint)ResourceMiscFlag.Texturecube
        };

        ID3D11Texture2D* texture = null;

        var result = _device->CreateTexture2D(&desc, null, &texture);

        if (result < 0)
            throw new InvalidOperationException(
                $"Failed to create D3D11 cube texture. HRESULT: 0x{result:X8}");

        return new D3D11Texture((ID3D11Resource*)texture);
    }
    
    public ITexture CreateTexture(in TextureDescription description)
    {
        return description.Type switch
        {
            TextureType.Texture1D => CreateTexture1D(description),
            TextureType.Texture2D => CreateTexture2D(description),
            TextureType.Texture3D => CreateTexture3D(description),
            TextureType.Cube => CreateTextureCube(description),
            _ => throw new ArgumentOutOfRangeException(nameof(description))
        };
    }

    public ISampler CreateSampler(in SamplerDescription description)
    {
        throw new NotImplementedException();
    }

    public IShader CreateShader(in ShaderDescription description)
    {
        throw new NotImplementedException();
    }

    public IPipeline CreatePipeline(in PipelineDescription description)
    {
        throw new NotImplementedException();
    }

    public ISwapchain CreateSwapchain(in SwapchainDescription description)
    {
        throw new NotImplementedException();
    }

    public ICommandBuffer CreateCommandBuffer()
    {
        throw new NotImplementedException();
    }

    public ICommandQueue CreateCommandQueue()
    {
        throw new NotImplementedException();
    }

    public IFence CreateFence()
    {
        throw new NotImplementedException();
    }

    public ISemaphore CreateSemaphore()
    {
        throw new NotImplementedException();
    }

    public void WaitIdle()
    {
        throw new NotImplementedException();
    }
}
