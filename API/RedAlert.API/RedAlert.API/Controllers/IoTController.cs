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

namespace RedAlert.API.Controllers
{
    /// <summary>
    /// This controller is responsible for providing the devices with required API methods.
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
                string hubAddress = "arduhub.azure-devices.net";

                string hubName = deviceId;
                string hubUser = $"{hubAddress}/{deviceId}";
                string hubConn = GenerateDeviceConnectionString(deviceId);
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
                catch
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
        private string GenerateDeviceConnectionString(string deviceId)
        {
            //Device Policy Primary key:
            //2BiZDXCkPaIgSAV0qPgREpNd+mfvy9uEjoRQQyLUl0o=
            string devicePolicyKey = ConfigurationManager.AppSettings["DevicePolicyKey"];
            byte[] signingKey = Convert.FromBase64String("2BiZDXCkPaIgSAV0qPgREpNd+mfvy9uEjoRQQyLUl0o=");

            //add one extra hour for uncallibrated RTCs
            string expiring = DateTimeToUnixTimestamp(DateTime.Now.AddHours(25)).ToString();

            //should be getting this from the config
            string resourceUri = ("arduhub.azure-devices.net/devices/" + deviceId).ToLower();

            string signature = resourceUri + "\n" + expiring;

            HMACSHA256 hash = new HMACSHA256(signingKey);

            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            byte[] message = encoding.GetBytes(signature);
            string signedMessage = HttpUtility.UrlEncode(Convert.ToBase64String(hash.ComputeHash(message)));
            string encodedResourceUri = HttpUtility.UrlEncode(resourceUri);

            string connectionString = $"SharedAccessSignature sig={signedMessage}&se={expiring}&sr={encodedResourceUri}";

            return connectionString;
        }

        private long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        #endregion private methods
    }
}
