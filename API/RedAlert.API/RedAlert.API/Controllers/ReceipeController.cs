using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    public class ReceipeController : BaseController
    {
        // GET: Receipe
        public ActionResult Index()
        {
            return View();
        }
    }
}