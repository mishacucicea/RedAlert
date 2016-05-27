using RedAlert.API.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RedAlert.API.Controllers
{
    public class MessageController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="color">RGB (six character) or named color. Case insensitive.</param>
        /// <param name="pattern">Specify the kind of pattern to use. Default is continunous.</param>
        /// <param name="timeout">How much time to keep the given color on (in seconds).</param>
        [HttpGet]
        public async Task<IHttpActionResult> Send(string deviceId, string color, string pattern = null, uint? timeout = null)
        {
            try
            {
                //we'll be building the message to be sent to the IoT Hub

                byte[] message = new byte[9];
                //1 byte is the type of the message
                //3 bytes is the RGB value
                //1 bytes is the pattern number
                //4 bytes is the timeout
                message[0] = 1;

                try
                {
                    System.Drawing.Color col = System.Drawing.ColorTranslator.FromHtml("#"+color);
                }
                catch
                {
                    System.Drawing.Color colorRGB = System.Drawing.Color.FromName(color);

                    if (colorRGB.ToArgb() == 0)
                    {
                        return BadRequest("Unknown color.");
                    }

                    message[1] = colorRGB.R;
                    message[2] = colorRGB.G;
                    message[3] = colorRGB.B;
                }
                
                if (timeout.HasValue)
                {
                    byte[] timeoutBytes = BitConverter.GetBytes(timeout.Value);

                    //because we're running on a little indian machine
                    Array.Reverse(timeoutBytes);
                    Array.Copy(timeoutBytes, 0, message, 4, 3);
                }

                await IotHubHelper.SendCloudToDeviceMessageAsync(deviceId, message);

                return Ok();
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> SendToGroup(string groupName,string message)
        {
            var groupList = new List<string>();
            groupList.Add("sl2yUZS+bC1fN/vU7/uxrkV3g9Z45oXvjPCpTY9kAws=");
            groupList.Add("WZZqpXbpSAwuBBS9VoWam+eqc+2C59ENk/yjMv1OOzw=");
            groupList.Add("5y/tgCuiCsriN8t71FCndQosAeBqf1DUmWx/ZbmUDkg=");

            try
            {
                foreach (var item in groupList)
                {
                    await IotHubHelper.SendCloudToDeviceMessageAsync(item, message);
                }

                return Ok();
            }
            catch
            {
                return InternalServerError();
            }
        }

        //[HttpGet]
        //public async Task<HttpResponseMessage> Receive(string id)
        //{
        //    HttpResponseMessage response;
        //    try
        //    {
        //        var message = await IotHubHelper.ReceiveMessage(id);

        //        response = Request.CreateResponse(HttpStatusCode.OK, message);
        //    }
        //    catch(Exception ex)
        //    {
        //        response = Request.CreateResponse(HttpStatusCode.InternalServerError);
        //    }
        //    return response;
        //}

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
