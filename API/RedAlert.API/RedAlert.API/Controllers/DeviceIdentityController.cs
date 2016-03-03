using RedAlert.API.BL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace RedAlert.API.Controllers
{
    public class DeviceIdentityController : ApiController
    {

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string deviceId)
        {
           HttpResponseMessage response;
           
            try
            {
                response = Request.CreateResponse(HttpStatusCode.Created);
                DeviceIdentity device = new DeviceIdentity(deviceId);
                var deviceKey = await device.GetDeviceAsync(deviceId);
                response.Content = new StringContent(deviceKey);
            }
            catch
            {
                response= Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return response;
        }
    }
}
