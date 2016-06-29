using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using RedAlert.API.BL;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{

    public class VotingController : BaseController
    {
        
        [HttpGet]
        public ActionResult Up()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.YesAnswer++;

            return View("Thanks");
            //return Content("Success! You have Up Voted.");
        }

        [HttpGet]
        public ActionResult Down()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.NoAnswer++;

            return View("Thanks");
        }
    }
}
