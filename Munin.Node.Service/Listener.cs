namespace Munin.Node.Service;

using System.Net;
using System.Net.Sockets;

internal sealed class Listener : IDisposable
{
    private readonly ILogger<Listener> logger;

    private readonly object sync = new();

    private readonly EndPoint endPoint;

    private readonly SocketAsyncEventArgs saea;

    private Socket? listener;

    public event Action<Socket>? OnClientAccepted;

    public Listener(ILogger<Listener> logger, EndPoint endPoint)
    {
        this.logger = logger;
        this.endPoint = endPoint;
        saea = new SocketAsyncEventArgs();
        saea.Completed += AcceptCompleted;
    }

    public void Dispose()
    {
        Stop();
        saea.Dispose();
    }

    public void Start()
    {
        lock (sync)
        {
            if (listener is not null)
            {
                return;
            }

            try
            {
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen();
            }
            catch (SocketException ex)
            {
                logger.LogError(ex, "Listener start failed");

                listener?.Dispose();
                listener = null;
                return;
            }
        }

        logger.LogInformation("Listener started.");

        StartAccept();
    }

    public void Stop()
    {
        lock (sync)
        {
            if (listener is null)
            {
                return;
            }

            listener.Dispose();
            listener = null;
        }

        logger.LogInformation("Listener stopped.");
    }

    private void StartAccept()
    {
        lock (sync)
        {
            saea.AcceptSocket = null;

            if ((listener is not null) && !listener.AcceptAsync(saea))
            {
                ProcessAccept(saea);
            }
        }
    }

    private void AcceptCompleted(object? sender, SocketAsyncEventArgs e)
    {
        if (e.LastOperation == SocketAsyncOperation.Accept)
        {
            ProcessAccept(e);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1031: not catch general exception types", Justification = "Ignore")]
    private void ProcessAccept(SocketAsyncEventArgs e)
    {
        if (e.SocketError == SocketError.Success)
        {
            try
            {
                OnClientAccepted?.Invoke(e.AcceptSocket!);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Accept failed.");
            }

            StartAccept();
        }
        else
        {
            logger.LogWarning("Accept failed. error=[{Error}]", e.SocketError);
        }
    }
}
