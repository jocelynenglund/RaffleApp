using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Features.BuyTicket;

public class Handler
{
    private readonly IRaffleRepository _raffleRepository;
    
    public Handler(IRaffleRepository raffleRepository)
    {
        _raffleRepository = raffleRepository ?? throw new ArgumentNullException(nameof(raffleRepository));
    }

    public async Task<int> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        // Retrieve the raffle from repository
        var raffle = await _raffleRepository.GetByIdAsync(command.RaffleId, cancellationToken) ?? throw new InvalidOperationException($"Raffle with ID {command.RaffleId} not found.");

        var ticketId = raffle.Handle(command);
        // Save changes
        await _raffleRepository.SaveAsync(raffle, cancellationToken);

        return ticketId;
    }
}