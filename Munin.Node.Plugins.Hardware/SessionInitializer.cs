namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

public sealed class SessionInitializer : ISessionInitializer, IDisposable
{
    private readonly Computer computer;

    public SessionInitializer(Computer computer)
    {
        this.computer = computer;
    }

    public void Dispose()
    {
        computer.Close();
    }

    public void Setup()
    {
        var value = SensorValuePool.Default.Rent();

        lock (computer)
        {
            SensorValueHelper.Update(computer);
            SensorValueHelper.Gather(computer, value);
        }

        HardwareContext.Snapshot.Value = value;
    }

    public void Shutdown()
    {
        SensorValuePool.Default.Return(HardwareContext.Snapshot.Value!);
    }
}
