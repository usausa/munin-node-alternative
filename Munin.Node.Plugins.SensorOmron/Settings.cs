namespace Munin.Node.Plugins.SensorOmron;

using System.Diagnostics.CodeAnalysis;

internal sealed class Settings
{
    [AllowNull]
    public string Port { get; set; }
}
