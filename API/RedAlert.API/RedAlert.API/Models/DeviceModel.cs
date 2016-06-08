using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedAlert.API.Models
{
    public class DeviceModel
    {
        public string DeviceKey { get; set; }

        public string SenderKey { get; set; }

        public string SerialNumber { get; set; }
    }
}