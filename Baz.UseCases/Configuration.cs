using Microsoft.Extensions.DependencyInjection;

namespace Baz.UseCases;

public static class Configuration
{
    public static void ConfigureUseCases(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Configuration).Assembly));
    }
}
