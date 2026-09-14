# Vulcan

**A low-level, cross-platform graphics API for .NET, built on top of Silk.NET.**

Vulcan provides a unified graphics abstraction over native graphics APIs such as **Direct3D 11** and **Vulkan**, allowing higher-level projects to work with graphics resources without being tightly coupled to a specific backend.

> 🚧 **Vulcan is currently in active development. APIs may change.**

## Table of Contents

- [Architecture](#architecture)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
- [Core Concepts](#core-concepts)
  - [IGraphicsDevice](#igraphicsdevice)
  - [Resource Descriptions](#resource-descriptions)
- [Backends](#backends)
- [Source Generation](#source-generation)
- [Design Goals](#design-goals)
- [Why Vulcan?](#why-vulcan)
- [Dependencies](#dependencies)
- [Project Structure](#project-structure)
- [Status](#status)
- [License](#license)

## Architecture

```text
Application / Engine
        │
        ▼
     Vulcan
        │
        ▼
     Silk.NET
        │
   ┌────┴─────┐
   ▼          ▼
D3D11      Vulkan
   │          │
   └────┬─────┘
        ▼
       GPU
```

Vulcan intentionally stays **low-level**. It provides graphics-device and resource abstractions rather than becoming a high-level rendering engine. Your application (or a rendering engine built on top of Vulcan) talks to a single `IGraphicsDevice` interface; Vulcan is responsible for routing that call to the correct native backend at runtime.

## Requirements

* **.NET 10.0 SDK**
* **Windows** for the Direct3D 11 backend
* **Linux or Android** for the Vulkan backend (see [platform selection](#platform-selection) below)
* A GPU and drivers supporting Direct3D 11 or Vulkan, respectively

## Getting Started

Clone the repository and open `Vulcan.slnx` in your IDE, or build from the command line:

```bash
git clone <repository-url>
cd Vulcan
dotnet build
```

The solution (`Vulcan.slnx`) contains two projects:

| Project             | TFM               | Purpose                                                            |
| ------------------- | ----------------- | ------------------------------------------------------------------|
| `Vulcan`             | `net10.0`          | The graphics API itself — public interfaces and backend implementations |
| `Vulcan.SourceGen`   | `netstandard2.0`   | A Roslyn incremental generator, referenced by `Vulcan` as a build-time analyzer |

### Creating a device

The simplest way to get an `IGraphicsDevice` is through the static `Vulcan.CreateDevice` factory, which picks the correct backend for the current OS:

```csharp
using Silk.NET.Windowing;
using Vulcan;

var window = Window.Create(WindowOptions.Default);
Window.Run();

IGraphicsDevice device = Vulcan.CreateDevice(window);
device.Initialize();
```

#### Platform selection

`Vulcan.CreateDevice` currently resolves the backend like this:

| OS        | Backend used                |
| --------- | ---------------------------- |
| Windows   | `Direct3D11GraphicsDevice`   |
| Linux     | `VulkanGraphicsDevice`       |
| Android   | `VulkanGraphicsDevice`       |
| Other     | throws `NotSupportedException` |

You can also construct a specific backend's `IGraphicsDevice` implementation directly (e.g. `Direct3D11GraphicsDevice` or `VulkanGraphicsDevice`) if you want to bypass OS auto-detection.

## Core Concepts

### IGraphicsDevice

`IGraphicsDevice` is the central abstraction. Every backend implements it, and it's the only type most consumers need to hold a reference to:

```csharp
public interface IGraphicsDevice : IDisposable
{
    void Initialize();

    IBuffer CreateBuffer(in BufferDescription description);
    ITexture CreateTexture(in TextureDescription description);
    ISampler CreateSampler(in SamplerDescription description);
    IShader CreateShader(in ShaderDescription description);
    IPipeline CreatePipeline(in PipelineDescription description);

    ISwapchain CreateSwapchain(in SwapchainDescription description);

    ICommandBuffer CreateCommandBuffer();
    ICommandQueue CreateCommandQueue();

    IFence CreateFence();
    ISemaphore CreateSemaphore();

    void WaitIdle();
}
```

Resources are created by describing what you want (a `*Description` struct) and letting the device translate that into the underlying native object. Every resource type has a matching interface (`IBuffer`, `ITexture`, `ISampler`, `IShader`, `IPipeline`, `ISwapchain`, `ICommandBuffer`, `ICommandQueue`, `IFence`, `ISemaphore`) so that application code never needs to reference `Silk.NET.Direct3D11` or `Silk.NET.Vulkan` types directly.

### Resource Descriptions

Descriptions are plain, backend-agnostic `readonly struct`s living in `Vulcan.Graphics.Descriptions`. They're immutable value types constructed with object initializers, and the device implementation maps them onto the native API's equivalent objects.

**Buffers:**

```csharp
using var buffer = device.CreateBuffer(new BufferDescription
{
    Size = 1024 * 1024,
    Usage = BufferUsage.Vertex,
    MemoryUsage = MemoryUsage.DeviceLocal
});
```

`BufferUsage` covers `Vertex`, `Index`, `Uniform`, `Storage`, `Indirect`, `CopySource`, and `CopyDestination`. `MemoryUsage` covers `DeviceLocal`, `Upload`, and `Readback`.

**Textures:**

```csharp
using var texture = device.CreateTexture(new TextureDescription
{
    Width = 512,
    Height = 512,
    MipLevels = 1,
    ArrayLayers = 1,
    Format = TextureFormat.R8G8B8A8Unorm,
    Usage = TextureUsage.Sampled,
    Type = TextureType.Texture2D,
    Samples = SampleCount.X1
});
```

`TextureFormat` includes unorm/srgb color formats (`R8Unorm` through `R32G32B32A32Float`) and depth/stencil formats (`D16Unorm`, `D24UnormS8Uint`, `D32Float`, `D32FloatS8Uint`). `TextureType` supports `Texture1D`, `Texture2D`, `Texture3D`, and `Cube`. `SampleCount` supports `X1`, `X2`, `X4`, `X8`, and `X16` for multisampling.

**Samplers:**

```csharp
using var sampler = device.CreateSampler(new SamplerDescription
{
    MinFilter = Filter.Linear,
    MagFilter = Filter.Linear,
    MipmapFilter = Filter.Linear,
    AddressU = AddressMode.Repeat,
    AddressV = AddressMode.Repeat,
    AddressW = AddressMode.ClampToEdge,
    MinLod = 0f,
    MaxLod = 1000f
});
```

**Shaders:**

```csharp
using var shader = device.CreateShader(new ShaderDescription
{
    Code = shaderBytecode,
    Stage = ShaderStage.Vertex,
    EntryPoint = "main"
});
```

`ShaderStage` covers `Vertex`, `Fragment`, `Geometry`, `TessellationControl`, `TessellationEvaluation`, and `Compute`.

**Pipelines** tie shaders, vertex layout, and fixed-function state together:

```csharp
using var pipeline = device.CreatePipeline(new PipelineDescription
{
    VertexShader = vertexShader,
    FragmentShader = fragmentShader,
    VertexLayout = vertexLayout,
    PrimitiveTopology = PrimitiveTopology.TriangleList,
    Rasterizer = rasterizerState,
    DepthStencil = depthStencilState,
    Blend = blendState
});
```

**Swapchains** connect a device to a window surface:

```csharp
using var swapchain = device.CreateSwapchain(new SwapchainDescription
{
    WindowHandle = window.Native!.Win32!.Value.Hwnd,
    Width = 1280,
    Height = 720,
    Format = TextureFormat.R8G8B8A8Unorm,
    PresentMode = PresentMode.Fifo,
    BufferCount = 2
});
```

`PresentMode` supports `Immediate`, `Mailbox`, and `Fifo` (vsync).

Beyond these, `IGraphicsDevice` also exposes `CreateCommandBuffer`, `CreateCommandQueue`, `CreateFence`, and `CreateSemaphore` for recording GPU work and synchronizing it — mirroring the explicit, low-level synchronization model of modern graphics APIs rather than hiding it.

## Backends

Current development is focused on:

| Backend     | Status            | Location            | Underlying Silk.NET package(s)                                          |
| ----------- | ------------------| ---------------------| --------------------------------------------------------------------------|
| Direct3D 11 | 🟢 In development | `Vulcan/DirectX/`     | `Silk.NET.Direct3D11`, `Silk.NET.DXGI`, `Silk.NET.Direct3D.Compilers`    |
| Vulkan      | 🟡 In development | `Vulcan/Vulkan/`      | `Silk.NET.Vulkan`, `Silk.NET.Vulkan.Extensions.KHR`                       |

The Direct3D 11 backend (`Direct3D11GraphicsDevice`) currently implements buffer and texture resources (`D3D11Buffer`, `D3D11Texture`). The Vulkan backend (`VulkanGraphicsDevice`) is split across partial-class files for instance creation, physical device selection, logical device creation, surface handling, and swapchain management (`VulkanGraphicsDevice.Instance.cs`, `.PhysicalDevice.cs`, `.Device.cs`, `.Surface.cs`, `.Swapchain.cs`).

The architecture is intended to allow additional backends (e.g. Metal, D3D12) without changing the public `Vulcan.Graphics` abstraction — a new backend just needs to implement `IGraphicsDevice` and the associated resource interfaces.

## Source Generation

Vulcan includes **Vulcan.SourceGen**, a Roslyn `IIncrementalGenerator` used to generate backend-specific device initialization boilerplate.

You annotate a `partial` class implementing `IGraphicsDevice` with `[VulcanGraphicsDevice]`:

```csharp
[VulcanGraphicsDevice(
    Backend = GraphicsBackend.D3D11,
    Debug = true)]
public unsafe partial class Direct3D11GraphicsDevice : IGraphicsDevice
{
}
```

At compile time, the generator:

1. Emits the `VulcanGraphicsDeviceAttribute` and `GraphicsBackend` types themselves (`D3D11`, `Vulkan`) via `RegisterPostInitializationOutput`, so consumers don't need a separate package reference just for the attribute.
2. Scans the compilation for classes decorated with `[VulcanGraphicsDevice]`.
3. Reads the `Backend` and `Debug` named arguments off the attribute.
4. Generates backend-specific initialization source (`GenerateD3D11` or `GenerateVulkan`) into a matching `partial class`, wiring up native instance/device creation and optional debug/validation layers.

This keeps repetitive native initialization code (device/instance creation, debug layer setup, etc.) out of the hand-written implementation, without hiding what's actually happening the way a runtime reflection-based approach would — you can inspect the generated source for any device at any time.

`Vulcan.SourceGen` targets `netstandard2.0` (the required TFM for Roslyn analyzers/generators), sets `EnforceExtendedAnalyzerRules`, and is referenced from `Vulcan.csproj` as:

```xml
<ProjectReference Include="..\Vulcan.SourceGen\Vulcan.SourceGen.csproj"
                   OutputItemType="Analyzer"
                   ReferenceOutputAssembly="false" />
```

## Design Goals

Vulcan is designed around a few principles:

* **Low-level** — expose graphics concepts rather than hiding them behind a high-level renderer.
* **Cross-platform** — the same graphics abstraction can target different native APIs.
* **Explicit** — resources and their intended usage are described directly, via plain descriptor structs rather than hidden defaults.
* **.NET-friendly** — designed for C# and modern .NET (nullable reference types, `init`-only properties, `ReadOnlyMemory<T>`) while still allowing direct native/unsafe interaction where needed.
* **Minimal abstraction overhead** — the abstraction should help portability without becoming a bottleneck; descriptions map close to 1:1 with native concepts.
* **Extensible** — new graphics backends should be able to implement the same core interfaces without changes to consuming code.

## Why Vulcan?

Native graphics APIs are powerful, but applications that want to support multiple APIs can end up maintaining separate rendering implementations.

Vulcan provides a common layer:

```text
             Your renderer
                  │
                  ▼
                Vulcan
             ┌────┴────┐
             ▼         ▼
          D3D11      Vulkan
             │         │
             └────┬────┘
                  ▼
                 GPU
```

Higher-level projects can build their own rendering systems on top of Vulcan without Vulcan itself dictating how rendering should be done. Vulcan owns device/resource creation and backend routing; everything above that (render graphs, materials, scene graphs, etc.) is left to the consumer.

## Dependencies

Vulcan is built on the [Silk.NET](https://github.com/dotnet/Silk.NET) bindings for native graphics APIs. `Vulcan.csproj` references:

* `Silk.NET.Direct3D11` (2.23.0)
* `Silk.NET.Direct3D.Compilers` (2.23.0)
* `Silk.NET.DXGI` (2.23.0)
* `Silk.NET.Vulkan` (2.23.0)
* `Silk.NET.Vulkan.Extensions.KHR` (2.23.0)
* `Silk.NET.Windowing` (2.23.0)

`Vulcan.SourceGen.csproj` references:

* `Microsoft.CodeAnalysis.CSharp` (5.9.0)
* `Microsoft.CodeAnalysis.Analyzers` (5.9.0) — build-time only (`PrivateAssets=all`)

`Vulcan` also enables `<AllowUnsafeBlocks>true</AllowUnsafeBlocks>` and `<Nullable>enable</Nullable>` project-wide.

## Project Structure

```text
Vulcan/
├── Vulcan/                                  # Core graphics API (net10.0)
│   ├── Graphics/
│   │   ├── Descriptions/
│   │   │   ├── BufferDescription.cs
│   │   │   ├── TextureDescription.cs
│   │   │   ├── SamplerDescription.cs
│   │   │   ├── ShaderDescription.cs
│   │   │   ├── PipelineDescription.cs
│   │   │   └── SwapchainDescription.cs
│   │   ├── IBuffer.cs / ITexture.cs / ISampler.cs / IShader.cs
│   │   ├── IPipeline.cs / ISwapchain.cs
│   │   ├── ICommandBuffer.cs / ICommandQueue.cs
│   │   ├── IFence.cs / ISemaphore.cs
│   │   ├── IRasterizerState.cs / IDepthStencilState.cs / IBlendState.cs
│   │   ├── IVertexLayout.cs / IViewport.cs / IScissorRect.cs
│   │   └── (enums) BufferUsage, TextureUsage, TextureFormat, TextureType,
│   │       MemoryUsage, ShaderStage, PresentMode, PrimitiveTopology,
│   │       Filter, AddressMode, SampleCount
│   ├── DirectX/                              # Direct3D 11 backend
│   │   ├── Direct3D11GraphicsDevice.cs
│   │   ├── D3D11Buffer.cs
│   │   └── D3D11Texture.cs
│   ├── Vulkan/                               # Vulkan backend
│   │   ├── VulkanGraphicsDevice.cs
│   │   ├── VulkanGraphicsDevice.Instance.cs
│   │   ├── VulkanGraphicsDevice.PhysicalDevice.cs
│   │   ├── VulkanGraphicsDevice.Device.cs
│   │   ├── VulkanGraphicsDevice.Surface.cs
│   │   └── VulkanGraphicsDevice.Swapchain.cs
│   ├── Maths/
│   │   └── Vertex.cs
│   ├── Shaders/
│   │   └── Shaders.cs
│   ├── IGraphicsDevice.cs                    # Core device abstraction
│   ├── Vulcan.cs                             # Vulcan.CreateDevice(...) factory
│   └── Vulcan.csproj
│
├── Vulcan.SourceGen/                         # Roslyn incremental generator (netstandard2.0)
│   ├── VulcanGraphicsDeviceAttribute.cs
│   ├── VulcanGraphicsDeviceGenerator.cs
│   ├── GraphicsBackend.cs
│   └── Vulcan.SourceGen.csproj
│
├── Vulcan.slnx
├── LICENSE.txt
└── README.md
```

## Status

Vulcan is currently being developed alongside **Crafty Engine**.

The API is still evolving as additional graphics resources, commands, and backend implementations are added. The Direct3D 11 backend currently has buffer and texture support; the Vulkan backend currently covers instance/device/surface/swapchain setup. Command recording, pipeline creation, and full resource support across both backends are ongoing work.

Expect breaking changes while the core abstraction is being established.

## License

Vulcan is licensed under the **MIT License**.

See [LICENSE.txt](LICENSE.txt) for details.