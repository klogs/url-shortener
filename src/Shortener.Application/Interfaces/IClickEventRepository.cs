using Shortener.Domain.Events;

namespace Shortener.Application.Interfaces;

public interface IClickEventRepository
{
    Task InsertAsync(ClickEvent evt, CancellationToken ct = default);
}
