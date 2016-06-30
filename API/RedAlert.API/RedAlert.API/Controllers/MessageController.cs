﻿using RedAlert.API.BL;
using RedAlert.API.DAL;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Ajax.Utilities;
using NLog;

namespace RedAlert.API.Controllers
{
    public class MessageController : ApiController
    {
        /// <summary>
        /// Gets or sets the logger.
        /// </summary>
        /// <value>
        /// The logger.
        /// </value>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        public MessageController()
        {
            Logger = LogManager.GetLogger("RedAlert");
        }


        /// <summary>
        /// Client facing API.
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="color">RGB (six character) or named color. Case insensitive.</param>
        /// <param name="pattern">Specify the kind of pattern to use. Default is continunous.</param>
        /// <param name="timeout">How much time to keep the given color on (in seconds).</param>
        [HttpGet]
        public async Task<IHttpActionResult> Send(string senderKey, string color, string pattern = null, uint? timeout = null)
        {
            //we'll be building the message to be sent to the IoT Hub
            MessageBuilder mb = new MessageBuilder();

            if (!mb.TrySetColor(color))
            {
                return BadRequest("Unknown color.");
            }

            if (!string.IsNullOrEmpty(pattern) && !mb.TrySetPattern(pattern))
            {
                return BadRequest("Unknown pattern.");
            }

            if (timeout.HasValue)
            {
                mb.SetTimeout(timeout.Value);
            }

            using (RedAlertContext context = new RedAlertContext())
            {
                byte[] message = mb.GetBytes();

                Models.Device device = await context.Devices.SingleOrDefaultAsync(x => x.SenderKey == senderKey);

                if (device == null)
                {
                    return BadRequest();
                }

                device.LastMessage = message;

                DeviceManagement dm = new DeviceManagement();

                Logger.Debug("Sending the message: " + ByteToHex(message));

                //TODO: figure out how to await in parallel
                await dm.SendCloudToDeviceMessageAsync(device.DeviceKey, message);
                await context.SaveChangesAsync();
            }
            return Ok();
        }

        [HttpGet]
        public async Task<IHttpActionResult> SendToGroup(string groupName, string message)
        {



            throw new NotImplementedException();


        }

        private static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0) throw new FormatException("Not a hex string.");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private string ByteToHex(byte[] array)
        {
            string output = string.Empty;

            foreach (byte b in array)
            {
                output += b.ToString("x2");
            }

            return output;
        }
    }
}
