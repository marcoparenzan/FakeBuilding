using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace FakeTornello
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var deviceId = configuration["deviceId"];
            var authenticationMethod =
                new DeviceAuthenticationWithRegistrySymmetricKey(
                    deviceId,
                    configuration["deviceKey"]
                )
            ;

            var transportType = TransportType.Mqtt;
            if (!string.IsNullOrWhiteSpace(configuration["transportType"]))
            {
                transportType = (TransportType)
                    Enum.Parse(typeof(TransportType),
                    configuration["transportType"], true);

            }

            var client = DeviceClient.Create(
                configuration["hostName"],
                authenticationMethod,
                transportType
            );

            Console.WriteLine($"Welcome to THE COMPANY");
            Console.WriteLine($"You are in front of the door {configuration["deviceId"]}");
            //Console.ReadLine();

            while (true)
            {
                var message = await client.ReceiveAsync();
                if (message == null) continue;

                var bytes = message.GetBytes();
                if (bytes == null) continue;

                var text = Encoding.UTF8.GetString(bytes);

                Console.WriteLine($"Messaggio ricevuto: {text}");

                await client.CompleteAsync(message);
            }
        }
    }
}
