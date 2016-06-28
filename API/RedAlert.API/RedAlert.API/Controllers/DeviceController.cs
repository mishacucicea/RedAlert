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

        DeviceManagement dm = new DeviceManagement();

        // GET: Device
        public ActionResult Register()
        {
            DeviceModel model = new DeviceModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Register(DeviceModel model)
        {
            try
            {
                model = await dm.AddDeviceAsync(model.SerialNumber);
            }
            catch (ArgumentException)
            {
                ModelState.AddModelError("SerialNumber", "Invalid serial number.");
            }

            return View(model);
        }

        /// <summary>
        /// Details the specified device.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>The view corresponding to the device details.</returns>
        public async Task<ActionResult> Details(int id)
        {
            var model = await dm.GetDeviceAsync(id);

            ViewBag.ApiUrl = $"{ApiUrl}/api/message/send?senderkey={model.SenderKey}&color=red";

            return View(model);
        }

        public ActionResult Get()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
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

        public async Task<ActionResult> Mood()
        {
            return View();
        }
    }
}