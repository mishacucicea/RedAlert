using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    //[OutputCache(Location = System.Web.UI.OutputCacheLocation.Any, Duration = 60)]
    public class HomeController : BaseController
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";        
            return View();
        }

        public ActionResult Error()
        {
            ViewBag.Title = "Error";

            return PartialView("Error");
        }
    }
}
