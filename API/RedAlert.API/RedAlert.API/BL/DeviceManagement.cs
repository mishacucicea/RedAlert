using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using RedAlert.API.DAL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace RedAlert.API.BL
{
    public class DeviceManagement
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        /// <summary>
        /// The registry manager
        /// </summary>

        private string iotHubUri = ConfigurationManager.AppSettings["IotHubUri"];




        /// <summary>
        /// Adds the device asynchronous.
        /// </summary>
        /// <param name="serialNumber">The Serial Number for the device.</param>
        /// <returns>Device Key</returns>
        /// <exception cref="System.Exception">Device Alredy Exist</exception>
        public async Task<DeviceModel> AddDeviceAsync(string serialNumber)
        {
            RandomProvider rand = new RandomProvider();

            using (RedAlertContext context = new RedAlertContext())
            {
                SerialNumber sn = context.SerialNumbers.SingleOrDefault(x => x.Code == serialNumber);

                if (sn == null)
                {
                    throw new ArgumentException("Invalid serial number.");
                }

                //create keys - will be used to updated if already registered.
                Models.Device device = context.Devices.SingleOrDefault(x => x.SerialNumber == serialNumber) ?? context.Devices.Create();
                
                do
                {
                    device.DeviceKey = rand.GetAlphaNumeric(8);
                    device.SenderKey = rand.GetAlphaNumeric(8);
                    
                    //check for collisions!
                }
                while (context.Devices.Any(x => x.DeviceKey == device.DeviceKey || x.SenderKey == device.SenderKey));

                //if it's the first registration
                if (device.SerialNumber == null)
                {
                    //copy the serial number from the database!
                    device.SerialNumber = sn.Code;

                    //save the device because we need the id!
                    await context.SaveChangesAsync();

                    //this is the name that will be used by the Azure IoT Hub
                    device.HubDeviceId = "device" + device.DeviceId;

                    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                    Microsoft.Azure.Devices.Device IoTdevice;
                    try
                    {
                        IoTdevice = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(device.HubDeviceId));
                    }
                    catch (DeviceAlreadyExistsException)
                    {
                        IoTdevice = await registryManager.GetDeviceAsync(device.HubDeviceId);
                    }

                    //and don't forget to update the SerialNumber as used
                    sn.Activated = true;
                }

                await context.SaveChangesAsync();

                return new DeviceModel
                {
                    SerialNumber = device.SerialNumber,
                    DeviceKey = device.DeviceKey,
                    SenderKey = device.SenderKey
                };
            }
        }

        /// <summary>
        /// Gets the device asynchronous.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Does not Exist</exception>
        public async Task<string> GetDeviceAsync(string deviceId)
        {
            throw new NotImplementedException();

            //try
            //{
            //    var device = await registryManager.GetDeviceAsync(deviceId);
            //    return device.Authentication.SymmetricKey.PrimaryKey;
            //}
            //catch (DeviceNotFoundException)
            //{
            //    throw new Exception("Device Does not Exist");
            //}


        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.Exception">Device Does not Exist</exception>
        public async Task<IEnumerable<Models.Device>> GetDevices()
        {
            throw new NotImplementedException();

            //try
            //{
            //    var devices = await registryManager.GetDevicesAsync(100);
            //    return devices;
            //}

            //catch (DeviceNotFoundException)
            //{
            //    throw new Exception("Device Does not Exist");
            //}
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="deviceId">The serial number.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async Task SendCloudToDeviceMessageAsync(string deviceId, byte[] message)
        {
            ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
            var messageToBytes = new Message(message);
            await serviceClient.SendAsync(deviceId, messageToBytes);
        }

    }
}
