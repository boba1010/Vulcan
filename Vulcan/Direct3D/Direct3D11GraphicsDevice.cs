using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;
using System.Runtime.InteropServices;
using Vulcan.Graphics;
using Vulcan.Graphics.Descriptions;
using CullMode = Vulcan.Graphics.CullMode;
using FillMode = Vulcan.Graphics.FillMode;
using Filter = Vulcan.Graphics.Filter;

namespace Vulcan.DirectX;

[VulcanGraphicsDevice(Backend = GraphicsBackend.D3D11, Debug = true)]
public unsafe partial class Direct3D11GraphicsDevice(IWindow window) : IGraphicsDevice
{
    private readonly IWindow _window = window;
    private D3D11? _d3d = null;
    private ID3D11Device* _device;
    private ID3D11Device5* _device5;
    private ID3D11DeviceContext* _context;
    private ComPtr<IDXGIFactory2> _factory;
    private ID3D11Query* _waitIdleQuery;
    private ID3D11DeviceContext4* _context4;

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

        return new D3D11Buffer(buffer, _context);
    }

    private Format GetFormat(TextureFormat format)
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

    private uint GetBindFlags(TextureUsage usage)
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

        ID3D11DepthStencilView* depthStencilView = null;

        if (description.Usage == TextureUsage.DepthStencil)
        {
            result = _device->CreateDepthStencilView((ID3D11Resource*)texture, null, &depthStencilView);

            if (result < 0)
            {
                texture->Release();
                throw new InvalidOperationException($"Failed to create D3D11 depth-stencil view. HRESULT: 0x{result:X8}");
            }
        }

        return new D3D11Texture((ID3D11Resource*)texture, depthStencilView);
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

    private static Silk.NET.Direct3D11.Filter GetFilter(Filter min, Filter mag, Filter mip)
    {
        return (min, mag, mip) switch
        {
            (Filter.Nearest, Filter.Nearest, Filter.Nearest)
                => Silk.NET.Direct3D11.Filter.MinMagMipPoint,

            (Filter.Nearest, Filter.Nearest, Filter.Linear)
                => Silk.NET.Direct3D11.Filter.MinMagPointMipLinear,

            (Filter.Nearest, Filter.Linear, Filter.Nearest)
                => Silk.NET.Direct3D11.Filter.MinPointMagLinearMipPoint,

            (Filter.Nearest, Filter.Linear, Filter.Linear)
                => Silk.NET.Direct3D11.Filter.MinPointMagMipLinear,

            (Filter.Linear, Filter.Nearest, Filter.Nearest)
                => Silk.NET.Direct3D11.Filter.MinLinearMagMipPoint,

            (Filter.Linear, Filter.Nearest, Filter.Linear)
                => Silk.NET.Direct3D11.Filter.MinLinearMagPointMipLinear,

            (Filter.Linear, Filter.Linear, Filter.Nearest)
                => Silk.NET.Direct3D11.Filter.MinMagLinearMipPoint,

            (Filter.Linear, Filter.Linear, Filter.Linear)
                => Silk.NET.Direct3D11.Filter.MinMagMipLinear,

            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static TextureAddressMode GetAddressMode(AddressMode mode)
    {
        return mode switch
        {
            AddressMode.Repeat => TextureAddressMode.Wrap,
            AddressMode.MirroredRepeat => TextureAddressMode.Mirror,
            AddressMode.ClampToEdge => TextureAddressMode.Clamp,
            AddressMode.ClampToBorder => TextureAddressMode.Border,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    public ISampler CreateSampler(in SamplerDescription description)
    {
        var desc = new SamplerDesc
        {
            Filter = GetFilter(
                description.MinFilter,
                description.MagFilter,
                description.MipmapFilter),

            AddressU = GetAddressMode(description.AddressU),
            AddressV = GetAddressMode(description.AddressV),
            AddressW = GetAddressMode(description.AddressW),

            MipLODBias = description.MipLodBias,
            MinLOD = description.MinLod,
            MaxLOD = description.MaxLod
        };

        ID3D11SamplerState* sampler = null;

        var result = _device->CreateSamplerState(&desc, &sampler);

        if (result < 0)
            throw new InvalidOperationException(
                $"Failed to create D3D11 sampler. HRESULT: 0x{result:X8}");

        return new D3D11Sampler(sampler);
    }

    public IShader CreateShader(in ShaderDescription description)
    {
        if (description.Code.IsEmpty)
            throw new ArgumentException("Shader code cannot be empty.", nameof(description));


        var code = description.Code.Span;

        fixed (byte* codePtr = code)
        {
            switch (description.Stage)
            {
                case ShaderStage.Vertex:
                    {
                        ID3D11VertexShader* shader = null;

                        var result = _device->CreateVertexShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException($"Failed to create D3D11 vertex shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.Vertex, [.. code]);
                    }

                case ShaderStage.Fragment:
                    {
                        ID3D11PixelShader* shader = null;

                        var result = _device->CreatePixelShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException(
                                $"Failed to create D3D11 pixel shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.Fragment, [.. code]);
                    }

                case ShaderStage.Geometry:
                    {
                        ID3D11GeometryShader* shader = null;

                        var result = _device->CreateGeometryShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException(
                                $"Failed to create D3D11 geometry shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.Geometry, [.. code]);
                    }

                case ShaderStage.TessellationControl:
                    {
                        ID3D11HullShader* shader = null;

                        var result = _device->CreateHullShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException(
                                $"Failed to create D3D11 hull shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.TessellationControl, [.. code]);
                    }

                case ShaderStage.TessellationEvaluation:
                    {
                        ID3D11DomainShader* shader = null;

                        var result = _device->CreateDomainShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException(
                                $"Failed to create D3D11 domain shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.TessellationEvaluation, [.. code]);
                    }

                case ShaderStage.Compute:
                    {
                        ID3D11ComputeShader* shader = null;

                        var result = _device->CreateComputeShader(
                            codePtr,
                            (nuint)code.Length,
                            null,
                            &shader);

                        if (result < 0)
                            throw new InvalidOperationException(
                                $"Failed to create D3D11 compute shader. HRESULT: 0x{result:X8}");

                        return new D3D11Shader((ID3D11DeviceChild*)shader, ShaderStage.Compute, [.. code]);
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(description.Stage));
            }
        }
    }

    public IVertexLayout CreateVertexLayout(in VertexLayoutDescription description)
    {
        var elements = description.Elements.Span;

        var nativeElements = stackalloc InputElementDesc[elements.Length];
        var semanticPointers = new nint[elements.Length];

        for (var i = 0; i < elements.Length; i++)
        {
            var element = elements[i];

            semanticPointers[i] =
                Marshal.StringToCoTaskMemUTF8(element.Semantic);

            nativeElements[i] = new InputElementDesc
            {
                SemanticName = (byte*)semanticPointers[i],
                SemanticIndex = element.Location,
                Format = GetFormat(element.Format),
                InputSlot = 0,
                AlignedByteOffset = element.Offset,
                InputSlotClass = InputClassification.PerVertexData,
                InstanceDataStepRate = 0
            };
        }

        var shader = (D3D11Shader)description.VertexShader;
        ID3D11InputLayout* layout = null;

        try
        {
            fixed (byte* shaderCode = shader.Code)
            {
                var result = _device->CreateInputLayout(
                    nativeElements,
                    (uint)elements.Length,
                    shaderCode,
                    (nuint)shader.Code.Length,
                    &layout);

                if (result < 0)
                    throw new InvalidOperationException(
                        $"Failed to create D3D11 input layout. HRESULT: 0x{result:X8}");
            }

            return new D3D11VertexLayout(layout);
        }
        finally
        {
            foreach (var pointer in semanticPointers)
                Marshal.FreeCoTaskMem(pointer);
        }
    }

    public IRasterizerState CreateRasterizerState(in RasterizerDescription description)
    {
        var desc = new RasterizerDesc
        {
            CullMode = description.CullMode switch
            {
                CullMode.None => Silk.NET.Direct3D11.CullMode.None,
                CullMode.Front => Silk.NET.Direct3D11.CullMode.Front,
                CullMode.Back => Silk.NET.Direct3D11.CullMode.Back,
                _ => throw new ArgumentOutOfRangeException()
            },

            FillMode = description.FillMode switch
            {
                FillMode.Solid => Silk.NET.Direct3D11.FillMode.Solid,
                FillMode.Wireframe => Silk.NET.Direct3D11.FillMode.Wireframe,
                _ => throw new ArgumentOutOfRangeException()
            },

            FrontCounterClockwise =
                description.FrontFace == FrontFace.CounterClockwise,

            DepthClipEnable = description.DepthClipEnable
        };

        ID3D11RasterizerState* state = null;

        var result = _device->CreateRasterizerState(
            &desc,
            &state);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 rasterizer state. HRESULT: 0x{result:X8}");

        return new D3D11RasterizerState(state);
    }

    public IDepthStencilState CreateDepthStencilState(in DepthStencilDescription description)
    {
        var desc = new DepthStencilDesc
        {
            DepthEnable = description.DepthTestEnable,

            DepthWriteMask = description.DepthWriteEnable ? DepthWriteMask.All : DepthWriteMask.Zero,

            DepthFunc = description.DepthCompare switch
            {
                CompareOperation.Never => ComparisonFunc.Never,
                CompareOperation.Less => ComparisonFunc.Less,
                CompareOperation.Equal => ComparisonFunc.Equal,
                CompareOperation.LessOrEqual => ComparisonFunc.LessEqual,
                CompareOperation.Greater => ComparisonFunc.Greater,
                CompareOperation.NotEqual => ComparisonFunc.NotEqual,
                CompareOperation.GreaterOrEqual => ComparisonFunc.GreaterEqual,
                CompareOperation.Always => ComparisonFunc.Always,
                _ => throw new ArgumentOutOfRangeException()
            }
        };

        ID3D11DepthStencilState* state = null;

        var result = _device->CreateDepthStencilState(&desc, &state);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 depth-stencil state. HRESULT: 0x{result:X8}");

        return new D3D11DepthStencilState(state);
    }

    public IBlendState CreateBlendState(in BlendDescription description)
    {
        var desc = new BlendDesc
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = false
        };

        desc.RenderTarget[0] = new RenderTargetBlendDesc
        {
            BlendEnable = description.Enable,

            SrcBlend = description.SourceColor switch
            {
                BlendFactor.Zero => Blend.Zero,
                BlendFactor.One => Blend.One,
                BlendFactor.SourceColor => Blend.SrcColor,
                BlendFactor.InverseSourceColor => Blend.InvSrcColor,
                BlendFactor.DestinationColor => Blend.DestColor,
                BlendFactor.InverseDestinationColor => Blend.InvDestColor,
                BlendFactor.SourceAlpha => Blend.SrcAlpha,
                BlendFactor.InverseSourceAlpha => Blend.InvSrcAlpha,
                BlendFactor.DestinationAlpha => Blend.DestAlpha,
                BlendFactor.InverseDestinationAlpha => Blend.InvDestAlpha,
                _ => throw new ArgumentOutOfRangeException()
            },

            DestBlend = description.DestinationColor switch
            {
                BlendFactor.Zero => Blend.Zero,
                BlendFactor.One => Blend.One,
                BlendFactor.SourceColor => Blend.SrcColor,
                BlendFactor.InverseSourceColor => Blend.InvSrcColor,
                BlendFactor.DestinationColor => Blend.DestColor,
                BlendFactor.InverseDestinationColor => Blend.InvDestColor,
                BlendFactor.SourceAlpha => Blend.SrcAlpha,
                BlendFactor.InverseSourceAlpha => Blend.InvSrcAlpha,
                BlendFactor.DestinationAlpha => Blend.DestAlpha,
                BlendFactor.InverseDestinationAlpha => Blend.InvDestAlpha,
                _ => throw new ArgumentOutOfRangeException()
            },

            BlendOp = description.ColorOperation switch
            {
                BlendOperation.Add => BlendOp.Add,
                BlendOperation.Subtract => BlendOp.Subtract,
                BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
                BlendOperation.Min => BlendOp.Min,
                BlendOperation.Max => BlendOp.Max,
                _ => throw new ArgumentOutOfRangeException()
            },

            SrcBlendAlpha = description.SourceAlpha switch
            {
                BlendFactor.Zero => Blend.Zero,
                BlendFactor.One => Blend.One,
                BlendFactor.SourceColor => Blend.SrcColor,
                BlendFactor.InverseSourceColor => Blend.InvSrcColor,
                BlendFactor.DestinationColor => Blend.DestColor,
                BlendFactor.InverseDestinationColor => Blend.InvDestColor,
                BlendFactor.SourceAlpha => Blend.SrcAlpha,
                BlendFactor.InverseSourceAlpha => Blend.InvSrcAlpha,
                BlendFactor.DestinationAlpha => Blend.DestAlpha,
                BlendFactor.InverseDestinationAlpha => Blend.InvDestAlpha,
                _ => throw new ArgumentOutOfRangeException()
            },

            DestBlendAlpha = description.DestinationAlpha switch
            {
                BlendFactor.Zero => Blend.Zero,
                BlendFactor.One => Blend.One,
                BlendFactor.SourceColor => Blend.SrcColor,
                BlendFactor.InverseSourceColor => Blend.InvSrcColor,
                BlendFactor.DestinationColor => Blend.DestColor,
                BlendFactor.InverseDestinationColor => Blend.InvDestColor,
                BlendFactor.SourceAlpha => Blend.SrcAlpha,
                BlendFactor.InverseSourceAlpha => Blend.InvSrcAlpha,
                BlendFactor.DestinationAlpha => Blend.DestAlpha,
                BlendFactor.InverseDestinationAlpha => Blend.InvDestAlpha,
                _ => throw new ArgumentOutOfRangeException()
            },

            BlendOpAlpha = description.AlphaOperation switch
            {
                BlendOperation.Add => BlendOp.Add,
                BlendOperation.Subtract => BlendOp.Subtract,
                BlendOperation.ReverseSubtract => BlendOp.RevSubtract,
                BlendOperation.Min => BlendOp.Min,
                BlendOperation.Max => BlendOp.Max,
                _ => throw new ArgumentOutOfRangeException()
            },

            RenderTargetWriteMask = (byte)ColorWriteEnable.All
        };

        ID3D11BlendState* state = null;

        var result = _device->CreateBlendState(
            &desc,
            &state);

        if (result < 0)
            throw new InvalidOperationException(
                $"Failed to create D3D11 blend state. HRESULT: 0x{result:X8}");

        return new D3D11BlendState(state);
    }

    public IPipeline CreatePipeline(in PipelineDescription description)
    {
        var vertexShader = (D3D11Shader)description.VertexShader;
        var fragmentShader = (D3D11Shader)description.FragmentShader;
        var vertexLayout = (D3D11VertexLayout)description.VertexLayout;
        var rasterizer = (D3D11RasterizerState)description.Rasterizer;
        var depthStencil = (D3D11DepthStencilState)description.DepthStencil;
        var blend = (D3D11BlendState)description.Blend;

        var d3dPipeline = new D3D11Pipeline(
            vertexShader,
            fragmentShader,
            vertexLayout,
            rasterizer,
            depthStencil,
            blend,
            description.PrimitiveTopology);
            
        return d3dPipeline;
    }

    public ISwapchain CreateSwapchain(in SwapchainDescription description)
    {
        return new D3D11Swapchain(_device, _factory, _window, description);
    }

    public ICommandQueue CreateCommandQueue()
    {
        return new D3D11CommandQueue(_context4, _context);
    }

    public IFence CreateFence()
    {
        ID3D11Fence* fence = null;

        var result = _device5->CreateFence(
            0,
            0,
            SilkMarshal.GuidPtrOf<ID3D11Fence>(),
            (void**)&fence);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D3D11 fence. HRESULT: 0x{result:X8}");

        return new D3D11Fence(fence);
    }

    public ISemaphore CreateSemaphore()
    {
        throw new NotImplementedException();
    }

    public void WaitIdle()
    {
        _context->End((ID3D11Asynchronous*)_waitIdleQuery);
        _context->Flush();

        while (true)
        {
            var result = _context->GetData((ID3D11Asynchronous*)_waitIdleQuery, null, 0, 0);

            if (result == 0)
                return;

            if (result != 1 )
                throw new InvalidOperationException($"D3D11 WaitIdle failed. HRESULT: 0x{result:X8}");

            Thread.Yield();
        }
    }

    public void Dispose()
    {
        _factory.Dispose();

        if (_context is not null)
        {
            _context->Release();
            _context = null;
        }

        if (_device is not null)
        {
            _device->Release();
            _device = null;
        }

        _d3d?.Dispose();

        GC.SuppressFinalize(this);
    }
}
