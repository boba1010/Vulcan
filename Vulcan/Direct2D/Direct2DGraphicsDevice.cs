using Silk.NET.Direct2D;
using Silk.NET.DirectWrite;
using Silk.NET.DXGI;
using Silk.NET.Windowing;
using System.Numerics;
using Vulcan.Direct2D;
using Vulcan.UI;

namespace Vulcan.DirectX;

[VulcanGraphicsDevice(Backend = GraphicsBackend.D2D1, Debug = true)]
public unsafe sealed partial class Direct2DGraphicsDevice : I2DGraphicsDevice
{
    private readonly IWindow _window = null!;
    private ID2D1Factory* _factory;
    private ID2D1HwndRenderTarget* _renderTarget;
    private D2D _d2d = null!;
    private DWrite _dwrite = null!;
    private Silk.NET.DirectWrite.IDWriteFactory* _writeFactory;

    public Direct2DGraphicsDevice(IWindow window)
    {
        _window = window;
    }

    public void ResizeFrameBuffer(uint width, uint height)
    {
        var size = new Silk.NET.Maths.Vector2D<uint>(width, height);

        _renderTarget->Resize(&size);
    }

    public void GetDpi(out float dpiX, out float dpiY)
    {
        float DPIX;
        float DPIY;

        _renderTarget->GetDpi(&DPIX, &DPIY);

        dpiX = DPIX;
        dpiY = DPIY;
    }

    public void GetSize(out float width, out float height)
    {
        var size = _renderTarget->GetSize();

        width = size.X;
        height = size.Y;
    }

    public void DrawText(IBrush brush, string text, Rect rect, float fontSize, TextHorizontalAlignment horizontalAlignment, TextVerticalAlignment verticalAlignment)
    {
        var d2dBrush = (D2D1Brush)brush;

        Silk.NET.DirectWrite.IDWriteTextFormat* dwriteFormat = null;

        GetDpi(out float dpiX, out float dpiY);

        fixed (char* fontFamily = "Arial")
        fixed (char* locale = "en-us")
        {

            var result = _writeFactory->CreateTextFormat(
                fontFamily,
                null,
                FontWeight.Normal,
                FontStyle.Normal,
                FontStretch.Normal,
                fontSize,
                locale,
                &dwriteFormat);

            if (result < 0)
                throw new InvalidOperationException($"CreateTextFormat failed: 0x{result:X8}");
        }

        var textAlignment = horizontalAlignment switch
        {
            TextHorizontalAlignment.Left => TextAlignment.Leading,
            TextHorizontalAlignment.Right => TextAlignment.Trailing,
            TextHorizontalAlignment.Center => TextAlignment.Center,
            TextHorizontalAlignment.Stretch => TextAlignment.Justified,
            _ => TextAlignment.Center
        };
        var paragraphAlignment = verticalAlignment switch
        {
            TextVerticalAlignment.Top => ParagraphAlignment.Near,
            TextVerticalAlignment.Center => ParagraphAlignment.Center,
            TextVerticalAlignment.Bottom => ParagraphAlignment.Far,
            _ => ParagraphAlignment.Center
        };

        dwriteFormat->SetTextAlignment(textAlignment);
        dwriteFormat->SetParagraphAlignment(paragraphAlignment);

        var textFormat = (Silk.NET.Direct2D.IDWriteTextFormat*)dwriteFormat;

        var rectangle = new Silk.NET.Maths.Box2D<float>(
            rect.X,
            rect.Y,
            rect.X + rect.Width,
            rect.Y + rect.Height);

        fixed (char* textPtr = text)
        {
            _renderTarget->DrawTextA(
                textPtr,
                (uint)text.Length,
                textFormat,
                &rectangle,
                (ID2D1Brush*)d2dBrush.Handle,
                DrawTextOptions.None,
                DwriteMeasuringMode.GdiNatural);
        }

        dwriteFormat->Release();
    }

    public void Clear(Color color)
    {
        var d3dColor = new D3Dcolorvalue
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A
        };

        _renderTarget->Clear(&d3dColor);
    }

    public IBrush CreateSolidColorBrush(Color color)
    {
        var d3dColor = new D3Dcolorvalue
        {
            R = color.R,
            G = color.G,
            B = color.B,
            A = color.A
        };

        ID2D1SolidColorBrush* brush = null;

        var result = _renderTarget->CreateSolidColorBrush(&d3dColor, null, &brush);

        if (result < 0)
            throw new InvalidOperationException($"Failed to create D2D brush. HRESULT: 0x{result:X8}");

        return new D2D1Brush(brush);
    }

    public void FillRectangle(IBrush brush, Rect rect)
    {
        var d2dBrush = (D2D1Brush)brush;

        var rectangle = new Silk.NET.Maths.Box2D<float>(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);

        _renderTarget->FillRectangle(&rectangle, (ID2D1Brush*)d2dBrush.Handle);
    }

    public void FillRoundedRectangle(IBrush brush, Rect rect, float radius)
    {
        var d2dBrush = (D2D1Brush)brush;

        var rectangle = new Silk.NET.Maths.Box2D<float>(rect.X, rect.Y, rect.X + rect.Width, rect.Y + rect.Height);
        var roundedRectangle = new RoundedRect
        {
            Rect = rectangle,
            RadiusX = radius,
            RadiusY = radius
        };

        _renderTarget->FillRoundedRectangle(&roundedRectangle, (ID2D1Brush*)d2dBrush.Handle);
    }

    public void FillEllipse(IBrush brush, Vector2 center, Vector2 radius)
    {
        var d2dBrush = (D2D1Brush)brush;

        var ellipse = new Ellipse
        {
            Point = new(center.X, center.Y),
            RadiusX = radius.X,
            RadiusY = radius.Y
        };

        _renderTarget->FillEllipse(&ellipse, (ID2D1Brush*)d2dBrush.Handle);
    }

    public void DrawLine(IBrush brush, Vector2 start, Vector2 end, float thickness)
    {
        var d2dBrush = (D2D1Brush)brush;

        var startPoint = new Silk.NET.Maths.Vector2D<float>(start.X, start.Y);

        var endPoint = new Silk.NET.Maths.Vector2D<float>(end.X,end.Y);

        _renderTarget->DrawLine(startPoint, endPoint, (ID2D1Brush*)d2dBrush.Handle, thickness, null);
    }

    public void BeginDraw()
    {
        _renderTarget->BeginDraw();
    }

    public void EndDraw()
    {
        ulong tag1 = 0;
        ulong tag2 = 0;

        var result = _renderTarget->EndDraw(ref tag1, ref tag2);

        if (result < 0)
            throw new InvalidOperationException($"D2D EndDraw failed. HRESULT: 0x{result:X8}");
    }

    public void Dispose()
    {
        if (_renderTarget is not null)
        {
            _renderTarget->Release();
            _renderTarget = null;
        }

        if (_factory is not null)
        {
            _factory->Release();
            _factory = null;
        }

        _d2d?.Dispose();
    }
}
