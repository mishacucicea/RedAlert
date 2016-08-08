﻿using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using RedAlert.API.DAL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Data.Entity;

namespace RedAlert.API.BL
{
    public class DeviceManagement
    {
        /// <summary>
        /// The connection string
        /// </summary>
        private string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;

        public async Task<String> GetDeviceSASKey(string deviceId)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            Microsoft.Azure.Devices.Device IoTdevice = await registryManager.GetDeviceAsync(deviceId);
            return IoTdevice.Authentication.SymmetricKey.PrimaryKey;
        }

        /// <summary>
        /// Adds the device asynchronous.
        /// </summary>
        /// <param name="serialNumber">The Serial Number for the device.</param>
        /// <returns>Device Key</returns>
        /// <exception cref="System.Exception">Device Alredy Exist</exception>
        public async Task<DeviceModel> AddDeviceAsync(string serialNumber)
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                SerialNumber sn = await context.SerialNumbers.SingleOrDefaultAsync(x => x.Code == serialNumber);

                if (sn == null)
                {
                    throw new ArgumentException("Invalid serial number.");
                }

                //create keys - will be used to updated if already registered.
                Models.Device device = await context.Devices.SingleOrDefaultAsync(x => x.SerialNumber == serialNumber);

                if (device != null)
                {
                    throw new ArgumentOutOfRangeException("Serial number already registered.");
                }

                device = context.Devices.Create();

                await PopulateNewKeys(context, device);

                //if it's the first registration
                if (device.SerialNumber == null)
                {
                    //the entity has been created so we need to add it to the collection
                    context.Devices.Add(device);

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
        /// Retrieves a cloud device by its hub id.
        /// </summary>
        /// <param name="hubDeviceId">The hub device id.</param>
        /// <returns>The cloud device details.</returns>
        public async Task<Microsoft.Azure.Devices.Device> GetCloudDeviceAsync(string hubDeviceId)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            return await registryManager.GetDeviceAsync(hubDeviceId);
        }

        /// <summary>
        /// Resets the keys for a given device.
        /// </summary>
        /// <param name="id">The device id.</param>
        /// <returns>The device.</returns>
        public async Task<Models.Device> ResetDeviceKeysAsync(int id)
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                Models.Device device = await context.Devices.SingleOrDefaultAsync(x => x.DeviceId == id);

                await PopulateNewKeys(context, device);

                await context.SaveChangesAsync();

                return device;
            }
        }

        /// <summary>
        /// Populates the given device with new device and sender keys.
        /// </summary>
        /// <param name="context">The DB context.</param>
        /// <param name="device">The device.</param>
        private async Task PopulateNewKeys(RedAlertContext context, Models.Device device)
        {
            RandomProvider rand = new RandomProvider();

            do
            {
                device.DeviceKey = rand.GetAlphaNumeric(8);
                device.SenderKey = rand.GetAlphaNumeric(8);

                //check for collisions!
            }
            while (await context.Devices.AnyAsync(x => x.DeviceKey == device.DeviceKey || x.SenderKey == device.SenderKey));
        }

        /// <summary>
        /// Gets the device asynchronous.
        /// </summary>
        /// <param name="deviceId">The device identifier.</param>
        /// <returns>The device.</returns>
        public async Task<Models.Device> GetDeviceAsync(int deviceId)
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);

                return device;
            }
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>The list of devices.</returns>
        public async Task<List<Models.Device>> GetDevices()
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                return await context.Devices.ToListAsync();
            }
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="deviceKey">The generated DeviceKey.</param>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public async Task SendCloudToDeviceMessageAsync(string deviceKey, byte[] message)
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                Models.Device device = await context.Devices.SingleOrDefaultAsync(x => x.DeviceKey == deviceKey);

                if (device == null)
                {
                    throw new ArgumentException("No such DeviceKey.");
                }

                ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                var messageToBytes = new Message(message);
                await serviceClient.SendAsync(device.HubDeviceId, messageToBytes);
            }
        }

        /// <summary>
        /// Removes a device from both the DB and the IoT Hub entry.
        /// </summary>
        /// <param name="deviceId">The device id.</param>
        public async Task RemoveDeviceAsync(int deviceId)
        {
            using (RedAlertContext context = new RedAlertContext())
            {
                var device = await context.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId);
                context.Devices.Remove(device);

                await context.SaveChangesAsync();

                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);

                await registryManager.RemoveDeviceAsync(device.HubDeviceId);
            }
        }
    }
}
