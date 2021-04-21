using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using BeatManager_WPF_.Interfaces;
using BeatManager_WPF_.Models;
using BeatManager_WPF_.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Sentry;

namespace BeatManager_WPF_
{
    public partial class App : Application
    {
        private readonly ServiceProvider _serviceProvider;

        public App()
        {
            var serviceCollection = new ServiceCollection();

            ConfigureServices(serviceCollection);
            
            _serviceProvider = serviceCollection.BuildServiceProvider();

            // DispatcherUnhandledException += App_DispatcherUnhandledException;
            // SentrySdk.Init(new SentryOptions
            // {
            //     Dsn = "https://cf63505d892741229421f4b182c8a9b5@o575010.ingest.sentry.io/5726733",
            //     Environment = "Development"
            // });
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            SentrySdk.CaptureException(e.Exception);

            e.Handled = true;
        }

        private Config LoadConfig()
        {
            var config = new Config();

            if (!Directory.Exists("./data"))
                Directory.CreateDirectory("./data");

            if (File.Exists("./data/config.json"))
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("./data/config.json"));
            else
                File.WriteAllText("./data/config.json", JsonConvert.SerializeObject(config, Formatting.Indented));

            return config;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var config = LoadConfig();
            services.AddSingleton(config);

            services.AddHttpClient<IBeatSaverAPI, BeatSaverAPI>(options =>
            {
                options.BaseAddress = new Uri("https://beatsaver.com/api/");
                options.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0");
                options.DefaultRequestHeaders.Add("Accept-Language","en-GB,en-US;q=0.7,en;q=0.3");
                options.DefaultRequestHeaders.Add("Accept", "application/json,text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            });

            services.AddSingleton<SplashScreen>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var splashWindow = _serviceProvider.GetService<SplashScreen>();
            splashWindow.Show();
        }
    }
}
