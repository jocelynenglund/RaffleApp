namespace RaffleDraw.Domain.Common;

public abstract record DomainEvent {
    public DateTime OccurredOn { get; } = DateTime.UtcNow;

    protected DomainEvent(TimeProvider? timeProvider = null)
    {
        timeProvider ??= TimeProvider.System;
        OccurredOn = timeProvider.GetUtcNow().UtcDateTime;
    }
}
