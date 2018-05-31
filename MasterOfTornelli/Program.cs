using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MasterOfTornelli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var serviceClient = 
                ServiceClient.CreateFromConnectionString(
                    configuration["IoTHubConnectionString"]);

            Console.WriteLine("Hello Master of the Tornellis!");
            Console.WriteLine("I'm Cortana!");
            while (true)
            {
                Console.Write("What do you want to do now? ");
                var commandText = Console.ReadLine();
                var commandParts = commandText.ToLower().Split();

                switch (commandParts[0])
                {
                    case "opendoor":
                        await OpenDoor(serviceClient, commandParts);
                        break;
                    case "closedoor":
                        await CloseDoor(serviceClient, commandParts);
                        break;
                    default:
                        Console.WriteLine("Scrivi meglio, la prossima volta!");
                        break;
                }
            }
        }

        private static async Task OpenDoor(ServiceClient serviceClient, string[] commandParts)
        {
            var bytes = Encoding.UTF8.GetBytes("opendoor");
            var message = new Message(bytes);
            try
            {
                await serviceClient.SendAsync(commandParts[1], message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ehila furbastro! Serve documentazione?");
                Console.WriteLine("Scrivi: opendoor <idtornello>");
            }
        }

        private static async Task CloseDoor(ServiceClient serviceClient, string[] commandParts)
        {
            var bytes = Encoding.UTF8.GetBytes("closedoor");
            var message = new Message(bytes);
            try
            {
                await serviceClient.SendAsync(commandParts[1], message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ehila furbastro! Serve documentazione?");
                Console.WriteLine("Scrivi: closedoor <idtornello>");
            }
        }
    }
}
