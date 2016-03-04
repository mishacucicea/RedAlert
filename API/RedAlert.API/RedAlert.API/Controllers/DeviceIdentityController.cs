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
    [RoutePrefix("api/device")]
    public class DeviceIdentityController : ApiController
    {

        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody]string id)
        {
           HttpResponseMessage response;
           
            try
            {
               response = Request.CreateResponse(HttpStatusCode.Created);
               var deviceKey= await DeviceIdentity.AddDeviceAsync(id);
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
            HttpResponseMessage response;

            try
            {
                response = Request.CreateResponse(HttpStatusCode.OK);

                var deviceKey = await DeviceIdentity.GetDeviceAsync(id);
                response.Content = new StringContent(deviceKey);
            }
            catch
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return response;
        }
    }
}
