using System.Threading.Channels;

namespace AutoNex.Services;

public class WhatsAppSendQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _channel
        = Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(100);

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> work)
    {
        _channel.Writer.TryWrite(work);
    }

    public IAsyncEnumerable<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
