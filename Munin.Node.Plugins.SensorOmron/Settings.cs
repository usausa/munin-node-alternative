namespace Munin.Node.Plugins.SensorOmron;

using System.Diagnostics.CodeAnalysis;

internal sealed class Settings
{
    [AllowNull]
    public string Port { get; set; }

    public int Expire { get; set; } = 30000;

    public bool Temperature { get; set; }

    public bool Humidity { get; set; }

    public bool Light { get; set; }

    public bool Pressure { get; set; }

    public bool Noise { get; set; }

    public bool Equivalent { get; set; }

    public bool Index { get; set; }

    public bool Seismic { get; set; }
}
