namespace RaffleDraw;
public static class Extensions
{
    public static IServiceCollection InstallRaffle(this IServiceCollection services)
    {
        // Register the RaffleDraw services
        services.AddScoped<Features.CreateRaffle.Handler>();
        services.AddScoped<Features.GetRaffle.Handler>();
        services.AddSingleton<Domain.Ports.IRaffleRepository, RaffleApi.Infrastructure.InMemoryRaffleRepository>();
        // Add any additional registrations needed for RaffleDraw
        return services;
    }
}
