using System.IO;
using System.Windows;
using BeatManager_WPF_.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

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

            services.AddSingleton<SplashScreen>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var splashWindow = _serviceProvider.GetService<SplashScreen>();
            splashWindow.Show();
        }
    }
}
