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
        internal static string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        /// <summary>
        /// The registry manager
        /// </summary>
        internal static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        /// <summary>
        /// The iot hub URI
        /// </summary>
        internal static string iotHubUri = ConfigurationManager.AppSettings["IotHubUri"];

        internal static ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);

        
        /// <summary>
        /// Adds the device asynchronous.
        /// </summary>
        /// <param name="deviceId">The identifier.</param>
        /// <returns>Device Key</returns>
        /// <exception cref="System.Exception">Device Alredy Exist</exception>
        public static async Task<string> AddDeviceAsync(string deviceId)
        {
            try
            {
                var device = await registryManager.AddDeviceAsync(new Device(deviceId));
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
        public static async Task<string> GetDeviceAsync(string deviceId)
        {         
            try
            {
             var  device = await registryManager.GetDeviceAsync(deviceId);
             return device.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (DeviceNotFoundException)
            {
                throw new Exception("Device Does not Exist");
            }

         
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Does not Exist</exception>
        public static async Task<IEnumerable<Device>> GetDevices()
        {
            try
            {
                var  devices = await registryManager.GetDevicesAsync(100);
                return devices;
            }

            catch (DeviceNotFoundException)
            {
                throw new Exception("Device Does not Exist");
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="deviceId">The serial number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public static async Task SendCloudToDeviceMessageAsync(string deviceId, string message)
            { 
                var messageToBytes = new Message(Encoding.ASCII.GetBytes(message));
                await serviceClient.SendAsync(deviceId, messageToBytes);
            }
        }
    }
