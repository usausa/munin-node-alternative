namespace Munin.Node.Service;

internal sealed class Settings
{
    public int Port { get; set; }

    public string[]? Modules { get; set; }
}
