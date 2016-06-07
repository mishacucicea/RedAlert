using RedAlert.API.Models;
using System.Data.Entity;

namespace RedAlert.API.DAL
{
    /// <summary>
    /// The database context.
    /// </summary>
    /// <seealso cref="System.Data.Entity.DbContext" />
    public class RedAlertContext : DbContext
    {
        

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public DbSet<Device> Devices { get; set; }

        /// <summary>
        /// Gets or sets the device groups.
        /// </summary>
        /// <value>
        /// The device groups.
        /// </value>
        public DbSet<DeviceGroup> DeviceGroups { get; set; }

        public DbSet<SerialNumber> SerialNumbers { get; set; }
    }
}