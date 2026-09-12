namespace Vulcan.SourceGen;

[AttributeUsage(AttributeTargets.Class)]
public sealed class VulcanGraphicsDeviceAttribute : Attribute
{
    public bool Debug { get; set; }
    public bool BgraSupport { get; set; } = true;
}