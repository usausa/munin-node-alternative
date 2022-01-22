using Munin.Node.Service;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

var builder = Host.CreateDefaultBuilder(args);

builder.UseWindowsService();

builder
    .ConfigureLogging((context, logging) =>
    {
        logging.ClearProviders();
        if (context.HostingEnvironment.IsDevelopment())
        {
            logging.AddConsole();
            logging.AddDebug();
        }
        else
        {
            logging.AddEventLog();
        }
    });

builder.ConfigureServices((context, services) =>
{
    var settings = context.Configuration.GetSection("Node").Get<Settings>();
    services.AddSingleton(settings);

    services.AddHostedService<HostedService>();

    services.AddSingleton<PluginManager>();

    if (settings.Modules is not null)
    {
        var pluginBuilder = new PluginBuilder();
        foreach (var module in settings.Modules)
        {
            pluginBuilder.AddModule(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), module)));
        }

        pluginBuilder.Build(context.Configuration, services);
    }
});

var host = builder.Build();

await host.RunAsync().ConfigureAwait(false);
