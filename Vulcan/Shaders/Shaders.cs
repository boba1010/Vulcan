using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;

namespace Vulcan.Shaders;

public unsafe static class Shaders
{
    public static void CompileShaders(
        D3DCompiler compiler, 
        string source, 
        out ID3D10Blob* vertexShader,
        out ID3D10Blob* pixelShader)
    {
        vertexShader = CompileShader(source, "vs_main", "vs_5_0", compiler);
        pixelShader = CompileShader(source, "ps_main", "ps_5_0", compiler);
    }

    private static ID3D10Blob* CompileShader(string source, string entryPoint, string target, D3DCompiler compiler)
    {
        ID3D10Blob* blob = null;
        ID3D10Blob* errors = null;

        var sourceBytes = System.Text.Encoding.UTF8.GetBytes(source);

        fixed (byte* pSource = sourceBytes)
        {
            compiler.Compile(
                pSource,
                (nuint)sourceBytes.Length,
                (string)null!,
                null,
                null,
                entryPoint,
                target,
                0,
                0,
                &blob,
                &errors);
        }

        if (errors != null)
            Console.WriteLine(SilkMarshal.PtrToString((nint)errors->GetBufferPointer()));

        return blob;
    }
}
