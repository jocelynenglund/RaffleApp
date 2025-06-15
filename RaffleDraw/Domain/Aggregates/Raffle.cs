using System.Collections.ObjectModel;
using RaffleDraw.Core.Common;
using RaffleDraw.Domain.Ports;
using RaffleDraw.Domain.Services;
using RaffleDraw.Features.BuyTicket;
using RaffleDraw.Features.CreateRaffle;
using RaffleDraw.Features.SelectWinner;

namespace RaffleDraw.Domain.Aggregates;

public class Raffle : AggregateRoot<DomainEvent>
{
    public decimal TicketPrice { get; private set; }

    public int InitialTicketNumber => 1000; // Starting point for ticket numbers

    private List<Ticket> _availableTickets = [];
    public ReadOnlyCollection<Ticket> AvailableTickets => _availableTickets.AsReadOnly();

    private List<Ticket> _boughtTickets = [];
    public ReadOnlyCollection<Ticket> BoughtTickets => _boughtTickets.AsReadOnly();

    private List<Ticket> _selectedTickets = [];
    public ReadOnlyCollection<Ticket> SelectedTickets => _selectedTickets.AsReadOnly();

    public Guid Id { get; private set; }
    public string Title { get; internal set; }

    private int _numberOfTickets;
    private IWinnerSelector _winnerSelector;
    private readonly Dictionary<Type, Action<DomainEvent>> _handlers;

    private Raffle(IWinnerSelector winnerSelector = null)
    {
        Title = "Default Title";
        _winnerSelector = winnerSelector ?? new RandomWinnerSelector();
        _handlers = new()
        {
            { typeof(RaffleCreated), ev => When((RaffleCreated)ev) },
            { typeof(TicketBought), ev => When((TicketBought)ev) },
            { typeof(WinnerSelected), ev => When((WinnerSelected)ev) },
        };
    }

    private Raffle(IWinnerSelector winnerSelector, params DomainEvent[] events)
        : this(winnerSelector) => base.Rehydrate(events.AsEnumerable());

    public static Raffle Create(
        Features.CreateRaffle.Command createRaffleCommand,
        IWinnerSelector winnerSelector = null
    )
    {
        var raffle = new Raffle(winnerSelector);
        raffle.Handle(createRaffleCommand);
        return raffle;
    }

    public static Raffle LoadFromHistory(
        IEnumerable<DomainEvent> events,
        IWinnerSelector winnerSelector = null
    )
    {
        var raffle = new Raffle(winnerSelector, [.. events]);
        return raffle;
    }

    internal void Handle(Features.CreateRaffle.Command createRaffle)
    {
        if (string.IsNullOrWhiteSpace(createRaffle.Title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(createRaffle.Title));
        }
        if (createRaffle.NumberOfTickets <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createRaffle.NumberOfTickets),
                "Number of tickets must be greater than zero."
            );
        }
        if (createRaffle.Price <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(createRaffle.Price),
                "Ticket price must be greater than zero."
            );
        }
        RaiseEvent(
            new RaffleCreated(createRaffle.Title, createRaffle.NumberOfTickets, createRaffle.Price)
        );
    }

    internal int Handle(Features.BuyTicket.Command buyTicket)
    {
        if (_availableTickets.Count <= 0)
        {
            throw new InvalidOperationException("No tickets left to buy.");
        }
        if (string.IsNullOrWhiteSpace(buyTicket.BuyerName))
        {
            throw new ArgumentException("Buyer name cannot be empty.", nameof(buyTicket.BuyerName));
        }
        int ticketNumber = buyTicket.TicketNumber ?? _availableTickets.First().Number;
        if (!_availableTickets.Any(t => t.Number == ticketNumber))
        {
            // If requested ticket isn't available, throw or default to first available
            throw new InvalidOperationException($"Ticket number {ticketNumber} is not available.");
            // Alternative approach: ticketNumber = _availableTickets.First().Number;
        }

        RaiseEvent(new TicketBought(buyTicket.BuyerName, Id, ticketNumber));

        return ticketNumber;
    }

    internal void Handle(Features.SelectWinner.Command selectWinner)
    {
        if (_boughtTickets.Count == 0)
        {
            throw new InvalidOperationException(
                "No tickets have been bought. Cannot select a winner."
            );
        }

        var unselectedTickets = _boughtTickets.Except(_selectedTickets).ToList();

        if (unselectedTickets.Count == 0)
        {
            throw new InvalidOperationException("No unselected tickets remain.");
        }

        var winner = _winnerSelector.ChooseWinner([.. unselectedTickets.Select(t => t.Number)]);

        var winningTicket =
            unselectedTickets.FirstOrDefault(x => x.Number == winner)
            ?? throw new InvalidOperationException($"Ticket number {winner} does not exist.");

        RaiseEvent(new WinnerSelected(winningTicket.Number, Id));
    }

    private void When(RaffleCreated @event)
    {
        Id = @event.Id;
        Title = @event.Title;
        _numberOfTickets = @event.NumberOfTickets;
        this.TicketPrice = @event.Price;

        _availableTickets =
        [
            .. Enumerable.Range(InitialTicketNumber, _numberOfTickets).Select(x => new Ticket(x)),
        ];
    }

    private void When(TicketBought @event)
    {
        var ticket =
            _availableTickets.Find(t => t.Number == @event.TicketNumber)
            ?? _availableTickets.First();

        ticket.Name = @event.BuyerName;
        _availableTickets.Remove(ticket);
        _boughtTickets.Add(ticket);
    }

    private void When(WinnerSelected @event)
    {
        var ticket =
            _boughtTickets.FirstOrDefault(t => t.Number == @event.TicketNumber)
            ?? throw new InvalidOperationException(
                $"Ticket number {@event.TicketNumber} does not exist."
            );

        _selectedTickets.Add(ticket);
    }

    protected override void ApplyEvent(DomainEvent @event)
    {
        if (_handlers.TryGetValue(@event.GetType(), out var handler))
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

public record Ticket(int Number)
{
    public string Name { get; set; } = string.Empty;
}
