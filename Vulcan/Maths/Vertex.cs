using System.Numerics;
using System.Runtime.InteropServices;

namespace Vulcan.Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector2 Position;
    public Vector4 Color;
}
