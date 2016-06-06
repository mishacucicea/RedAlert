﻿using System.ComponentModel.DataAnnotations.Schema;

namespace RedAlert.API.Models
{
    /// <summary>
    /// Represents the device.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the serial number.
        /// </summary>
        /// <value>
        /// The serial number.
        /// </value>
        [Index(IsUnique = true)]
        public string SerialNumber { get; set; }

        /// <summary>
        /// Gets or sets the DeviceId used by the Azure IoT Hub.
        /// </summary>
        /// <value>
        /// The device Id.
        /// </value>
        public string HubDeviceId { get; set; }

        /// <summary>
        /// Gets or sets the DeviceKey used by the device to get Azure IoT Hub authentication data.
        /// </summary>
        [Index(IsUnique = true)]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the SenderKey used by the sender to send messages to this device.
        /// </summary>
        [Index(IsUnique = true)]
        public string SenderKey { get; set; }
    }
}