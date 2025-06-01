using RaffleDraw.Core.Common;

namespace RaffleDraw.Features.BuyTicket;

public record TicketBought(string BuyerName, Guid RaffleId, int? TicketNumber = null) : DomainEvent;
