using FastEndpoints;
using RaffleDraw.Features.BuyTicket;

namespace RaffleApi.Endpoints;

public record BuyTicketRequest
{
    public string HolderName { get; init; }
    public int? TicketNumber { get; init; } // Optional, can be null if not specified
}

public record BuyTicketResponse
{
    public int TicketNumber { get; init; }
    public string HolderName { get; init; }
    public Guid RaffleId { get; init; }
}

public class BuyTicketEndpoint(Handler handler) : Endpoint<BuyTicketRequest, BuyTicketResponse>
{
    public override void Configure()
    {
        Post("/raffles/{id}/tickets");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Buy a ticket for a raffle";
            s.Description = "Purchases a ticket for the specified raffle";
            s.Response<BuyTicketResponse>(201, "Ticket purchased successfully");
            s.Response(400, "Invalid request");
            s.Response(404, "Raffle not found");
            s.Response(409, "No tickets available");
        });
    }

    public override async Task HandleAsync(BuyTicketRequest req, CancellationToken ct)
    {
        // Extract the raffle ID from the route
        var raffleId = Route<Guid>("id");

        try
        {
            var result = await handler.HandleAsync(
                new Command(raffleId, req.HolderName, req.TicketNumber),
                ct
            );

            var response = new BuyTicketResponse
            {
                TicketNumber = result,
                HolderName = req.HolderName,
                RaffleId = raffleId,
            };

            await SendCreatedAtAsync<GetRaffleEndpoint>(
                new { id = raffleId },
                response,
                cancellation: ct
            );
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No available tickets"))
        {
            AddError(ex.Message);
            await SendErrorsAsync(StatusCodes.Status409Conflict, cancellation: ct);
        }
        catch (Exception ex)
        {
            AddError(ex.Message);
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
        }
    }
}
