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
using System.Data.Entity;
using NLog;

namespace RedAlert.API.BL
{
    public class DeviceManagement
    {
        public ILogger Logger { get; set; }
        
        public DeviceManagement()
        {
            Logger = LogManager.GetLogger("RedAlert");
        }

        /// <summary>
        /// Primary IoT Hub connection string
        /// </summary>
        private string connectionString = ConfigurationManager.ConnectionStrings["PrimaryIotHub"].ConnectionString;

        /// <summary>
        /// Secondary IoT Hub connection string
        /// </summary>
        private string connectionString2 = ConfigurationManager.ConnectionStrings["SecondaryIotHub"].ConnectionString;

        [Obsolete]
        public async Task<string> GetDeviceSASKey(string deviceId)
        {
            RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            Microsoft.Azure.Devices.Device IoTdevice = await registryManager.GetDeviceAsync(deviceId);
            return IoTdevice.Authentication.SymmetricKey.PrimaryKey;
        }

        /// <summary>
        /// Retrieves the Primary and Secondary IoT Hub SAS Keys.
        /// </summary>
        public async Task<Tuple<string, string>> GetDeviceSASKeys(string deviceId)
        {
            string key1 = string.Empty;
            string key2 = string.Empty;
            try
            {
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                Microsoft.Azure.Devices.Device IoTdevice = await registryManager.GetDeviceAsync(deviceId);

                //register the device if it's missing on this IoT Hub
                if (IoTdevice == null)
                {
                    IoTdevice = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(deviceId));
                }

                key1 = IoTdevice.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not retrieve the Primary IoT Hub SAS Key.");
            }

            try
            {
                RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString2);
                Microsoft.Azure.Devices.Device IoTdevice = await registryManager.GetDeviceAsync(deviceId);

                //register the device if it's missing on this IoT Hub
                if (IoTdevice == null)
                {
                    IoTdevice = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(deviceId));
                }

                key2 = IoTdevice.Authentication.SymmetricKey.PrimaryKey;
            }
            catch (Exception e)
            {
                Logger.Error(e, "Could not retrieve the Secondary IoT Hub SAS Key.");
            }

            //if both IoT Hubs are down we can't continue with the execution
            if (string.IsNullOrEmpty(key1) && string.IsNullOrEmpty(key2))
            {
                throw new ApplicationException("Could not retrieve SAS Keys for none of the IoT Hubs. Can not continue execution.");
            }

            return new Tuple<string, string>(key1, key2);
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

                    
                    Microsoft.Azure.Devices.Device IoTdevice;
                    bool primaryHasError = false;
                    bool secondaryHasError = false;

                    //tyr add the device in the Primary IoT Hub
                    try
                    {
                        RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                        IoTdevice = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(device.HubDeviceId));
                    }
                    catch (DeviceAlreadyExistsException)
                    {
                        //IoTdevice = await registryManager.GetDeviceAsync(device.HubDeviceId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Could not add the Device {0} to the Primary IoT Hub registry", device.HubDeviceId);
                        primaryHasError = true;
                    }

                    //try add the device in the Secondary IoT Hub
                    try
                    {
                        RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString2);
                        IoTdevice = await registryManager.AddDeviceAsync(new Microsoft.Azure.Devices.Device(device.HubDeviceId));
                    }
                    catch (DeviceAlreadyExistsException)
                    {
                        //IoTdevice = await registryManager.GetDeviceAsync(device.HubDeviceId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e, "Could not add the Device {0} to the Secondary IoT Hub registry", device.HubDeviceId);
                        secondaryHasError = true;
                    }

                    if (primaryHasError && secondaryHasError)
                    {
                        throw new ApplicationException("Could not add the device to any of the IoT Hubs.");
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
                return await context.Devices.Include(x =>x.Group).ToListAsync();
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

                bool primaryHasError = false;
                bool secondaryHasError = false;
                try
                {
                    ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString);
                    var messageBytes = new Message(message);
                    await serviceClient.SendAsync(device.HubDeviceId, messageBytes);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not send message to the device {0} on the Primary IoT Hub", device.HubDeviceId);
                    primaryHasError = true;
                }

                try
                {
                    ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(connectionString2);
                    var messageBytes = new Message(message);
                    await serviceClient.SendAsync(device.HubDeviceId, messageBytes);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not send message to the device {0} on the Secondary IoT Hub", device.HubDeviceId);
                    secondaryHasError = true;
                }

                if (primaryHasError && secondaryHasError)
                {
                    throw new ApplicationException("Could not send message to device on any IoT Hub");
                }
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
                
                try
                {
                    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
                    await registryManager.RemoveDeviceAsync(device.HubDeviceId);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not remote device {0} from the Primary IoT Hub", deviceId);
                }

                try
                {
                    RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString2);
                    await registryManager.RemoveDeviceAsync(device.HubDeviceId);
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Could not remove device {0} from the Primary IoT Hub", deviceId);
                }

                //for removing devices we will not fail the whole process even if the device could not be 
                //removed from any of the IoT Hubs
            }
        }
    }
}
