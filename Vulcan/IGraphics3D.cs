using System.Numerics;

namespace Vulcan;

public interface IGraphics3D
{
    void DrawPoint(Vector3 position, Vector4 color);
    void DrawLine(Vector3 start, Vector3 end, Vector4 color);
    void DrawTriangle(Vector3 a, Vector3 b, Vector3 c, Vector4 color);
    void DrawCube(Vector3 position, Vector3 size, Vector4 color);
    void DrawSphere(Vector3 position, float radius, Vector4 color);
    void DrawPlane(Vector3 position, Vector2 size, Vector4 color);
}
