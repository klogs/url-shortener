using Shortener.Application.Interfaces;
using Shortener.Domain.Events;

namespace Shortener.IntegrationTests.Infrastructure;

internal sealed class NullClickEventBuffer : IClickEventBuffer
{
    public bool TryWrite(ClickEvent evt) => true;
}
