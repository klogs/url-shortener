using Shortener.Domain.Events;

namespace Shortener.Application.Interfaces;

/// <summary>
/// Non-blocking, fire-and-forget write into the in-process analytics buffer.
/// Returns false (and drops the event) when the buffer is full — never throws.
/// </summary>
public interface IClickEventBuffer
{
    bool TryWrite(ClickEvent evt);
}
