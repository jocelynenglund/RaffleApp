using System.Linq;
using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Features.SelectWinner;

public class Handler
{
    private readonly IRaffleRepository _raffleRepository;

    public Handler(IRaffleRepository raffleRepository)
    {
        _raffleRepository = raffleRepository ?? throw new ArgumentNullException(nameof(raffleRepository));
    }

    public async Task<int> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        var raffle = await _raffleRepository.GetByIdAsync(command.RaffleId, cancellationToken)
                     ?? throw new InvalidOperationException($"Raffle with ID {command.RaffleId} not found.");

        raffle.Handle(command);

        await _raffleRepository.SaveAsync(raffle, cancellationToken);

        return raffle.SelectedTickets.Last().Number;
    }
}
