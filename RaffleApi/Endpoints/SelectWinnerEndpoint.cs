using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using RaffleDraw.Features.SelectWinner;

namespace RaffleApi.Endpoints;

public class SelectWinnerRequest
{
    [FromRoute]
    public Guid Id { get; set; }
}

public record SelectWinnerResponse(int TicketNumber);

public class SelectWinnerEndpoint(Handler handler) : Endpoint<SelectWinnerRequest, SelectWinnerResponse>
{
    public override void Configure()
    {
        Post("/raffles/{id}/select-winner");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Select a winner for a raffle";
            s.Description = "Chooses a winning ticket for the specified raffle";
            s.Response<SelectWinnerResponse>(200, "Winner selected");
            s.Response(400, "No tickets purchased");
            s.Response(404, "Raffle not found");
        });
    }

    public override async Task HandleAsync(SelectWinnerRequest req, CancellationToken ct)
    {
        try
        {
            var ticketNumber = await handler.HandleAsync(new Command(req.Id), ct);
            await SendAsync(new SelectWinnerResponse(ticketNumber), cancellation: ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("No tickets"))
        {
            AddError(ex.Message);
            await SendErrorsAsync(StatusCodes.Status400BadRequest, cancellation: ct);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            await SendNotFoundAsync(ct);
        }
    }
}
