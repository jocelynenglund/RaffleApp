using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Commands;

public record BuyTicket(string BuyerName, int? TicketNumber = null): Command;

