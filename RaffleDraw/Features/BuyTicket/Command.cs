using RaffleDraw.Core.Common;

namespace RaffleDraw.Features.BuyTicket;

public record Command(string BuyerName, int? TicketNumber = null): CommandBase;

