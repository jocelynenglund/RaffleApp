using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RaffleApi.Infrastructure;
using RaffleDraw.Domain.Ports;
using Shouldly;

namespace TheTests.Integration;

public class RaffleApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(IRaffleRepository)
            );
            if (descriptor != null)
                services.Remove(descriptor);
            services.AddSingleton<IRaffleRepository, InMemoryRaffleRepository>();
        });
    }
}

public class WinnerEndpointTests : IClassFixture<RaffleApiFactory>
{
    private readonly HttpClient _client;

    public WinnerEndpointTests(RaffleApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SelectingWinnerShouldAppearInGetResponse()
    {
        var create = await _client.PostAsJsonAsync(
            "/raffles",
            new
            {
                Title = "Test",
                NumberOfTickets = 1,
                Price = 5m,
            }
        );
        create.EnsureSuccessStatusCode();
        var location = create.Headers.Location!.ToString();

        await _client.PostAsJsonAsync($"{location}/tickets", new { HolderName = "Jane" });
        var win = await _client.PostAsync($"{location}/winner", null);
        win.EnsureSuccessStatusCode();
        var winner = await win.Content.ReadFromJsonAsync<SelectWinnerResponse>();

        var raffle = await _client.GetFromJsonAsync<GetRaffleResponse>(location);

        raffle!.SelectedTickets.ShouldContain(t =>
            t.Number == winner!.TicketNumber && t.HolderName == "Jane"
        );
    }

    private record SelectWinnerResponse(int TicketNumber);

    private record TicketInfo(int Number, string HolderName);

    private record GetRaffleResponse(
        Guid Id,
        string Title,
        int NumberOfTickets,
        int AvailableTickets,
        decimal TicketPrice,
        List<TicketInfo> BoughtTickets,
        List<TicketInfo> SelectedTickets
    );
}
