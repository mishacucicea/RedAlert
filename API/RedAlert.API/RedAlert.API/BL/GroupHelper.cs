using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RedAlert.API.Controllers;
using RedAlert.API.Models;
using System.Threading.Tasks;

namespace RedAlert.API.BL
{
    public class GroupHelper
    {
        public static async Task SendGroupMessage(List<Device> devices,string color)
        {
            MessageController controller = new MessageController();
            foreach (var device in devices)
            {
                await controller.Send(device.SenderKey, color);
            }
            
        }
        public static async Task<List<Device>> GetDevices()
        {
            DeviceManagement dm = new DeviceManagement();
            var devices = await dm.GetDevices();
            return devices;
        }

    }
}