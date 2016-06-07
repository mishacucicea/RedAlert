using RedAlert.API.BL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    public class DeviceController : Controller
    {

        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        // GET: Device
        public ActionResult Register()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Register(DeviceModel model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                var response = await client.PostAsJsonAsync("api/DeviceIdentity", model.SerialNumber);
                if (response == null) throw new ArgumentNullException(nameof(response));
                ViewData["deviceKey"] =  response.Content.ReadAsStringAsync().Result;
            }
            return View();
        }
        
        public ActionResult Get()
        {

            return View();
        }
        
        [HttpPost]
        public async Task<ActionResult> Get(Device model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                var response =  await client.GetAsync("api/DeviceIdentity/" + model.SerialNumber);
                
                ViewData["deviceKey"] = response.Content.ReadAsStringAsync().Result;
            }
           
            return View();
        }

        public async Task<ActionResult> List()
        {
            DeviceManagement dm = new DeviceManagement();
            var devices = await dm.GetDevices();

            return View(devices);
        }

        public ActionResult ViewSendMessage()
        {
            return View("SendMessage");
        }

        //why do we need it??
        //public async Task<ActionResult> SendMessage(string id, string message)
        //{
        //    await IotHubHelper.SendCloudToDeviceMessageAsync(id, message);

        //    return View("SendMessage");
        //}
    }
}