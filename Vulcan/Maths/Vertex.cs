using System.Numerics;
using System.Runtime.InteropServices;

namespace Vulcan.Maths;

[StructLayout(LayoutKind.Sequential)]
public struct Vertex
{
    public Vector3 Position;
    public Vector4 Color;
}
