namespace Munin.Node.Plugins.Hardware;

internal sealed class SensorValuePool
{
    public static SensorValuePool Default { get; } = new();

    private readonly Stack<List<SensorValue>> stack = new();

    public List<SensorValue> Rent()
    {
        lock (stack)
        {
            return stack.TryPop(out var value) ? value : new List<SensorValue>();
        }
    }

    public void Return(List<SensorValue> value)
    {
        lock (stack)
        {
            if (stack.Count >= 8)
            {
                return;
            }

            value.Clear();
            stack.Push(value);
        }
    }
}
