
using RaffleDraw.Domain.Aggregates;
using Shouldly;

namespace TheTests.Domain.Aggregates;

public class RaffleTests

{
    [Fact]
    public void ShouldSetTitle_WhenCreateCommandIsTriggered()
    {
        var raffle = Raffle.Create("My Raffle", 100, 100.00m);
        raffle.Title.ShouldBe("My Raffle");
    }

    [Fact]
    public void ShouldAddToEligibleWinners_WhenTicketIsBought() { 
    
        var raffle = Raffle.Create("My Raffle", 100, 100.00m);

        raffle.Handle(new BuyTicket("Jane Doe"));

        raffle.EligibleWinners.Count.ShouldBe(1);
        raffle.EligibleWinners[0].ShouldBe("Jane Doe");
    }

    [Fact]
    public void ShouldThrowException_whenJoiningRaffleWithNoTicketsLeft()
    {
        var raffle = Raffle.LoadFromHistory(
        [
            new RaffleCreated("My Raffle", 1, 100.00m),
            new TicketBought("Jane Doe", Guid.NewGuid())
        ]);
        Should.Throw<InvalidOperationException>(() => raffle.Handle(new BuyTicket("John Doe")));
    }

    [Fact]
    public void ShouldSelectARandomWinner_WhenWinnerSelectionIsTriggered()
    {
        var raffle = Raffle.LoadFromHistory(
        [
            new RaffleCreated("My Raffle", 1, 100.00m),
            new TicketBought("Jane Doe", Guid.NewGuid()),
            new TicketBought("John Doe", Guid.NewGuid()),
            new TicketBought("Joseph Doe", Guid.NewGuid())
        ]);

        raffle.Handle(new SelectWinner());

        raffle.UncommittedChanges.Count.ShouldBe(1);

        raffle.UncommittedChanges.ShouldContain(e => e is WinnerSelected);
    }

    [Fact]
    public void ShouldThrowException_IfNoTicketsHaveBeenBought()
    {
        var raffle = Raffle.LoadFromHistory(
        [
           new RaffleCreated("My Raffle", 1, 100.00m)
        ]);

        Should.Throw<InvalidOperationException>(() => raffle.Handle(new SelectWinner()));

    }

}

