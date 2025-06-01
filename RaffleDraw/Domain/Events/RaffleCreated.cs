using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Events;

public record RaffleCreated(string Title, int NumberOfTickets, decimal Price) : DomainEvent;
