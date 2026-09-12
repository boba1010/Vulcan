using System.Numerics;

namespace Vulcan;

public interface IGraphics2D
{
    void DrawPoint(Vector2 position, Vector4 color);
    void DrawLine(Vector2 start, Vector2 end, Vector4 color);
    void DrawTriangle(Vector2 a, Vector2 b, Vector2 c, Vector4 color);
    void DrawRectangle(Vector2 position, Vector2 size, Vector4 color);
    void DrawCircle(Vector2 center, float radius, Vector4 color, int segments = 64);
}
