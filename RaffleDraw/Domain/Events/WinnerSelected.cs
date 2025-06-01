using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Events;

public record WinnerSelected(int TicketNumber, Guid RaffleID): DomainEvent;
