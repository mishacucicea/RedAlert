using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace RedAlert.API.BL
{
    public static class IotHubHelper
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        static RegistryManager registryManager = RegistryManager.CreateFromConnectionString(connectionString);
        static string iotHubUri = ConfigurationManager.AppSettings["IotHubUri"];


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
        public async static Task SendMessage(string serialNumber,string message)
        {
             DeviceClient deviceClient;
             string deviceKey= await GetDeviceAsync(serialNumber);
             deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(serialNumber, deviceKey));

             var messageToBytes = new Microsoft.Azure.Devices.Client.Message(Encoding.ASCII.GetBytes(message));

             await deviceClient.SendEventAsync(messageToBytes);
        }
    }
}