using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;
using RaffleDraw.Features.SelectWinner;
using Shouldly;

namespace TheTests.Features;

public class SelectWinnerHandlerTests
{
    private class TestRepository : IRaffleRepository
    {
        private readonly Dictionary<Guid, Raffle> _store = new();

        public Task SaveAsync(Raffle raffle, CancellationToken cancellationToken)
        {
            _store[raffle.Id] = raffle;
            return Task.CompletedTask;
        }

        public Task<Raffle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            _store.TryGetValue(id, out var raffle);
            return Task.FromResult<Raffle?>(raffle);
        }

        public Task<IEnumerable<Raffle>> GetAllAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<Raffle>>(_store.Values);
        }
    }

    class DeterministicSelector : IWinnerSelector
    {
        public int ChooseWinner(IReadOnlyList<int> holders) => holders.Last();
    }

    [Fact]
    public async Task HandleAsync_SelectsWinnerAndReturnsTicketNumber()
    {
        var repo = new TestRepository();
        var raffle = Raffle.LoadFromHistory(
            [
                new RaffleDraw.Features.CreateRaffle.RaffleCreated("Test", 2, 10m),
                new RaffleDraw.Features.BuyTicket.TicketBought("Jane", Guid.NewGuid()),
                new RaffleDraw.Features.BuyTicket.TicketBought("John", Guid.NewGuid()),
            ],
            new DeterministicSelector()
        );
        await repo.SaveAsync(raffle, CancellationToken.None);

        var handler = new Handler(repo);
        var winning = await handler.HandleAsync(new(raffle.Id), CancellationToken.None);

        winning.ShouldBe(raffle.InitialTicketNumber + 1);
        var stored = await repo.GetByIdAsync(raffle.Id, CancellationToken.None);
        stored!.SelectedTickets.Count.ShouldBe(1);
        stored.SelectedTickets[0].Number.ShouldBe(winning);
    }
}
