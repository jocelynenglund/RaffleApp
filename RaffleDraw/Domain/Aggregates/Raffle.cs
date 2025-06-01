using RaffleDraw.Domain.Common;
using System.Collections.ObjectModel;

namespace RaffleDraw.Domain.Aggregates;

public class Raffle: AggregateRoot<IEvent>
{
    private int _numberOfTickets;
    private decimal _ticketPrice;
    List<string> _ticketHolders = new List<string>();
    public ReadOnlyCollection<string> EligibleWinners => _ticketHolders.AsReadOnly(); 

    public Guid Id { get; private set; }
    public string Title { get; internal set; }

    private Raffle() {
        Title = "Default Title";
    }
    private Raffle(params IEvent[] events) => base.Rehydrate(events.AsEnumerable());


    internal static Raffle Create(string title, int numberOfTickets, decimal price)
    {
        var raffle = new Raffle();
        raffle.Handle(new CreateRaffle(title, numberOfTickets, price));
        return raffle;
    }

    internal static Raffle LoadFromHistory(IEnumerable<IEvent> events)
    {
        var raffle = new Raffle(events.ToArray());
        return raffle;
    }

    internal void Handle(CreateRaffle createRaffle)
    {
        if (string.IsNullOrWhiteSpace(createRaffle.title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(createRaffle.title));
        }
        if (createRaffle.numberOfTickets <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(createRaffle.numberOfTickets), "Number of tickets must be greater than zero.");
        }
        if (createRaffle.price <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(createRaffle.price), "Ticket price must be greater than zero.");
        }
        RaiseEvent(new RaffleCreated(createRaffle.title, createRaffle.numberOfTickets, createRaffle.price));
    }

    internal void Handle(BuyTicket buyTicket)
    {
        if (_numberOfTickets <= 0)
        {
            throw new InvalidOperationException("No tickets left to buy.");
        }
        if (string.IsNullOrWhiteSpace(buyTicket.BuyerName))
        {
            throw new ArgumentException("Buyer name cannot be empty.", nameof(buyTicket.BuyerName));
        }
        RaiseEvent(new TicketBought(buyTicket.BuyerName, Id));
    }
    internal void Handle(SelectWinner selectWinner)
    {
        if (_ticketHolders.Count == 0)
        {
            throw new InvalidOperationException("No tickets have been bought. Cannot select a winner.");
        }
        var random = new Random();
        int winnerIndex = random.Next(_ticketHolders.Count);
        string winner = _ticketHolders[winnerIndex];
 
        RaiseEvent(new WinnerSelected(winner, Id));
    }

    private void When(RaffleCreated @event)
    {
        Id = Guid.NewGuid();
        Title = @event.Title;
        _numberOfTickets = @event.NumberOfTickets;
        _ticketPrice = @event.Price;
    }
    private void When(TicketBought @event)
    {
        _ticketHolders.Add(@event.BuyerName);
        _numberOfTickets--;
    }

    private void When(WinnerSelected @event)
    {
        _ticketHolders.Remove(@event.Name);
    }

    protected override void ApplyEvent(IEvent @event)
    {
        When((dynamic)@event);
    }

  
}

public interface IEvent { }
public interface  ICommand { }
public record RaffleCreated(string Title, int NumberOfTickets, decimal Price) : IEvent;
public record TicketBought(string BuyerName, Guid RaffleId) : IEvent;
public record WinnerSelected(string Name, Guid RaffleID): IEvent;
public record CreateRaffle(string title, int numberOfTickets, decimal price);
public record BuyTicket(string BuyerName): ICommand;
public record SelectWinner(): ICommand;