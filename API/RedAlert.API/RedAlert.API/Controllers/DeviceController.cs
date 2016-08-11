using RedAlert.API.BL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    [Authorize]
    public class DeviceController : BaseController
    {
        /// <summary>
        /// The Api Url.
        /// </summary>
        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        /// <summary>
        /// The device management.
        /// </summary>
        DeviceManagement dm = new DeviceManagement();

        /// <summary>
        /// Shows the Register view.
        /// </summary>
        public ActionResult Register()
        {
            DeviceModel model = new DeviceModel();
            return View(model);
        }

        /// <summary>
        /// Registers the given device.
        /// </summary>
        /// <param name="model">The device.</param>
        [HttpPost]
        public async Task<ActionResult> Register(DeviceModel model)
        {
            try
            {
                model = await dm.AddDeviceAsync(model.SerialNumber);
            }
            catch (ArgumentOutOfRangeException)
            {
                ModelState.AddModelError("SerialNumber", "Already registered.");
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
            SetDefaultApiUrl(model);

            return View(model);
        }

        public ActionResult Get()
        {
            return View();
        }

        /// <summary>
        /// Action that shows the list of devices.
        /// </summary>
        public async Task<ActionResult> List()
        {
            var devices = await dm.GetDevices();
            List<Task<Microsoft.Azure.Devices.Device>> list = new List<Task<Microsoft.Azure.Devices.Device>>();

            foreach (var device in devices)
            {
                list.Add(dm.GetCloudDeviceAsync(device.HubDeviceId));
            }

            await Task.WhenAll(list.ToArray());

            for (int i = 0; i < devices.Count; i++)
            {
                devices[i].LastActivityTime = list[i].Result.LastActivityTime;
            }

            return View(devices);
        }

        public ActionResult GenerateUrl()
        {
            return View();
        }

        /// <summary>
        /// Populates and shows the Mood view.
        /// </summary>
        /// <param name="id">The device id.</param>
        /// <param name="senderKey">The device sender key.</param>
        public async Task<ActionResult> Mood(int id, string senderKey)
        {
            if (string.IsNullOrEmpty(senderKey))
            {
                // sender Key was not given, get the device by id.
                var device = await dm.GetDeviceAsync(id);
                senderKey = device.SenderKey;
            }

            ViewBag.SenderKey = senderKey;

            return View();
        }

        /// <summary>
        /// Resets the device/sender keys.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The modified device.</returns>
        public async Task<ActionResult> ResetKeys(int id)
        {
            var device = await dm.ResetDeviceKeysAsync(id);
            SetDefaultApiUrl(device);

            return View("Details", device);
        }

        /// <summary>
        /// Removes a device.
        /// </summary>
        /// <param name="id">The device id.</param>
        public async Task<ActionResult> Remove(int id)
        {
            await dm.RemoveDeviceAsync(id);

            return RedirectToAction("List");
        }

        /// <summary>
        /// Populates the given device with a default Api URL.
        /// </summary>
        /// <param name="device">The device.</param>
        private void SetDefaultApiUrl(Device device)
        {
            device.ApiUrl = $"{ApiUrl}/api/message/send?senderkey={device.SenderKey}&color=red";
        }
    }
}