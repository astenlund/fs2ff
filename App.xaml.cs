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
        private readonly IHost _host;

        public App()
        {
            Instance = this;
            _host = new HostBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        public static App? Instance { get; private set; }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(3));
            _host.Dispose();
            base.OnExit(e);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            base.OnStartup(e);
        }

        public static Version AssemblyVersion => Assembly.GetEntryAssembly()!.GetName().Version!;

        public static string InformationalVersion => "v" + (Assembly
            .GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "0.0.0");

        public IServiceProvider? ServiceProvider => _host.Services;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<SimConnectAdapter>();
            services.AddSingleton<DataSender>();
            services.AddSingleton<IpDetectionService>();
            services.AddHostedService(provider => provider.GetRequiredService<IpDetectionService>());
        }
    }
}
