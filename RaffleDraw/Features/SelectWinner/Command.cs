using RaffleDraw.Core;

namespace RaffleDraw.Features.SelectWinner;

public record Command(Guid RaffleId) : CommandBase;
