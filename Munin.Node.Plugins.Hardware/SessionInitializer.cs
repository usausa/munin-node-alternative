namespace Munin.Node.Plugins.Hardware;

using LibreHardwareMonitor.Hardware;

public sealed class SessionInitializer : ISessionInitializer, IDisposable
{
    private readonly Computer computer;

    private readonly Stack<SensorValue> stack = new();

    public SessionInitializer(Computer computer)
    {
        this.computer = computer;
        // TODO 分配
    }

    public void Dispose()
    {
        computer.Close();
    }

    public void Setup()
    {
        var value = Rent();
        if (value is null)
        {
            // TODO
            value = new SensorValue();
        }

        SensorValue.Current.Value = value;

        lock (computer)
        {
            // TODO lock
            // TODO Sensor update
        }
    }

    public void Shutdown()
    {
        Return(SensorValue.Current.Value!);
    }

    private SensorValue? Rent()
    {
        lock (stack)
        {
            return stack.TryPop(out var value) ? value : null;
        }
    }

    private void Return(SensorValue value)
    {
        lock (stack)
        {
            if (stack.Count > 8)
            {
                return;
            }

            stack.Push(value);
        }
    }
}
