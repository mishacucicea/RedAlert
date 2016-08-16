using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;

namespace RedAlert.Client
{
    class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            ReceiveMessage();

            Console.ReadLine();
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        static void ReceiveMessage()
        {
            var client = EventHubClient.CreateFromConnectionString(
                ConfigurationManager.ConnectionStrings["PrimaryIotHubConnectionString"].ConnectionString,
                "messages/events");

            var d2cPartitions = client.GetRuntimeInformation().PartitionIds;

            foreach (string partition in d2cPartitions)
            {
                ReceiveMessagesFromDeviceAsync(client, partition);
            }
        }

        /// <summary>
        /// Receives the messages from device asynchronous.
        /// </summary>
        /// <param name="client">The client.</param>
        /// <param name="partition">The partition.</param>
        /// <returns></returns>
        private static async Task ReceiveMessagesFromDeviceAsync(EventHubClient client, string partition)
        {
            try
            {
                var eventHubReceiver = client.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.UtcNow);
                while (true)
                {
                    EventData eventData = await eventHubReceiver.ReceiveAsync();
                    if (eventData == null) continue;

                    string data = Encoding.UTF8.GetString(eventData.GetBytes());

                    WriteToSerial(data);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
           
        }

        /// <summary>
        /// Writes to serial.
        /// </summary>
        /// <param name="message">The message.</param>
        static void WriteToSerial(string message)
        {
            using (var mySerialPort = new SerialPort("COM3"))
            {
                mySerialPort.Open();

                
                    mySerialPort.Write(message);
                
            }
        }
    }
}