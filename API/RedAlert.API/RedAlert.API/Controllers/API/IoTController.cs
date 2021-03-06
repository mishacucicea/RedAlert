﻿using RedAlert.API.BL;
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
using System.IO;
using System.Net.Http.Headers;
using System.Text;
using NLog;

namespace RedAlert.API.Controllers.API
{
    /// <summary>
    /// Device facing API - This controller is responsible for providing the devices with required API methods.
    /// </summary>
    public class IoTController : ApiController
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
        public IoTController()
        {
            Logger = LogManager.GetLogger("RedAlert");
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Authentication(string deviceKey)
        {
            Logger.Debug($"Authentication device with key: {deviceKey}");
            //the expected ID is the id generated on the portal (consider as an API key)

            //check that the key exists in the database
            //generate a connection string and return it
            //push a C2D message with the latest status (if any) - will be delivered when connection is established (hopefully)

            using (RedAlertContext context = new RedAlertContext())
            {
                Device device = context.Devices.SingleOrDefault(x => x.DeviceKey == deviceKey);

                if (device == null)
                {
                    Logger.Debug($"Device key was invalid: {deviceKey}");
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

                Tuple<string, string> deviceConnString = await GenerateDeviceConnectionStrings(deviceId);
                string responseMessage = string.Empty;

                if (!string.IsNullOrEmpty(deviceConnString.Item1))
                {
                    string hubAddress = ConfigurationManager.AppSettings["PrimaryIotHubUri"];

                    string hubName = deviceId;
                    string hubUser = $"{hubAddress}/{deviceId}";
                    string hubConn = deviceConnString.Item1;
                    string hubTopic = $"devices/{deviceId}/messages/devicebound/#";

                    responseMessage += $"{hubAddress}\n{hubName}\n{hubUser}\n{hubConn}\n{hubTopic}";
                }

                if (!string.IsNullOrEmpty(deviceConnString.Item2))
                {
                    string hubAddress2 = ConfigurationManager.AppSettings["SecondaryIotHubUri"];

                    string hubName2 = deviceId;
                    string hubUser2 = $"{hubAddress2}/{deviceId}";
                    string hubConn2 = deviceConnString.Item2;
                    string hubTopic2 = $"devices/{deviceId}/messages/devicebound/#";

                    if (!string.IsNullOrEmpty(responseMessage))
                    {
                        responseMessage += '\n';
                    }

                    responseMessage += $"{hubAddress2}\n{hubName2}\n{hubUser2}\n{hubConn2}\n{hubTopic2}";
                }
                HttpResponseMessage response;

                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(responseMessage);

                // if there was a previous message we have to resend it to make sure the device is always
                //showing the latest state
                if (device.LastMessage != null)
                {
                    //TODO: it does not behave well with messages that have a timeout/expiry
                    DeviceManagement dm = new DeviceManagement();
                    await dm.SendCloudToDeviceMessageAsync(device.DeviceKey, device.LastMessage);
                }

                return response;
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Update()
        {
            IEnumerable<string> headerValues = Request.Headers.GetValues("x-ESP8266-free-space");

            //0 will also be considered as unknown
            int freeSpace;
            int.TryParse(headerValues.FirstOrDefault(), out freeSpace);

            headerValues = Request.Headers.GetValues("x-ESP8266-version");
            string version = headerValues.FirstOrDefault();

            //we need the version so we don't update the device in an infinite loop
            if (string.IsNullOrEmpty(version))
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }

            //if version is already up to date then return 304
            string[] splitVersion = version.Split('-');
            int deviceVersionNumber = int.MaxValue;
            string deviceVersion = "dev";
            if (splitVersion.Count() == 2)
            {
                deviceVersionNumber = int.Parse(splitVersion[1]);
            }
            else if (splitVersion.Count() == 3)
            {
                deviceVersion = splitVersion[1];
                deviceVersionNumber = int.Parse(splitVersion[2]);
            }

            if (deviceVersion == "dev")
            {
                if (deviceVersionNumber >= 12)
                {
                    return Request.CreateResponse(HttpStatusCode.NotModified);
                }

                string newVersion = "v2-dev-12";

                Logger.Debug($"Updating device from version {version} to {newVersion}.");

                HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
                var stream = new FileStream(System.Web.HttpContext.Current.Server.MapPath("~/Firmware/" + newVersion + ".bin"), FileMode.Open);

                //have to calculate md5
                MD5 md5 = System.Security.Cryptography.MD5.Create();
                byte[] hash = md5.ComputeHash(stream);
                //initialize with 32 to avoid buffer resizing
                StringBuilder sb = new StringBuilder(32);
                for (int i = 0; i < hash.Length; i++)
                {
                    sb.Append(hash[i].ToString("x2"));
                }

                string md5hash = sb.ToString();

                //Don't forget to rest the stream!
                stream.Position = 0;
                result.Content = new StreamContent(stream);
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = newVersion + ".bin"
                };
                result.Content.Headers.Add("x-MD5", md5hash);


                return result;
            }

            return Request.CreateResponse(HttpStatusCode.NotModified);
        }

        #region private methods

        private async Task<Tuple<string, string>> GenerateDeviceConnectionStrings(string deviceId)
        {
            DeviceManagement dm = new DeviceManagement();
            Tuple<string, string> deviceSasKeys = await dm.GetDeviceSASKeys(deviceId);

            string sasKeyPrimary = string.Empty;
            string sasKeySecondary = string.Empty;

            if (!string.IsNullOrEmpty(deviceSasKeys.Item1))
            {
                var sasBuilderPrimary = new SharedAccessSignatureBuilder()
                {
                    Key = deviceSasKeys.Item1,
                    Target = String.Format("{0}/devices/{1}", ConfigurationManager.AppSettings["PrimaryIotHubUri"], WebUtility.UrlEncode(deviceId)),

                    TimeToLive = TimeSpan.FromHours(25)
                };

                sasKeyPrimary = sasBuilderPrimary.ToSignature();
            }

            if (!string.IsNullOrEmpty(deviceSasKeys.Item2))
            {
                var sasBuilderSecondary = new SharedAccessSignatureBuilder()
                {
                    Key = deviceSasKeys.Item2,
                    Target = String.Format("{0}/devices/{1}", ConfigurationManager.AppSettings["SecondaryIotHubUri"], WebUtility.UrlEncode(deviceId)),

                    TimeToLive = TimeSpan.FromHours(25)
                };

                sasKeySecondary = sasBuilderSecondary.ToSignature();
            }

            return new Tuple<string, string>(sasKeyPrimary, sasKeySecondary);
        }

        private long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        #endregion private methods
    }
}
