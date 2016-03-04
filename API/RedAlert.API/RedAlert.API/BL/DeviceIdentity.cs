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
     public static class DeviceIdentity
    {
        static string connectionString = ConfigurationManager.ConnectionStrings["IotHubConnectionString"].ConnectionString;
        static RegistryManager registryManager =RegistryManager.CreateFromConnectionString(connectionString);
       

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


    }
}