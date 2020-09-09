using System;
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

        public static IServiceProvider? ServiceProvider { get; private set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<FlightSimService>();
        }
    }
}
