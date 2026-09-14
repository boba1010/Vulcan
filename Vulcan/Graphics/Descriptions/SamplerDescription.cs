namespace Vulcan.Graphics.Descriptions;

public readonly struct SamplerDescription
{
    public Filter MinFilter { get; init; }
    public Filter MagFilter { get; init; }
    public Filter MipmapFilter { get; init; }

    public AddressMode AddressU { get; init; }
    public AddressMode AddressV { get; init; }
    public AddressMode AddressW { get; init; }

    public float MipLodBias { get; init; }
    public float MinLod { get; init; }
    public float MaxLod { get; init; }
}
