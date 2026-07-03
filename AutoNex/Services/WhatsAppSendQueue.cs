using System.Threading.Channels;

namespace AutoNex.Services;

public class QueuedMessage
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N");
    public required string Phone { get; init; }
    public required string Message { get; init; }
    public string Source { get; init; } = string.Empty;
    public string SentBy { get; init; } = string.Empty;
}

public class WhatsAppSendQueue
{
    private readonly Channel<QueuedMessage> _messageChannel = Channel.CreateBounded<QueuedMessage>(100);
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _workChannel =
        Channel.CreateBounded<Func<IServiceProvider, CancellationToken, Task>>(100);

    public string Enqueue(string phone, string message, string source, string sentBy)
    {
        var msg = new QueuedMessage
        {
            Phone = phone,
            Message = message,
            Source = source,
            SentBy = sentBy,
        };
        _messageChannel.Writer.TryWrite(msg);
        return msg.Id;
    }

    public void Enqueue(Func<IServiceProvider, CancellationToken, Task> work)
    {
        _workChannel.Writer.TryWrite(work);
    }

    public IAsyncEnumerable<QueuedMessage> DequeueMessagesAsync(CancellationToken ct)
        => _messageChannel.Reader.ReadAllAsync(ct);

    public IAsyncEnumerable<Func<IServiceProvider, CancellationToken, Task>> DequeueWorkAsync(CancellationToken ct)
        => _workChannel.Reader.ReadAllAsync(ct);
}
