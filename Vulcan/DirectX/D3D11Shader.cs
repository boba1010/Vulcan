using Silk.NET.Direct3D11;
using Vulcan.Graphics;

namespace Vulcan.DirectX;

public unsafe sealed class D3D11Shader : IShader
{
    internal ID3D11DeviceChild* Handle;
    internal ShaderStage Stage;
    internal byte[] Code;

    internal D3D11Shader(ID3D11DeviceChild* handle, ShaderStage stage, byte[] code)
    {
        Handle = handle;
        Stage = stage;
        Code = code;
    }

    public void Dispose()
    {
        if (Handle is not null)
        {
            Handle->Release();
            Handle = null;
        }
    }
}
