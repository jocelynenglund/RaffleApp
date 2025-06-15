using RaffleDraw.Core;

namespace RaffleDraw.Features.CreateRaffle;

public record RaffleCreated(string Title, int NumberOfTickets, decimal Price, Guid Id) : DomainEvent
{
    public RaffleCreated(string title, int numberOfTickets, decimal price)
        : this(title, numberOfTickets, price, Guid.NewGuid()) { }

    public RaffleCreated()
        : this("Default Title", 1000, 10.0m) { }
}
