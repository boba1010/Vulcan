using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Vulcan.SourceGen;

[Generator]
public sealed class VulcanGraphicsDeviceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource(
                "VulcanGraphicsDeviceAttribute.g.cs",
                """
                using System;

                namespace Vulcan;

                public enum GraphicsBackend
                {
                    D3D11,
                    Vulkan
                }

                [AttributeUsage(AttributeTargets.Class)]
                internal sealed class VulcanGraphicsDeviceAttribute : Attribute
                {
                    public GraphicsBackend Backend { get; set; }
                    public bool Debug { get; set; }
                }
                """);
        });

        var types = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (node, _) =>
                    node is ClassDeclarationSyntax,

                static (ctx, _) =>
                {
                    var declaration = (ClassDeclarationSyntax)ctx.Node;

                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(declaration);

                    if (symbol is INamedTypeSymbol type &&
                        type.GetAttributes().Any(a =>
                            a.AttributeClass?.Name == "VulcanGraphicsDeviceAttribute"))
                    {
                        return type;
                    }

                    return null;
                })
            .Where(static type => type is not null);

        context.RegisterSourceOutput(types, static (ctx, type) =>
        {
            Generate(ctx, type!);
        });
    }

    private static void Generate(SourceProductionContext context, INamedTypeSymbol type)
    {
        var attribute = type.GetAttributes()
            .First(a => a.AttributeClass?.Name == "VulcanGraphicsDeviceAttribute");

        var debug = attribute.NamedArguments
            .FirstOrDefault(x => x.Key == "Debug")
            .Value.Value is true;

        var backend = attribute.NamedArguments
            .FirstOrDefault(x => x.Key == "Backend")
            .Value.Value is GraphicsBackend value
                ? value
                : GraphicsBackend.D3D11;

        var source = backend == GraphicsBackend.D3D11 ? GenerateD3D11(type, debug) : GenerateVulkan(type, debug);

        context.AddSource($"{type.Name}.g.cs", source);
    }

    private static string GenerateD3D11(INamedTypeSymbol type, bool debug)
    {
        var source = $$""""
        using Silk.NET.Direct3D11;
        using Silk.NET.Core.Native;
        using Silk.NET.DXGI;
        using Silk.NET.Direct3D.Compilers;
        using Vulcan.Maths;
        using System.Numerics;

        namespace {{type.ContainingNamespace.ToDisplayString()}};

        unsafe partial class {{type.Name}} : IGraphicsDevice
        {
            private const uint RenderTargetOutput = 0x20;

            public void Initialize()
            {
                InitializeD3D();
                InitializeDevice();
                InitializeSwapChain();
                InitializeBackBuffer();
                InitializeRenderTarget();
                InitializeViewport();

                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D11 successfully");
            }

            private void InitializeD3D()
            {
                _d3d = D3D11.GetApi(_window);
                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D API successfully");
            }

            private void InitializeDevice()
            {
                fixed (ID3D11Device** device = &_device)
                fixed (ID3D11DeviceContext** context = &_context)
                {
                    _d3d.CreateDevice(
                        null,
                        D3DDriverType.Hardware,
                        default,
                        (uint)CreateDeviceFlag.BgraSupport,
                        null,
                        0,
                        D3D11.SdkVersion,
                        device,
                        null,
                        context);
                }

                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D device successfully");
            }

            private void InitializeSwapChain()
            {
                _factory = DXGI.GetApi(_window).CreateDXGIFactory2<IDXGIFactory2>(0);

                var desc = new SwapChainDesc1
                {
                    Width = (uint)_window.Size.X,
                    Height = (uint)_window.Size.Y,
                    Format = Format.FormatB8G8R8A8Unorm,
                    BufferCount = 2,
                    BufferUsage = RenderTargetOutput,
                    SampleDesc = new SampleDesc
                    {
                        Count = 1,
                        Quality = 0
                    },
                    SwapEffect = SwapEffect.FlipDiscard
                };

                fixed (IDXGISwapChain1** swapChain = &_swapChain)
                {
                    _factory.CreateSwapChainForHwnd(
                        (IUnknown*)_device,
                        _window.Native!.DXHandle!.Value,
                        &desc,
                        (SwapChainFullscreenDesc*)null,
                        (IDXGIOutput*)null,
                        swapChain);
                }

                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D Swapchain successfully");
            }

            private void InitializeBackBuffer()
            {
                fixed (ID3D11Texture2D** backBuffer = &_backBuffer)
                {
                    _swapChain->GetBuffer(
                        0,
                        SilkMarshal.GuidPtrOf<ID3D11Texture2D>(),
                        (void**)backBuffer);
                }

                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D backbuffer successfully");
            }

            private void InitializeRenderTarget()
            {
                fixed (ID3D11RenderTargetView** renderTargetView = &_renderTargetView)
                {
                    _device->CreateRenderTargetView(
                        (ID3D11Resource*)_backBuffer,
                        null,
                        renderTargetView);
                }

                if ({{debug.ToString().ToLower()}})
                    Console.WriteLine("Initialized D3D render target successfully");
            }

            private void InitializeViewport()
            {
                _viewport = new Viewport
                {
                    TopLeftX = 0,
                    TopLeftY = 0,
                    Width = _window.Size.X,
                    Height = _window.Size.Y,
                    MinDepth = 0,
                    MaxDepth = 1
                };

                fixed (Viewport* viewport = &_viewport)
                {
                    _context->RSSetViewports(1, viewport);
                }
            }
        }
        """";

        return source;
    }

    private static string GenerateVulkan(INamedTypeSymbol type, bool debug)
    {
        return "";
    }
}