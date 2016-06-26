using RedAlert.API.BL;
using RedAlert.API.Models;
using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
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

                try
                {
                    model = await dm.AddDeviceAsync(model.SerialNumber);
                }
                catch (ArgumentException)
                {
                    ModelState.AddModelError("SerialNumber", "Invalid serial number.");
                }
            }

            return View(model);
        }
        
        public ActionResult Get()
        {
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
    }
}