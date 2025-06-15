namespace RaffleDraw.Core;

public abstract class AggregateRoot<TEvent>
{
    private readonly List<TEvent> _changes = new();
    protected Dictionary<Type, Action<TEvent>> Handlers { get; init; } = [];
    public IReadOnlyList<TEvent> UncommittedChanges => _changes;

    /// <summary>
    /// Used in command handlers: applies state + buffers for persistence.
    /// </summary>
    protected void RaiseEvent(TEvent @event)
    {
        ApplyEvent(@event);
        _changes.Add(@event);
    }

    public void ClearUncommitted() => _changes.Clear();

    public void Rehydrate(IEnumerable<TEvent> history)
    {
        foreach (var e in history)
            ApplyEvent(e);
    }

    protected virtual void ApplyEvent(TEvent @event)
    {
        if (Handlers.TryGetValue(@event.GetType(), out var handler))
        {
            handler(@event);
        }
        else
        {
            throw new InvalidOperationException(
                $"No handler registered for event type {@event.GetType().Name}."
            );
        }
    }
}
