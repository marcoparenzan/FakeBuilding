using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Shared;
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
        private static string doorState = "closed";
        private static string doorName = "";

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

            doorName = deviceId;

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

            var twin = await client.GetTwinAsync();
            if (!string.IsNullOrWhiteSpace((string)twin.Properties.Desired["doorName"]))
            {
                doorName = (string)twin.Properties.Desired["doorName"];
                Console.WriteLine($"DoorName is now {doorName}");
            }

            await client.SetDesiredPropertyUpdateCallbackAsync(async (tc, oc) =>
            {
                doorName = (string)tc["doorName"];
                Console.WriteLine($"DoorName is now {doorName}");
            }, null);

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
                var textParts = text.Split();
                switch (textParts[0].ToLower())
                {
                    case "opendoor":
                        await OpenDoor(client);
                        break;
                    case "closedoor":
                        await CloseDoor(client);
                        break;
                    default:
                        Console.WriteLine("Syntax error");
                        break;
                }


                await client.CompleteAsync(message);
            }
        }

        private static async Task OpenDoor(DeviceClient client)
        {
            doorState = "open";
            Console.WriteLine("Door is now open");
            var coll = new TwinCollection();
            coll["doorState"] = doorState;
            await client.UpdateReportedPropertiesAsync(coll);
        }

        private static async Task CloseDoor(DeviceClient client)
        {
            doorState = "close";
            Console.WriteLine("Door is now closed");
            var coll = new TwinCollection();
            coll["doorState"] = doorState;
            await client.UpdateReportedPropertiesAsync(coll);
        }
    }
}
