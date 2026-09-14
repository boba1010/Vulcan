namespace Vulcan.Graphics;

public interface ICommandBuffer : IDisposable
{
    void Begin();
    void End();

    void SetPipeline(IPipeline pipeline);

    void SetVertexBuffer(IBuffer buffer);
    void SetIndexBuffer(IBuffer buffer);

    void SetViewport(in IViewport viewport);
    void SetScissor(in IScissorRect scissor);

    void ClearColor(float r, float g, float b, float a);
    void ClearDepth(float depth);

    void Draw(
        uint vertexCount,
        uint instanceCount = 1,
        uint firstVertex = 0,
        uint firstInstance = 0);

    void DrawIndexed(
        uint indexCount,
        uint instanceCount = 1,
        uint firstIndex = 0,
        int vertexOffset = 0,
        uint firstInstance = 0);
}
