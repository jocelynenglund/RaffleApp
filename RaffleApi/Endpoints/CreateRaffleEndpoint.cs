using FastEndpoints;
using RaffleDraw.Domain.Aggregates;
using RaffleDraw.Features.CreateRaffle;

namespace RaffleApi.Endpoints;

public record CreateRaffleRequest(string Title, int NumberOfTickets, decimal Price);
public record CreateRaffleResponse(Guid Id);
public class CreateRaffleEndpoint(Handler handler): Endpoint<CreateRaffleRequest, CreateRaffleResponse>
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
        var result = handler.HandleAsync(new Command(req.Title, req.NumberOfTickets, req.Price), ct); 

        await SendCreatedAtAsync<GetRaffleEndpoint>(new {id = result});
    } 
}
