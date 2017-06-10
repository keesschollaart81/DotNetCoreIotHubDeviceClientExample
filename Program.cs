using System;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.IO;
using NLog.Extensions.Logging;
using System.Threading;
using System.Collections.Generic;

namespace CoreIotHubClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceCollection = GetServiceColletion();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();
            var iotHubClient = serviceProvider.GetService<IotHubClient>();
            var runTask = iotHubClient.Run();

            Console.WriteLine("Press any key to close");
            Console.ReadKey();

            iotHubClient.Stop();
            logger.LogInformation("Stopping...");

            runTask.Wait(Convert.ToInt16(TimeSpan.FromSeconds(3).TotalMilliseconds));
            logger.LogInformation("Stopped");
        }

        public static IServiceCollection GetServiceColletion()
        {
            var serviceCollection = new ServiceCollection();
            var loggerFactory = new LoggerFactory();
            loggerFactory
                .AddNLog()
                .ConfigureNLog("nlog.config");
            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

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
