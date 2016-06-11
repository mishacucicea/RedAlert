using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    public class GroupController : BaseController
    {
        //
        // GET: /Group/
        public ActionResult Create(DeviceGroup model)
        {
            var name = "test";
            var password = "pass";
            var list = new List<Device>();
            if (name == model.Name && password == model.Password)
            {
                ViewData["result"] = "success";
            }
            return View();
        }
        public ActionResult Get(DeviceGroup model)
        {
           
            return View();
        }
	}
}