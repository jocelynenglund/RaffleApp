using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Features.CreateRaffle;

public class Handler
{
    private readonly IRaffleRepository _raffleRepository;
    public Handler(IRaffleRepository raffleRepository)
    {
        _raffleRepository = raffleRepository ?? throw new ArgumentNullException(nameof(raffleRepository));
    }
    public async Task<Guid> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var raffle = Raffle.Create(command);
            
        await _raffleRepository.SaveAsync(raffle, cancellationToken);

        return raffle.Id;

    }
}


