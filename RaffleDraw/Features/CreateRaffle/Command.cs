using RaffleDraw.Core;

namespace RaffleDraw.Features.CreateRaffle;

public record Command : CommandBase
{
    public string Title { get; }
    public int NumberOfTickets { get; }
    public decimal Price { get; }

    public Command(string title, int numberOfTickets, decimal price)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title cannot be empty.", nameof(title));
        }

        // Validate number of tickets
        if (numberOfTickets <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numberOfTickets),
                "Number of tickets must be greater than zero."
            );
        }

        // Validate price
        if (price <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(price),
                "Ticket price must be greater than zero."
            );
        }

        Title = title;
        NumberOfTickets = numberOfTickets;
        Price = price;
    }
}
