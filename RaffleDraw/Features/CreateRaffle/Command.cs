using RaffleDraw.Core.Common;
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;

namespace RaffleDraw.Features.CreateRaffle;

public record Command: CommandBase
{
    public string Title { get; }
    public int NumberOfTickets { get; }
    public decimal Price { get; }
    public Command(string title, int numberOfTickets, decimal price)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        // Validate number of tickets
        if (numberOfTickets <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(numberOfTickets), "Number of tickets must be greater than zero.");
        }

        // Validate price
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(price), "Ticket price must be greater than zero.");
        }

        Title = title;
        NumberOfTickets = numberOfTickets;
        Price = price;
    }
}

public class Handler
{
    private readonly IRaffleRepository _raffleRepository;
    public Handler(IRaffleRepository raffleRepository)
    {
        _raffleRepository = raffleRepository ?? throw new ArgumentNullException(nameof(raffleRepository));
    }
    public async Task<Guid> HandleAsync(Command command, CancellationToken cancellationToken)
    {
        // Create a new Raffle instance
        var raffle = Raffle.Create(command);
            
        // Save the raffle to the repository
        await _raffleRepository.SaveAsync(raffle, cancellationToken);

        return raffle.Id;

    }
}


