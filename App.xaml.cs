using System;
using System.Reflection;
using System.Windows;
using fs2ff.SimConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace fs2ff
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private static readonly IHost Host = new HostBuilder()
            .ConfigureServices(ConfigureServices)
            .Build();

        protected override async void OnExit(ExitEventArgs e)
        {
            await Host.StopAsync(TimeSpan.FromSeconds(3));
            Host.Dispose();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host.StartAsync();
            base.OnStartup(e);
        }

        public static Version AssemblyVersion => Assembly.GetEntryAssembly()!.GetName().Version!;

        public static string InformationalVersion => "v" + (Assembly
            .GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0");

        public static T GetRequiredService<T>() where T : class => Host.Services.GetRequiredService<T>();

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SimConnectAdapter>();
            services.AddSingleton<DataSender>();
            services.AddSingleton<IpDetectionService>();
            services.AddHostedService(provider => provider.GetRequiredService<IpDetectionService>());
        }
    }
}
