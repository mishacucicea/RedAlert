using RedAlert.API.BL;
using RedAlert.API.DAL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Azure.Devices.Common.Security;

namespace RedAlert.API.Controllers
{
    /// <summary>
    /// Device facing API - This controller is responsible for providing the devices with required API methods.
    /// </summary>
    public class IoTController : ApiController
    {
        [HttpGet]
        public async Task<HttpResponseMessage> Authentication(string deviceKey)
        {
            //the expected ID is the id generated on the portal (consider as an API key)

            //check that the key exists in the database
            //generate a connection string and return it
            //push a C2D message with the latest status (if any) - will be delivered when connection is established (hopefully)

            using (RedAlertContext context = new RedAlertContext())
            {
                Device device = context.Devices.SingleOrDefault(x => x.DeviceKey == deviceKey);

                if (device == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Not allowed.");
                }

                //1. hubAddress "arduhub.azure-devices.net"
                //2. hubName "pocDevice"
                //3. hubUser "arduhub.azure-devices.net/pocDevice"
                //4. hubConn "SharedAccessSignature sr=arduhub.azure-devices.net%2fdevices%2fpocDevice&sig=ksApO9qnlvs%2bERTKS3qqvO0T7cRG2D1xhI7PiE5C8uk%3d&se=1490896187"
                //5. hubTopic "devices/pocDevice/messages/devicebound/#"

                //device id should be looked up in the db based on the api key
                string deviceId = device.HubDeviceId;

                //should get from the config

                string hubAddress = ConfigurationManager.AppSettings["IotHubUri"];

                string hubName = deviceId;
                string hubUser = $"{hubAddress}/{deviceId}";
                string hubConn = await GenerateDeviceConnectionString(deviceId);
                string hubTopic = $"devices/{deviceId}/messages/devicebound/#";

                string responseMessage = $"{hubAddress}\n{hubName}\n{hubUser}\n{hubConn}\n{hubTopic}";

                HttpResponseMessage response;

                try
                {
                    response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(responseMessage);

                    // if there was a previous message we have to resend it to make sure the device is always
                    //showing the latest state
                    if (device.LastMessage != null)
                    {
                        //TODO: it does not behave well with messages that have a timeout/expiry
                        DeviceManagement dm = new DeviceManagement();
                        await dm.SendCloudToDeviceMessageAsync(device.HubDeviceId, device.LastMessage);
                    }
                }
                catch (Exception e)
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError);
                }

                return response;
            }
        }


        #region private methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId">IoT Hub DeviceId</param>
        /// <returns></returns>
        private async Task<string> GenerateDeviceConnectionString(string deviceId)
        {
            DeviceManagement dm = new DeviceManagement();
            string deviceSasKey = await dm.GetDeviceSASKey(deviceId);

            var sasBuilder = new SharedAccessSignatureBuilder()
            {
                Key = deviceSasKey,
                Target = String.Format("{0}/devices/{1}", ConfigurationManager.AppSettings["IotHubUri"], WebUtility.UrlEncode(deviceId)),
                
                TimeToLive = TimeSpan.FromHours(25)
            };

            string sasKey = sasBuilder.ToSignature();

            return sasKey;
        }

        private long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        #endregion private methods
    }
}
