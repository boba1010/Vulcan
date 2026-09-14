using System;
namespace Vulcan.Graphics;

public enum TextureFormat
{
    Unknown,

    R8Unorm,
    R8G8Unorm,
    R8G8B8A8Unorm,
    R8G8B8A8Srgb,

    R16Float,
    R16G16Float,
    R16G16B16A16Float,

    R32Float,
    R32G32Float,
    R32G32B32Float,
    R32G32B32A32Float,

    D16Unorm,
    D24UnormS8Uint,
    D32Float,
    D32FloatS8Uint
}
