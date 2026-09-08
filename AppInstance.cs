namespace Dtdash;

using System.IO.Pipes;
using System.Text;

public static class AppInstance
{
    private const string ActivationPipeName = "dtDash.Activate";
    private static Mutex? _mutex;
    private static CancellationTokenSource? _activationCancellation;

    public static bool TryAcquire()
    {
        try
        {
            _mutex = new Mutex(true, "dtDash.SingleInstance", out var createdNew);
            if (createdNew) return true;

            _mutex.Dispose();
            _mutex = null;
            return false;
        }
        catch
        {
            _mutex?.Dispose();
            _mutex = null;
            return false;
        }
    }

    public static void Release()
    {
        try
        {
            _mutex?.ReleaseMutex();
        }
        catch (ApplicationException)
        {
        }
        finally
        {
            _mutex?.Dispose();
            _mutex = null;
        }
    }

    public static void SignalExistingInstance()
    {
        try
        {
            using var client = new NamedPipeClientStream(".", ActivationPipeName, PipeDirection.Out);
            client.Connect(2000);
            var message = Encoding.UTF8.GetBytes("ACTIVATE");
            client.Write(message, 0, message.Length);
        }
        catch
        {
        }
    }

    public static void StartActivationListener(Action activate)
    {
        _activationCancellation = new CancellationTokenSource();
        var cancellationToken = _activationCancellation.Token;
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var server = new NamedPipeServerStream(
                        ActivationPipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                    await server.WaitForConnectionAsync(cancellationToken);
                    using var reader = new StreamReader(server, Encoding.UTF8);
                    if (await reader.ReadToEndAsync(cancellationToken) == "ACTIVATE")
                        activate();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }, cancellationToken);
    }

    public static void StopActivationListener()
    {
        _activationCancellation?.Cancel();
        _activationCancellation?.Dispose();
        _activationCancellation = null;
    }
}
