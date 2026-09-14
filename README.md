# Vulcan

**A low-level, cross-platform graphics API for .NET, built on top of Silk.NET.**

Vulcan provides a unified graphics abstraction over native graphics APIs such as **Direct3D 11** and **Vulkan**, allowing higher-level projects to work with graphics resources without being tightly coupled to a specific backend.

> 🚧 **Vulcan is currently in active development. APIs may change.**

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

Vulcan intentionally stays **low-level**. It provides graphics-device and resource abstractions rather than becoming a high-level rendering engine.

## Features

### Graphics resources

Vulcan is designed around explicit graphics resources such as:

* Buffers
* Textures
* Samplers
* Shaders
* Pipelines
* Swapchains
* Command buffers
* Command queues
* Fences
* Semaphores

For example:

```csharp
using var buffer = device.CreateBuffer(new BufferDescription
{
    Size = 1024 * 1024,
    Usage = BufferUsage.Vertex,
    MemoryUsage = MemoryUsage.DeviceLocal
});
```

Textures use the same backend-independent approach:

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

The backend then maps these descriptions to the appropriate native graphics API.

## Backends

Current development is focused on:

| Backend     | Status            |
| ----------- | ----------------- |
| Direct3D 11 | 🟢 In development |
| Vulkan      | 🟡 In development |

The architecture is intended to allow additional backends without changing the public graphics abstraction.

## Source Generation

Vulcan includes **Vulcan.SourceGen**, a Roslyn source generator used to generate backend-specific initialization code.

Example:

```csharp
[VulcanGraphicsDevice(
    Backend = GraphicsBackend.D3D11,
    Debug = true)]
public unsafe partial class Direct3D11GraphicsDevice : IGraphicsDevice
{
}
```

The generator handles backend initialization while the graphics device implementation handles the actual graphics functionality.

This keeps repetitive native initialization code out of the main implementation without hiding the underlying graphics API.

## Design Goals

Vulcan is designed around a few principles:

* **Low-level** — expose graphics concepts rather than hiding them behind a high-level renderer.
* **Cross-platform** — the same graphics abstraction can target different native APIs.
* **Explicit** — resources and their intended usage are described directly.
* **.NET-friendly** — designed for C# and modern .NET while still allowing direct native interaction.
* **Minimal abstraction overhead** — the abstraction should help portability without becoming a bottleneck.
* **Extensible** — new graphics backends should be able to implement the same core interfaces.

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

Higher-level projects can build their own rendering systems on top of Vulcan without Vulcan itself dictating how rendering should be done.

## Project Structure

```text
Vulcan/
├── Vulcan/
│   ├── Graphics/
│   ├── ...
│   └── ...
│
├── Vulcan.SourceGen/
│   └── ...
│
├── Vulcan.slnx
├── LICENSE.txt
└── README.md
```

## Status

Vulcan is currently being developed alongside **Crafty Engine**.

The API is still evolving as additional graphics resources, commands, and backend implementations are added.

Expect breaking changes while the core abstraction is being established.

## License

Vulcan is licensed under the **MIT License**.

See [LICENSE.txt](LICENSE.txt) for details.
