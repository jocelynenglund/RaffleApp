using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Domain.Events;
namespace RaffleApi.Endpoints;

public class GetRaffleRequest
{
    [FromRoute]
    public Guid Id { get; set; }
}
public class GetRaffleResponse
{
    public Guid Id { get; set;}
    public string Title { get; set;}
    public int NumberOfTickets { get; set;}
    public int AvailableTickets { get; set;}    
    public decimal TicketPrice { get; set;}
    public List<TicketInfo> BoughtTickets { get; set; } = [];
    public record TicketInfo(int Number, string HolderName);

}

public class GetRaffleEndpoint: Endpoint<GetRaffleRequest,GetRaffleResponse>
{
    public override void Configure()
    {
        Get("/raffles/{Id}");
        AllowAnonymous();
        Summary(x =>
        {
            x.Summary = "Get raffle by ID";
            x.Description = "Retrieves a raffle using its unique identifier";
            x.Response<GetRaffleResponse>(200, "Raffle found");
            x.Response(404, "Raffle not found");
        });

    }

    public override async Task HandleAsync(GetRaffleRequest req, CancellationToken ct)
    {
        
        var raffle = await Task.FromResult<Raffle>(Raffle.LoadFromHistory([new RaffleCreated()]); // Temporary placeholder

        if (raffle == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        var response = new GetRaffleResponse
        {
            Id = raffle.Id,
            Title = raffle.Title,
            AvailableTickets = raffle.AvailableTickets.Count,

            // Map bought tickets to response model
            BoughtTickets = [.. raffle.BoughtTickets.Select(t => new GetRaffleResponse.TicketInfo(t.Number, t.Name))]

            // Add any additional properties needed
        };

        await SendAsync(response, cancellation: ct);
    }
}