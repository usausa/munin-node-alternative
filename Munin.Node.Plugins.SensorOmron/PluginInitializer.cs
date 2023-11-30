namespace Munin.Node.Plugins.SensorOmron;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal sealed class PluginInitializer : IPluginInitializer
{
    public void Setup(IConfiguration config, IServiceCollection services)
    {
        var settings = config.GetSection("SensorOmron").Get<Settings>()!;
        if (String.IsNullOrEmpty(settings.Port))
        {
            return;
        }

        var repository = new SensorRepository(settings.Port, settings.Expire * 10000L);

        if (settings.Temperature)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_temperature",
                "environment",
                "Temperature",
                "C",
                "-l 0 -u 40",
                null,
                [
                    new("temperature", "Temperature", static x => x.Temperature)
                ]));
        }
        if (settings.Humidity)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_humidity",
                "environment",
                "Humidity",
                "%",
                "-l 0 -u 100",
                true,
                [
                    new("humidity", "Humidity", static x => x.Humidity)
                ]));
        }
        if (settings.Light)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_light",
                "environment",
                "Light",
                "lx",
                "-b 1000 -l 0",
                null,
                [
                    new("light", "Light", static x => x.Light)
                ]));
        }
        if (settings.Pressure)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_pressure",
                "environment",
                "Barometric pressure",
                "hPa",
                "-b 1000 -l 0",
                null,
                [
                    new("pressure", "Barometric pressure", static x => x.Pressure)
                ]));
        }
        if (settings.Noise)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_noise",
                "environment",
                "Sound noise",
                "dB",
                "-b 1000 -l 0",
                null,
                [
                    new("noise", "Sound noise", static x => x.Noise)
                ]));
        }
        if (settings.Equivalent)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_equivalent",
                "environment",
                "eTVOC / eCO2",
                "ppb / ppm",
                "-b 1000 -l 0",
                null,
                [
                    new("etvoc", "eTVOC", static x => x.Etvoc),
                    new("eco2", "eCO2", static x => x.Eco2)
                ]));
        }
        if (settings.Index)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_index",
                "environment",
                "Discomfort index / Heat stroke",
                "%",
                "-l 0 -u 100",
                false,
                [
                    new("discomfort", "Discomfort index", static x => x.Discomfort),
                    new("heat", "Heat stroke", static x => x.Heat)
                ]));
        }
        if (settings.Seismic)
        {
            services.AddSingleton<IPlugin>(new SeismicPlugin(
                repository,
                "omron_seismic",
                "environment",
                "Seismic intensity",
                "scale",
                "-b 1000 -l 0",
                null,
                [
                    new("seismic", "Seismic intensity", static x => x.Seismic)
                ]));
        }
    }
}
