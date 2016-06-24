using RedAlert.API.BL;
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

namespace RedAlert.API.Controllers
{
    public class MessageController : ApiController
    {
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
            try
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
                    await dm.SendCloudToDeviceMessageAsync(device.DeviceKey, message);
                }
                return Ok();
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> SendToGroup(string groupName, string message)
        {



            throw new NotImplementedException();


        }

        public static byte[] StringToByteArray(string hex)
        {
            if (hex.Length % 2 != 0) throw new FormatException("Not a hex string.");

            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
