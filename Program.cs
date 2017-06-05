using System;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;
using NLog.Extensions.Logging;

namespace CoreIotHubClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();

            var app = serviceProvider.GetService<IotHubClient>();

            Task.Run(() => app.Run()).Wait();
        }
        private static IServiceProvider BuildServiceProvider()
        {
            var serviceCollection = GetServiceColletion();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            serviceProvider.GetService<ILoggerFactory>()
                .AddNLog()
                .ConfigureNLog("nlog.config");
                
            return serviceProvider;
        }
        public static IServiceCollection GetServiceColletion()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();

            var configuration = GetConfiguration();
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            serviceCollection.AddOptions();
            serviceCollection.Configure<IotHubOptions>(configuration.GetSection("IotHub"));

            serviceCollection.AddTransient<IotHubClient>();
            return serviceCollection;
        }

        private static IConfigurationRoot GetConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"appsettings.json", optional: true)
                .Build();
        }
    }
}
