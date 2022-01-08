namespace Munin.Node.Service;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

internal sealed class HostedService : IHostedService, IDisposable
{
    private const int ReadBufferSize = 256;
    private const int WriteBufferSize = 8192;

    private static readonly byte[] CommandNodes = Encoding.ASCII.GetBytes("nodes");
    private static readonly byte[] CommandList = Encoding.ASCII.GetBytes("list");
    private static readonly byte[] CommandConfig = Encoding.ASCII.GetBytes("config");
    private static readonly byte[] CommandFetch = Encoding.ASCII.GetBytes("fetch");
    private static readonly byte[] CommandVersion = Encoding.ASCII.GetBytes("version");
    private static readonly byte[] CommandQuit = Encoding.ASCII.GetBytes("quit");

    private readonly ILogger<HostedService> logger;

    private readonly Listener listener;

    private readonly PluginManager pluginManager;

    public HostedService(ILoggerFactory loggerFactory, Settings settings, PluginManager pluginManager)
    {
        logger = loggerFactory.CreateLogger<HostedService>();
        listener = new Listener(loggerFactory.CreateLogger<Listener>(), new IPEndPoint(IPAddress.Any, settings.Port));
        listener.OnClientAccepted += OnClientAccepted;
        this.pluginManager = pluginManager;
    }

    public void Dispose()
    {
        listener.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        listener.Start();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        listener.Stop();
        return Task.CompletedTask;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1031: not catch general exception types", Justification = "Ignore")]
    private void OnClientAccepted(Socket parameter)
    {
        _ = Task.Run(async () =>
        {
            using var socket = parameter;
            using var request = new RequestBuffer(ReadBufferSize);
            using var response = new ResponseBuilder(WriteBufferSize);
            try
            {
                response.Add("# munin node at ");
                response.Add(Environment.MachineName);
                response.AddLineFeed();
                if (await WriteAsync(socket, response.AsSendMemory()).ConfigureAwait(false) < 0)
                {
                    return;
                }

                var process = true;
                do
                {
                    // Read line
                    var read = await socket.ReceiveAsync(request.AsReceiveMemory(), SocketFlags.None).ConfigureAwait(false);
                    if (read < 0)
                    {
                        break;
                    }

                    request.Advance(read);

                    while (request.TryGetLine(out var line))
                    {
                        response.Clear();
                        var result = Process(line, response);

                        // Write response
                        if (response.IsEmpty)
                        {
                            response.Add("# Unknown Error");
                            response.AddLineFeed();
                            response.AddEndLine();
                        }

                        if (await WriteAsync(socket, response.AsSendMemory()).ConfigureAwait(false) < 0)
                        {
                            process = false;
                            break;
                        }

                        if (!result)
                        {
                            process = false;
                            break;
                        }

                        request.Flip(line.Length);
                    }

                    if (!request.HasRemaining)
                    {
                        break;
                    }
                }
                while (process);
            }
            catch (SocketException ex)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(ex, "Connection error.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Connection error.");
            }
        });
    }

    private bool Process(Memory<byte> line, ResponseBuilder response)
    {
        var span = line.TrimEnd((byte)'\n').TrimEnd((byte)'\r').Span;
        var index = span.IndexOf((byte)' ');
        var command = index >= 0 ? span[..index] : span;

        if (command.SequenceEqual(CommandNodes))
        {
            response.Add(Environment.MachineName);
            response.AddLineFeed();
            response.AddEndLine();
        }
        else if (command.SequenceEqual(CommandList))
        {
            pluginManager.BuildNames(response);
        }
        else if (command.SequenceEqual(CommandConfig))
        {
            IPlugin? plugin;
            if ((index >= 0) && ((plugin = pluginManager.LookupPlugin(span[(index + 1)..].Trim((byte)' '))) != null))
            {
                plugin.BuildConfig(response);
            }
            else
            {
                response.Add("# Unknown service");
                response.AddLineFeed();
                response.AddEndLine();
            }
        }
        else if (command.SequenceEqual(CommandFetch))
        {
            IPlugin? plugin;
            if ((index >= 0) && ((plugin = pluginManager.LookupPlugin(span[(index + 1)..].Trim((byte)' '))) != null))
            {
                plugin.BuildFetch(response);
            }
            else
            {
                response.Add("# Unknown service");
                response.AddLineFeed();
                response.AddEndLine();
            }
        }
        else if (command.SequenceEqual(CommandVersion))
        {
            response.Add("munin node on %s version: Munin Node for Windows ");
            response.Add(typeof(HostedService).Assembly.GetName().Version!.ToString());
            response.AddLineFeed();
        }
        else if (command.SequenceEqual(CommandQuit))
        {
            return false;
        }
        else
        {
            response.Add("# Unknown command. Try list, nodes, config, fetch, version or quit");
            response.AddLineFeed();
        }

        return true;
    }

    private static async ValueTask<int> WriteAsync(Socket socket, ReadOnlyMemory<byte> buffer)
    {
        var offset = 0;
        do
        {
            var write = await socket.SendAsync(buffer[offset..], SocketFlags.None).ConfigureAwait(false);
            if (write <= 0)
            {
                return -1;
            }

            offset += write;
        }
        while (offset < buffer.Length);

        return offset;
    }
}
