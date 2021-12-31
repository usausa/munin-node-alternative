namespace Munin.Node.Service;

public sealed class Settings
{
    public int Port { get; set; }

    public string[]? Modules { get; set; }
}
