namespace Munin.Node.Service;

using System.Net.Sockets;

#pragma warning disable SYSLIB1006
public static partial class Log
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Listener started.")]
    public static partial void InfoListenerStarted(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Listener stopped.")]
    public static partial void InfoListenerStopped(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Error, Message = "Listener start failed.")]
    public static partial void ErrorListenerStartFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Accept failed.")]
    public static partial void ErrorAcceptFailed(this ILogger logger, Exception ex);

    [LoggerMessage(Level = LogLevel.Error, Message = "Accept failed. error=[{error}].")]
    public static partial void WarnAcceptFailed(this ILogger logger, SocketError error);

    [LoggerMessage(Level = LogLevel.Error, Message = "Connection error.")]
    public static partial void ErrorConnectionError(this ILogger logger, Exception ex);
}
#pragma warning restore SYSLIB1006
