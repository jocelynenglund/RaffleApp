using RaffleDraw.Core.Common;

namespace RaffleDraw.Features.SelectWinner;

public record Command(Guid RaffleId): CommandBase;
