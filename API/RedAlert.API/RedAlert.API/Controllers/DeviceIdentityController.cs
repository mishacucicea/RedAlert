using RedAlert.API.BL;
using RedAlert.API.DAL;
using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Linq;
using RedAlert.API.Models;

namespace RedAlert.API.Controllers
{
    //[RoutePrefix("api/device")]
    public class DeviceIdentityController : ApiController
    {

        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]DeviceModel device)
        {
            try
            {
                DeviceManagement dm = new DeviceManagement();
                
                device = await dm.AddDeviceAsync(device.SerialNumber);

                return Ok(device);
                
            }
            catch
            {
                return BadRequest();
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> Get(string id)
        {
            throw new NotImplementedException();
        }

       

    }
}
