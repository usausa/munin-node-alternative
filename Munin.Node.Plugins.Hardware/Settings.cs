namespace Munin.Node.Plugins.Hardware;

public sealed class HardwareEntry
{
    public bool Enable { get; set; }

    public string[]? Include { get; set; }

    public string[]? Exclude { get; set; }

    public string[]? Order { get; set; }
}

public sealed class Settings
{
    public HardwareEntry? MotherboardControl { get; set; }

    public HardwareEntry? MotherboardFan { get; set; }

    public HardwareEntry? MotherboardTemperature { get; set; }

    public HardwareEntry? MotherboardVoltage { get; set; }

    public HardwareEntry? CpuClock { get; set; }

    public HardwareEntry? CpuLoad { get; set; }

    public HardwareEntry? CpuPower { get; set; }

    public HardwareEntry? CpuTemperature { get; set; }

    public HardwareEntry? CpuVoltage { get; set; }

    public HardwareEntry? MemoryLoad { get; set; }

    public HardwareEntry? MemoryData { get; set; }

    public HardwareEntry? GpuClock { get; set; }

    public HardwareEntry? GpuControl { get; set; }

    public HardwareEntry? GpuFan { get; set; }

    public HardwareEntry? GpuLoad { get; set; }

    public HardwareEntry? GpuPower { get; set; }

    public HardwareEntry? GpuTemperature { get; set; }

    public HardwareEntry? GpuVoltage { get; set; }

    public HardwareEntry? StorageData { get; set; }

    public HardwareEntry? StorageLevel { get; set; }

    public HardwareEntry? StorageTemperature { get; set; }

    public HardwareEntry? StorageThroughput { get; set; }

    public HardwareEntry? StorageUsed { get; set; }

    public HardwareEntry? NetworkData { get; set; }

    public HardwareEntry? NetworkLoad { get; set; }

    public HardwareEntry? NetworkThroughput { get; set; }
}

public static class SettingsExtensions
{
    public static bool IsMotherboardControlEnable(this Settings settings) => settings.MotherboardControl?.Enable ?? false;

    public static bool IsMotherboardFanEnable(this Settings settings) => settings.MotherboardFan?.Enable ?? false;

    public static bool IsMotherboardTemperatureEnable(this Settings settings) => settings.MotherboardTemperature?.Enable ?? false;

    public static bool IsMotherboardVoltageEnable(this Settings settings) => settings.MotherboardVoltage?.Enable ?? false;

    public static bool IsCpuClockEnable(this Settings settings) => settings.CpuClock?.Enable ?? false;

    public static bool IsCpuLoadEnable(this Settings settings) => settings.CpuLoad?.Enable ?? false;

    public static bool IsCpuPowerEnable(this Settings settings) => settings.CpuPower?.Enable ?? false;

    public static bool IsCpuTemperatureEnable(this Settings settings) => settings.CpuTemperature?.Enable ?? false;

    public static bool IsCpuVoltageEnable(this Settings settings) => settings.CpuVoltage?.Enable ?? false;

    public static bool IsMemoryLoadEnable(this Settings settings) => settings.MemoryLoad?.Enable ?? false;

    public static bool IsMemoryDataEnable(this Settings settings) => settings.MemoryData?.Enable ?? false;

    public static bool IsGpuClockEnable(this Settings settings) => settings.GpuClock?.Enable ?? false;

    public static bool IsGpuControlEnable(this Settings settings) => settings.GpuControl?.Enable ?? false;

    public static bool IsGpuFanEnable(this Settings settings) => settings.GpuFan?.Enable ?? false;

    public static bool IsGpuLoadEnable(this Settings settings) => settings.GpuLoad?.Enable ?? false;

    public static bool IsGpuPowerEnable(this Settings settings) => settings.GpuPower?.Enable ?? false;

    public static bool IsGpuTemperatureEnable(this Settings settings) => settings.GpuTemperature?.Enable ?? false;

    public static bool IsGpuVoltageEnable(this Settings settings) => settings.GpuVoltage?.Enable ?? false;

    public static bool IsStorageDataEnable(this Settings settings) => settings.StorageData?.Enable ?? false;

    public static bool IsStorageLevelEnable(this Settings settings) => settings.StorageLevel?.Enable ?? false;

    public static bool IsStorageTemperatureEnable(this Settings settings) => settings.StorageTemperature?.Enable ?? false;

    public static bool IsStorageThroughputEnable(this Settings settings) => settings.StorageThroughput?.Enable ?? false;

    public static bool IsStorageUsedEnable(this Settings settings) => settings.StorageUsed?.Enable ?? false;

    public static bool IsNetworkDataEnable(this Settings settings) => settings.NetworkData?.Enable ?? false;

    public static bool IsNetworkLoadEnable(this Settings settings) => settings.NetworkLoad?.Enable ?? false;

    public static bool IsNetworkThroughputEnable(this Settings settings) => settings.NetworkThroughput?.Enable ?? false;
}
