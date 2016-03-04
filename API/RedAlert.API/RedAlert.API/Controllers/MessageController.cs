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
        
        [HttpGet]
        public async Task<HttpResponseMessage> Send(string id, string message)
        {
            HttpResponseMessage response;
            try
            {
                response = Request.CreateResponse(HttpStatusCode.OK);

               await IotHubHelper.SendMessage(id, message);
            }
            catch
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
            return response;
        }
    }
}
