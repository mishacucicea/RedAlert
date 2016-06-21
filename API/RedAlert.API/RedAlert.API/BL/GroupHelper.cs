using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RedAlert.API.Controllers;
using RedAlert.API.Models;


namespace RedAlert.API.BL
{
    public class GroupHelper
    {
        public static async void SendGroupMessage(List<Device> devices,string color)
        {
            MessageController controller = new MessageController();
            foreach (var device in devices)
            {
                await controller.Send(device.SenderKey, color);
            }
            
        }
    }
}