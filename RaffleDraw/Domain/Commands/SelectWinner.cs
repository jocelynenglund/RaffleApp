using RaffleDraw.Domain.Common;

namespace RaffleDraw.Domain.Commands;

public record SelectWinner(Guid RaffleId): Command;
