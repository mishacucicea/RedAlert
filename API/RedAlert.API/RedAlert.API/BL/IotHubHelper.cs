using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace RedAlert.API.BL
{
    public static class IotHubHelper
    {
        /// <summary>
        /// The connection string
        /// </summary>
        static internal string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        /// <summary>
        /// The registry manager
        /// </summary>
        static internal RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        /// <summary>
        /// The iot hub URI
        /// </summary>
        static internal string iotHubUri = ConfigurationManager.AppSettings["IotHubUri"];

        static internal ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);


        /// <summary>
        /// Adds the device asynchronous.
        /// </summary>
        /// <param name="deviceId">The identifier.</param>
        /// <returns>Device Key</returns>
        /// <exception cref="System.Exception">Device Alredy Exist</exception>
        public async static Task<string> AddDeviceAsync(string deviceId)
        {

            Device device = null;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceId));
                return device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceAlreadyExistsException)
            {
                var result = await GetDeviceAsync(deviceId);
                return result;
            }        
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
        /// <param name="deviceId">The serial number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async static Task SendCloudToDeviceMessageAsync(string deviceId, string message)
            { 
                var messageToBytes = new Microsoft.Azure.Devices.Message(Encoding.ASCII.GetBytes(message));
                await serviceClient.SendAsync(deviceId, messageToBytes);
            }

        }
    }
