using System.Collections.Generic;

namespace RedAlert.API.Models
{
    /// <summary>
    /// Represents a group of devices.
    /// </summary>
    public class DeviceGroup
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the serial numbers.
        /// </summary>
        /// <value>
        /// The serial numbers.
        /// </value>
        public List<string> SerialNumbers { get; set; }
    }
}