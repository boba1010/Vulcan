using Silk.NET.Direct2D;
using Vulcan.UI;

namespace Vulcan.Direct2D;

public unsafe sealed class D2D1Brush(ID2D1SolidColorBrush* handle) : IBrush
{
    internal ID2D1SolidColorBrush* Handle = handle;

    public void Dispose()
    {
        if (Handle is not null)
        {
            Handle->Release();
            Handle = null;
        }
    }
}
