using System.Threading.Channels;
using Microsoft.Extensions.Options;
using Shortener.Application.Interfaces;
using Shortener.Application.Options;
using Shortener.Domain.Events;

namespace Shortener.Infrastructure.Analytics;

/// <summary>
/// Singleton in-process buffer. TryWrite is non-blocking; drops events when full.
/// </summary>
public sealed class ClickEventChannel : IClickEventBuffer
{
    private readonly Channel<ClickEvent> _channel;

    public ClickEventChannel(IOptions<ClickEventOptions> opts)
    {
        _channel = Channel.CreateBounded<ClickEvent>(new BoundedChannelOptions(opts.Value.ChannelCapacity)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            AllowSynchronousContinuations = false,
        });
    }

    public bool TryWrite(ClickEvent evt) => _channel.Writer.TryWrite(evt);

    internal ChannelReader<ClickEvent> Reader => _channel.Reader;
}
