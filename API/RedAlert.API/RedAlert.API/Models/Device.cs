﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DeviceId { get; set; }

        /// <summary>
        /// Gets or sets the serial number. This is the physical key attached to the device.
        /// </summary>
        /// <value>
        /// The serial number.
        /// </value>
        [Index(IsUnique = true)]
        [MaxLength(40)]
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
        [MaxLength(20)]
        public string DeviceKey { get; set; }

        /// <summary>
        /// Gets or sets the SenderKey used by the sender to send messages to this device.
        /// </summary>
        [Index(IsUnique = true)]
        [MaxLength(20)]
        public string SenderKey { get; set; }

        /// <summary>
        /// Gets or sets the device name.
        /// </summary>
        [MaxLength(40)]
        public string Name { get; set; }

        public byte[] LastMessage { get; set; }

        public virtual DeviceGroup Group { get; set; }

        /// <summary>
        /// The device api url. Not present in database.
        /// </summary>
        [NotMapped]
        public string ApiUrl { get; set; }

        /// <summary>
        /// The last cloud device activity time.
        /// </summary>
        [NotMapped]
        public DateTime LastActivityTime { get; set; }

        /// <summary>
        /// Determines if the device was active during the last X seconds.
        /// </summary>
        [NotMapped]
        public bool IsActive {
            get { return (DateTime.UtcNow - LastActivityTime.ToUniversalTime()) < TimeSpan.FromSeconds(60); }
        }

        public string UserID { get; set; }
    }   
}