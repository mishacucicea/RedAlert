using RedAlert.API.BL;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RedAlert.API.Controllers
{
    [RoutePrefix("api/device")]
    public class DeviceIdentityController : ApiController
    {



        
        //[HttpGet]
        //public async Task<IHttpActionResult> Get(string id)
        //{
        //    //the expected ID is the id generated on the portal

        //    //check that the key exists in the database
        //    //generate a connection string and return it
        //    //push a C2D message with the latest status (if any) - will be delivered when connection is established (hopefully)

        //    string deviceId = "pocDevice";
        //    string connectionString = GenerateDeviceConnectionString(deviceId);

        //    return Ok(connectionString);
        //}

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]string id)
        {
           HttpResponseMessage response;
           
            try
            {
               response = Request.CreateResponse(HttpStatusCode.Created);
               var deviceKey= await IotHubHelper.AddDeviceAsync(id);
               response.Content = new StringContent(deviceKey);
                
            }
            catch
            {
                response= Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return response;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string id)
        {
            string deviceId = "pocDevice";
            string connectionString = GenerateDeviceConnectionString(deviceId);

            HttpResponseMessage response;

            try
            {
                response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(connectionString);
            }
            catch
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return response;
        }

        /*
 * var generateSasToken = function(resourceUri, signingKey, policyName, expiresInMins) {
resourceUri = encodeURIComponent(resourceUri.toLowerCase()).toLowerCase();

// Set expiration in seconds
var expires = (Date.now() / 1000) + expiresInMins * 60;
expires = Math.ceil(expires);
var toSign = resourceUri + '\n' + expires;

// using crypto
var decodedPassword = new Buffer(signingKey, 'base64').toString('binary');
const hmac = crypto.createHmac('sha256', decodedPassword);
hmac.update(toSign);
var base64signature = hmac.digest('base64');
var base64UriEncoded = encodeURIComponent(base64signature);

// construct autorization string
var token = "SharedAccessSignature sr=" + resourceUri + "&sig="
+ base64UriEncoded + "&se=" + expires;
if (policyName) token += "&skn="+policyName;
// console.log("signature:" + token);
return token;
};
 */
        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceId">IoT Hub DeviceId</param>
        /// <returns></returns>
        private string GenerateDeviceConnectionString(string deviceId)
        {
            //Device Policy Primary key:
            //2BiZDXCkPaIgSAV0qPgREpNd+mfvy9uEjoRQQyLUl0o=
            byte[] signingKey = Convert.FromBase64String("2BiZDXCkPaIgSAV0qPgREpNd+mfvy9uEjoRQQyLUl0o=");

            string expiring = DateTimeToUnixTimestamp(DateTime.Now.AddHours(24)).ToString();

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

    }
}
