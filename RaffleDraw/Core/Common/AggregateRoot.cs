namespace RaffleDraw.Core.Common;

public abstract class AggregateRoot<TEvent>
{
    private readonly List<TEvent> _changes = new();
    public IReadOnlyList<TEvent> UncommittedChanges => _changes;

    protected abstract void ApplyEvent(TEvent @event);
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
        foreach (var e in history) ApplyEvent(e);
    }
}
