using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    public class DeviceController : Controller
    {
        // GET: Device
        public ActionResult Create()
        {
            
            return View();
        }
        [HttpPost]
        public ActionResult Create(DeviceModel model)
        {
            return View();
        }

        public ActionResult Get()
        {
            return View();
        }
    }
}