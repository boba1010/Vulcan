using System.Numerics;

namespace Vulcan;

public interface IGraphics3D
{
    void DrawPoint(Vector3 position);
    void DrawLine(Vector3 start, Vector3 end);
    void DrawTriangle(Vector3 a, Vector3 b, Vector3 c);
    void DrawCube(Vector3 position, Vector3 size);
    void DrawSphere(Vector3 position, float radius);
    void DrawPlane(Vector3 position, Vector2 size);
}
