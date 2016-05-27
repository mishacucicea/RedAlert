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
        public async Task<IHttpActionResult> Send(string deviceId, string message)
        {
            try
            {
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
    }
}
