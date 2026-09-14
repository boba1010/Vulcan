namespace Vulcan.Graphics;

public interface IFence : IDisposable
{
    bool IsSignaled { get; }

    void Wait();
    void Reset();
}
