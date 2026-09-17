namespace Vulcan.Graphics;

public interface IFence : IDisposable
{
    ulong Value { get; }

    void Wait();
}
