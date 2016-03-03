using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace RedAlert.API.BL
{
    public class DeviceIdentity
    {
        RegistryManager registryManager;
        string connectionString;
        string _deviceId;
       

        public DeviceIdentity(string deviceId)
        {
            _deviceId = deviceId;
            AddDeviceAsync().Wait();
        }
        private async Task AddDeviceAsync()
        {
            connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
            registryManager = RegistryManager.CreateFromConnectionString(connectionString);
            Device device = null;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(_deviceId));
            }
            catch (DeviceAlreadyExistsException)
            {
                throw new Exception("Device Alredy Exist");
            }   
        }
        public async Task<string> GetDeviceAsync(string deviceId)
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


    }
}