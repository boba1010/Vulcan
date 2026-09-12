namespace Vulcan;

public interface IGraphicsDevice : IDisposable, IGraphics2D, IGraphics3D
{
    public void Initialize();
    public void Clear();
    public void Present();
}
