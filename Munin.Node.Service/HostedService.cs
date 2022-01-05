namespace Munin.Node.Service;

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public sealed class HostedService : IHostedService, IDisposable
{
    private const int ReadBufferSize = 256;
    private const int WriteBufferSize = 4194304;

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
            using var request = new BufferSegment(ReadBufferSize);
            using var response = new BufferSegment(WriteBufferSize);
            try
            {
                response.Add("# munin node at ");
                response.Add(Environment.MachineName);
                response.AddLineFeed();
                if (await WriteAsync(socket, response.Buffer, response.Length).ConfigureAwait(false) < 0)
                {
                    return;
                }

                // Setup
                pluginManager.SetupSession();

                do
                {
                    // Read line
                    var index = await ReadLineAsync(socket, request).ConfigureAwait(false);
                    if (index < 0)
                    {
                        break;
                    }

                    response.Clear();
                    var result = Process(request, response);

                    // Write response
                    if (response.Length > 0)
                    {
                        if (await WriteAsync(socket, response.Buffer, response.Length).ConfigureAwait(false) < 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        response.Add("# Unknown Error");
                        response.AddLineFeed();
                        response.AddEndLine();
                        if (await WriteAsync(socket, response.Buffer, response.Length).ConfigureAwait(false) < 0)
                        {
                            break;
                        }
                    }

                    if (!result)
                    {
                        break;
                    }

                    // Flip
                    var nextStart = index + 1;
                    if (nextStart < request.Length)
                    {
                        var nextSize = request.Length - nextStart;
                        request.Buffer.AsSpan(nextStart, nextSize).CopyTo(request.Buffer.AsSpan());
                        request.Length = nextSize;
                    }
                    else
                    {
                        request.Clear();
                    }
                }
                while (true);
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
            finally
            {
                // Shutdown
                pluginManager.ShutdownSession();
            }
        });
    }

    private bool Process(BufferSegment request, BufferSegment response)
    {
        var span = request.Buffer.AsSpan(0, request.Length).TrimEnd((byte)'\n').TrimEnd((byte)'\r');
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

    private static async ValueTask<int> ReadLineAsync(Socket socket, BufferSegment buffer)
    {
        do
        {
            var read = await socket.ReceiveAsync(new ArraySegment<byte>(buffer.Buffer, buffer.Length, buffer.Remaining), SocketFlags.None).ConfigureAwait(false);
            if (read <= 0)
            {
                return -1;
            }

            var index = buffer.Buffer.AsSpan(buffer.Length).IndexOf((byte)'\n');
            buffer.Length += read;

            if (index >= 0)
            {
                return index;
            }

            if (buffer.Remaining == 0)
            {
                return -1;
            }
        }
        while (true);
    }

    private static async ValueTask<int> WriteAsync(Socket socket, byte[] buffer, int length)
    {
        var offset = 0;
        do
        {
            var write = await socket.SendAsync(new ArraySegment<byte>(buffer, offset, length - offset), SocketFlags.None).ConfigureAwait(false);
            if (write <= 0)
            {
                return -1;
            }

            offset += write;
        }
        while (offset < length);

        return offset;
    }
}
