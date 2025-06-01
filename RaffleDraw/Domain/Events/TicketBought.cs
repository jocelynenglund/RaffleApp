using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Events;

public record TicketBought(string BuyerName, Guid RaffleId, int? TicketNumber = null) : DomainEvent;
