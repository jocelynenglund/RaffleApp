
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Ports;
using RaffleDraw.Features.BuyTicket;
using RaffleDraw.Features.CreateRaffle;
using RaffleDraw.Features.SelectWinner;
using Shouldly;

namespace TheTests.Domain.Aggregates;

public class RaffleTests

{
    [Fact]
    public void ShouldSetTitle_WhenCreateCommandIsTriggered()
    {
        var raffle = Raffle.Create(new("My Raffle", 100, 100.00m));
        raffle.Title.ShouldBe("My Raffle");
    }

    [Fact]
    public void ShouldListAvailableTickets_WhenRaffleIsCreated()
    {
        var raffle = Raffle.LoadFromHistory(
         [
             new RaffleCreated("My Raffle", 1, 100.00m)
         ]);

        raffle.AvailableTickets.Count.ShouldBe(1);
    }
    [Fact]
    public void ShouldAddToEligibleWinners_WhenTicketIsBought() { 
    
        var raffle = Raffle.Create(new("My Raffle", 100, 100.00m));

        var ticketNumber = raffle.Handle(new RaffleDraw.Features.BuyTicket.Command("Jane Doe"));

        ticketNumber.ShouldBe(raffle.InitialTicketNumber);
        raffle.BoughtTickets.Count.ShouldBe(1);
        raffle.BoughtTickets[0].Name.ShouldBe("Jane Doe");
        raffle.AvailableTickets.Count.ShouldBe(99);
    }

    [Fact]
    public void ShouldThrowException_whenJoiningRaffleWithNoTicketsLeft()
    {
        var raffle = Raffle.LoadFromHistory(
        [
            new RaffleCreated("My Raffle", 1, 100.00m),
            new TicketBought("Jane Doe", Guid.NewGuid())
        ]);
        Should.Throw<InvalidOperationException>(() => raffle.Handle(new RaffleDraw.Features.BuyTicket.Command("John Doe")));
    }

    class LastManWinnerSelector : IWinnerSelector
    {
        public int ChooseWinner(IReadOnlyList<int> holders)
        {
            return holders.Last();
        }
    }

    [Fact]
    public void ShouldSelectARandomWinner_WhenWinnerSelectionIsTriggered()
    {
        var raffle = Raffle.LoadFromHistory(
        [
            new RaffleCreated("My Raffle", 100, 100.00m),
            new TicketBought("Jane Doe", Guid.NewGuid()),
            new TicketBought("John Doe", Guid.NewGuid()),
            new TicketBought("Joseph Doe", Guid.NewGuid())
        ], new LastManWinnerSelector());

        raffle.Handle(new RaffleDraw.Features.SelectWinner.Command(raffle.Id));

        raffle.UncommittedChanges.Count.ShouldBe(1);

        raffle.UncommittedChanges.OfType<WinnerSelected>()
            .Any(winnerSelected => winnerSelected.TicketNumber == raffle.InitialTicketNumber+2)
            .ShouldBeTrue();
        raffle.SelectedTickets
            .Any(ticket => ticket.Name == "Joseph Doe")
            .ShouldBeTrue();
    }

    [Fact]
    public void SelectWinner_ShouldThrowException_IfNoTicketsHaveBeenBought()
    {
        var raffle = Raffle.LoadFromHistory(
        [
           new RaffleCreated("My Raffle", 1, 100.00m)
        ]);


        Should.Throw<InvalidOperationException>(() => raffle.Handle(new RaffleDraw.Features.SelectWinner.Command(raffle.Id)));

    }

}

