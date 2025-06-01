using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Commands;

public record CreateRaffle(string title, int numberOfTickets, decimal price): Command;
