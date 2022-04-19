namespace Munin.Node.Plugins.SensorOmron;

internal sealed class Settings
{
    public string Port { get; set; } = default!;

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
