using System;
using System.Linq;
using System.Threading.Tasks;
using CliFx;
using Microsoft.Extensions.DependencyInjection;
using MoreLinq.Extensions;

namespace CsprojToAsmdef.Cli
{
    public static class Program
    {
        public static async Task Main(string[] args) =>
            await new CliApplicationBuilder()
                .AddCommandsFromThisAssembly()
                .UseTypeActivator(CreateTypeActivator())
                .Build()
                .RunAsync(args);

        private static Func<Type, object> CreateTypeActivator() => new ServiceCollection()
            .AddCommands()
            .AddServices()
            .BuildServiceProvider()
            .GetService;

        private static IServiceCollection AddCommands(this IServiceCollection services)
        {
            typeof(Program)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(ICommand).IsAssignableFrom(t))
                .ForEach(t => services.AddTransient(t));

            return services;
        }

        private static IServiceCollection AddServices(this IServiceCollection services) =>
            services
                .AddTransient<CreateAsmdefForProject.IDotNetTooling, CreateAsmdefForProject.DotNetTooling>();
    }
}
