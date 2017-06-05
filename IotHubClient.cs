using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace CoreIotHubClient
{
    public class IotHubClient
    {
        private readonly ILogger _logger;
        private readonly IotHubOptions _settings;

        public IotHubClient(ILogger<IotHubClient> logger, IOptions<IotHubOptions> settings)
        {
            _logger = logger;
            _settings = settings.Value;
        }

        public async Task Run()
        {
            try
            {
                var deviceClient = DeviceClient.CreateFromConnectionString(_settings.Connectionstring);
                _logger.LogDebug("Connected to IoTHub!");
                while (true)
                {
                    var message = await deviceClient.ReceiveAsync();
                    var messageString = Encoding.UTF8.GetString(message.GetBytes());
                    _logger.LogInformation("{0} > Receiving message: {1}", DateTime.Now, messageString);
                    await deviceClient.CompleteAsync(message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
            }
        }
    }
}
