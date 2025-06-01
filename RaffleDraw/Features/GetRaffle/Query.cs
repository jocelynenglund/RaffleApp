using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Features.GetRaffle;
public record Query(Guid RaffleId);

public class Handler
{
    private readonly IRaffleRepository _raffleRepository;
    public Handler(IRaffleRepository raffleRepository)
    {
        _raffleRepository = raffleRepository ?? throw new ArgumentNullException(nameof(raffleRepository));
    }
    public async Task<Raffle> HandleAsync(Query query, CancellationToken cancellationToken)
    {

        return await _raffleRepository.GetByIdAsync(query.RaffleId, cancellationToken);
    }
}