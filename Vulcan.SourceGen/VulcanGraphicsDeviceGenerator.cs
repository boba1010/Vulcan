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
            public void Initialize()
            {
                InitializeD3D();
                InitializeDevice();
                InitializeDevice5();
                InitializeFactory();
                InitializeContext4();
                InitializeWaitIdle();

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

            private void InitializeDevice5()
            {
                fixed (ID3D11Device5** device5 = &_device5)
                {
                    _device->QueryInterface(
                        SilkMarshal.GuidPtrOf<ID3D11Device5>(),
                        (void**)device5);
                }
            }

            private void InitializeContext4()
            {
                fixed (ID3D11DeviceContext4** context4 = &_context4)
                {
                    _context->QueryInterface(
                        SilkMarshal.GuidPtrOf<ID3D11DeviceContext4>(),
                        (void**)context4);
                }
            }

            private void InitializeWaitIdle()
            {
                var description = new QueryDesc
                {
                    Query = Query.Event
                };

                ID3D11Query* query = null;

                var result = _device->CreateQuery(
                    &description,
                    &query);

                if (result < 0)
                    throw new InvalidOperationException(
                        $"Failed to create D3D11 wait-idle query. HRESULT: 0x{result:X8}");

                _waitIdleQuery = query;
            }

            private void InitializeFactory()
            {
                _factory = DXGI.GetApi(_window)
                    .CreateDXGIFactory2<IDXGIFactory2>(0);
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