using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace CoreIotHubClient
{
    public class IotHubClient
    {
        private readonly ILogger _logger;

        private readonly IotHubOptions _settings;

        private bool _stop = false;

        public IotHubClient(ILogger<IotHubClient> logger, IOptions<IotHubOptions> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task Run()
        {
            try
            {
                _stop = false;

                while (true)
                {
                    using (var deviceClient = DeviceClient.CreateFromConnectionString(_settings.Connectionstring))
                    {
                        await deviceClient.OpenAsync();
                        _logger.LogDebug("Connected to IoTHub!");

                        var cancellationTokenSource = new CancellationTokenSource();

                        await Task.WhenAny(new List<Task>{
                            ReceiveMessages(deviceClient,cancellationTokenSource.Token),
                            ReportAliveOrStop(cancellationTokenSource.Token)
                        });

                        cancellationTokenSource.Cancel();

                        await deviceClient.CloseAsync();
                        _logger.LogDebug("Disconnected to IoTHub!");

                        if (_stop) break;
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogCritical("Unhandeld exception in DeviceClient", ex);
                throw;
            }
        }

        public void Stop()
        {
            _stop = true;
        }

        private async Task ReceiveMessages(DeviceClient deviceClient, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (var message = await deviceClient.ReceiveAsync())
                    {
                        if (message != null)
                        {
                            OnMessageReceived(message);
                            await deviceClient.CompleteAsync(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex.Message, ex);
                }
            }
        }

        private async Task ReportAliveOrStop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (_stop)
                {
                    _logger.LogDebug("Actively stopped");
                    return;
                }

                Console.Write(".");
                await Task.Delay(TimeSpan.FromSeconds(1), token);
            }
            Console.WriteLine("ReportAliveOrStop");
        }

        private void OnMessageReceived(Message message)
        {
            var messageString = Encoding.UTF8.GetString(message.GetBytes());

            _logger.LogInformation("{0} > Receiving message: {1}", DateTime.Now, messageString);
        }
    }
}
