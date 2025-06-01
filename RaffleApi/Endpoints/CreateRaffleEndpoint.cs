using FastEndpoints;
using RaffleDraw.Domain.Aggregates;

namespace RaffleApi.Endpoints;

public record CreateRaffleRequest(string Title, int NumberOfTickets, decimal Price);
public record CreateRaffleResponse(Guid Id);
public class CreateRaffleEndpoint: Endpoint<CreateRaffleRequest, CreateRaffleResponse>
{
    public override void Configure()
    {
        Post("/raffles");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new raffle";
            s.Description = "New Raffle with customizeable title, number of tickets and ticket price";
            s.Response<CreateRaffleResponse>(201, "Raffle created successfully");
            s.Response(400, "Invalid request");
        });
    }

    public override async Task HandleAsync(CreateRaffleRequest req, CancellationToken ct)
    {
        var raffle = Raffle.Create(req.Title, req.NumberOfTickets, req.Price);

        await SendCreatedAtAsync<GetRaffleEndpoint>(new {id = raffle.Id});

    } 
}
