using RaffleDraw.Domain.Aggregates;

namespace RaffleDraw.Domain.Ports;

public interface IRaffleRepository
{
    Task SaveAsync(Raffle raffle, CancellationToken cancellationToken);
    Task<Raffle?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IEnumerable<Raffle>> GetAllAsync(CancellationToken cancellationToken);
}
