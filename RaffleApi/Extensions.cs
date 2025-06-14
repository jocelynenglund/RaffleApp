using RaffleApi.Infrastructure;
using RaffleDraw.Domain.Ports;

namespace RaffleDraw;

public static class Extensions
{
    public static IServiceCollection InstallRaffle(this IServiceCollection services)
    {
        services.AddScoped<Features.CreateRaffle.Handler>();
        services.AddScoped<Features.GetRaffle.Handler>();
        services.AddScoped<Features.BuyTicket.Handler>();
        services.AddScoped<Features.SelectWinner.Handler>();

        services.AddSingleton<IRaffleRepository>(sp =>
        {
            var storageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage");
            // Ensure storage directory exists
            if (!Directory.Exists(storageDirectory))
            {
                Directory.CreateDirectory(storageDirectory);
            }
            return new FileBasedRaffleRepository(storageDirectory);
        });

        return services;
    }
}
