namespace Munin.Node.Plugins.Hardware;

using System.Collections.Generic;
using System.Linq;

public static class Helper
{
    public static bool IsTarget(string value, string[]? include, string[]? exclude)
    {
        if (include?.Length > 0)
        {
            return include.Contains(value);
        }

        return !exclude?.Contains(value) ?? true;
    }

    public static List<string> Order(List<string> values, string[]? order)
    {
        if ((order is null) || (order.Length == 0))
        {
            return values;
        }

        var newList = new List<string>(values.Count);
        newList.AddRange(order.Where(values.Remove));
        newList.AddRange(values);
        return newList;
    }
}
