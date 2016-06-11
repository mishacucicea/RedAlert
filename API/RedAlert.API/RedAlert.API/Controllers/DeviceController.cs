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
    public class DeviceController : BaseController
    {

        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        // GET: Device
        public ActionResult Register()
        {
            DeviceModel model = new DeviceModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Register(DeviceModel model)
        {
            using (var client = new HttpClient())
            {
                DeviceManagement dm = new DeviceManagement();

                model = await dm.AddDeviceAsync(model.SerialNumber);

                //client.BaseAddress = new Uri(ApiUrl);
                //var response = await client.PostAsJsonAsync("http://localhost:63913/api/DeviceIdentity", model);
                //if (!response.IsSuccessStatusCode) //throw BUBU?
                //if (response == null) throw new ArgumentNullException(nameof(response));
                //model =  await response.Content.ReadAsAsync<DeviceModel>();
            }
            return View(model);
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


        public ActionResult GenerateUrl()
        {
            return View();
        }

        //why do we need it??
        //public async Task<ActionResult> SendMessage(string id, string message)
        //{
        //    await IotHubHelper.SendCloudToDeviceMessageAsync(id, message);

        //    return View("SendMessage");
        //}
    }
}