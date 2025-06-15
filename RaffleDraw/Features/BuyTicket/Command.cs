using RaffleDraw.Core;

namespace RaffleDraw.Features.BuyTicket;

public record Command(Guid RaffleId, string BuyerName, int? TicketNumber = null) : CommandBase;
