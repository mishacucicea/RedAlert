using System.Collections.Generic;

namespace RedAlert.API.Models
{
    /// <summary>
    /// Represents a group of devices.
    /// </summary>
    public class DeviceGroup
    {
        public int DeviceGroupId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the serial numbers.
        /// </summary>
        /// <value>
        /// The serial numbers.
        /// </value>
        public virtual List<Device> Devices { get; set; }
    }
}