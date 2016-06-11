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
        [Obsolete("Just delete it..")]
        [HttpPost]
        public async Task<IHttpActionResult> Post([FromBody]DeviceModel device)
        {
            try
            {
                DeviceManagement dm = new DeviceManagement();
                
                device = await dm.AddDeviceAsync(device.SerialNumber);

                return Ok(device);
                
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [Obsolete("Just delete it...")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get(int id)
        {
            throw new NotImplementedException();
        }

       

    }
}
