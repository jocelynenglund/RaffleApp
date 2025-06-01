namespace RaffleDraw.Core.Common;

public abstract record DomainEvent {
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

    protected DomainEvent(TimeProvider? timeProvider = null)
    {
        timeProvider ??= TimeProvider.System;
        OccurredOn = timeProvider.GetUtcNow().UtcDateTime;
    }
}
