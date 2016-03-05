using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedAlert.API.BL
{
    public static class IotHubHelper
    {
        /// <summary>
        /// The connection string
        /// </summary>
        static string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        /// <summary>
        /// The registry manager
        /// </summary>
        static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        /// <summary>
        /// The iot hub URI
        /// </summary>
        static string iotHubUri = ConfigurationManager.AppSettings["IotHubUri"];


        /// <summary>
        /// Adds the device asynchronous.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Alredy Exist</exception>
        public async static Task<string> AddDeviceAsync(string id)
        {

            Device device = null;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(id));
            }
            catch (DeviceAlreadyExistsException)
            {
                throw new Exception("Device Alredy Exist");
            }

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        /// <summary>
        /// Gets the device asynchronous.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Does not Exist</exception>
        public async static Task<string> GetDeviceAsync(string deviceId)
        {
            var device = new Device();
            try
            {
                device = await registryManager.GetDeviceAsync(deviceId);
            }

            catch (DeviceNotFoundException)
            {
                throw new Exception("Device Does not Exist");
            }

            return device.Authentication.SymmetricKey.PrimaryKey;
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Does not Exist</exception>
        public async static Task<IEnumerable<Device>> GetDevices()
        {
            IEnumerable<Device> devices;

            try
            {
                devices = await registryManager.GetDevicesAsync(100);
            }

            catch (DeviceNotFoundException)
            {
                throw new Exception("Device Does not Exist");
            }

            return devices;
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async static Task SendMessage(string serialNumber,string message)
        {
             DeviceClient deviceClient;
             string deviceKey= await GetDeviceAsync(serialNumber);
             deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(serialNumber, deviceKey));

             var messageToBytes = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(message));

             await deviceClient.SendEventAsync(messageToBytes);
        }

        /// <summary>
        /// Receives the message.
        /// </summary>
        /// <param name="serialNumber">The serial number.</param>
        /// <returns></returns>
        public async static Task<string> ReceiveMessage(string serialNumber)
        {
            DeviceClient deviceClient;
            string deviceKey = await GetDeviceAsync(serialNumber);
            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(serialNumber, deviceKey));

            //var messageObj = await deviceClient.ReceiveAsync(new TimeSpan(0,0,1));

            string message = "";

            //if (messageObj != null)
            //{
            //    using (var sr = new StreamReader(messageObj.BodyStream))
            //    {
            //        message = sr.ReadToEnd();
            //    }
            //}

            return message;
        }
    }
}