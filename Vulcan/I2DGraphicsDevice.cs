using System.Numerics;
using Vulcan.UI;

namespace Vulcan;

public interface I2DGraphicsDevice : IDisposable
{
    public void Initialize();
    void Clear(Color color);
    public IBrush CreateSolidColorBrush(Color color);
    public void FillRectangle(IBrush brush,Rect rect);
    public void FillRoundedRectangle(IBrush brush, Rect rect, float radius);
    public void FillEllipse(IBrush brush, Vector2 center, Vector2 radius);
    public void DrawLine(IBrush brush, Vector2 start, Vector2 end, float thickness);
    public void BeginDraw();
    public void EndDraw();
    public void GetDpi(out float dpiX, out float dpiY);
    public void GetSize(out float width, out float height);
    public void ResizeFrameBuffer(uint width, uint height);
    void DrawText(
        IBrush brush,
        string text,
        Rect bounds,
        float fontSize,
        TextHorizontalAlignment horizontalAlignment = TextHorizontalAlignment.Center,
        TextVerticalAlignment verticalAlignment = TextVerticalAlignment.Center);
}
