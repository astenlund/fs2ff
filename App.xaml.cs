using System;
using System.Reflection;
using fs2ff.FlightSim;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace fs2ff
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public App()
        {
            var host = new HostBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            ServiceProvider = host.Services;
        }

        public static Version AssemblyVersion => Assembly.GetEntryAssembly()!.GetName().Version!;

        public static string InformationalVersion => "v" + Assembly
            .GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "";

        public static IServiceProvider? ServiceProvider { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<FlightSimAdapter>();
            services.AddSingleton<NetworkAdapter>();
        }
    }
}
