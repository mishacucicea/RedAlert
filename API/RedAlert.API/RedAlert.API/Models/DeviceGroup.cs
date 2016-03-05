using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RedAlert.API.Models
{
    public class DeviceGroup
    {
        public string Name { get; set; }
        public string Password { get; set; }
        public List<string> SerialNumbers { get; set; }
    }
}