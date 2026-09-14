namespace Vulcan.Graphics;

public interface ICommandQueue : IDisposable
{
    ICommandBuffer CreateCommandBuffer();

    void Submit(ICommandBuffer commandBuffer);
}
