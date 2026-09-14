namespace Vulcan.Graphics.Descriptions;

public readonly struct ShaderDescription
{
    public ReadOnlyMemory<byte> Code { get; init; }
    public ShaderStage Stage { get; init; }
    public string EntryPoint { get; init; }
}
