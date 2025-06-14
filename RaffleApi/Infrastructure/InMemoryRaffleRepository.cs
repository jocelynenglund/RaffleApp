using RaffleDraw.Core.Common;
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;
using System.Collections.Concurrent;

namespace RaffleApi.Infrastructure;

public class InMemoryRaffleRepository : IRaffleRepository
{
    // State-based storage
    private readonly ConcurrentDictionary<Guid, Raffle> _raffleStates = new();

    // Event-sourced storage
    private readonly ConcurrentDictionary<Guid, List<DomainEvent>> _raffleEvents = new();

    public Task SaveAsync(Raffle raffle, CancellationToken cancellationToken)
    {
        // Save state
        _raffleStates[raffle.Id] = raffle;

        // Save events (assuming AggregateRoot exposes uncommitted events)
        if (!_raffleEvents.ContainsKey(raffle.Id))
            _raffleEvents[raffle.Id] = new List<DomainEvent>();

        // This assumes you have a way to get uncommitted events from the aggregate
        var newEvents = raffle.UncommittedChanges; // Implement this in your AggregateRoot
        _raffleEvents[raffle.Id].AddRange(newEvents);

        raffle.ClearUncommitted();

        return Task.CompletedTask;
    }

    public Task<Raffle?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {

        if (_raffleEvents.TryGetValue(id, out var events))
            return Task.FromResult<Raffle?>(Raffle.LoadFromHistory(events));
        // Option 1: Return state-based
        if (_raffleStates.TryGetValue(id, out var raffle))
            return Task.FromResult<Raffle?>(raffle);

        // Option 2: Rehydrate from events

        return Task.FromResult<Raffle?>(null);
    }

    public Task<IEnumerable<Raffle>> GetAllAsync(CancellationToken cancellationToken)
    {
        // Return all state-based raffles
        return Task.FromResult<IEnumerable<Raffle>>(_raffleStates.Values);
    }

    // Optionally, expose event streams for testing or diagnostics
    public IEnumerable<DomainEvent> GetEventsForRaffle(Guid id)
        => _raffleEvents.TryGetValue(id, out var events) ? events : Enumerable.Empty<DomainEvent>();
}