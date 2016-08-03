using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using RedAlert.API.BL;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    [System.Web.Mvc.Authorize]
    public class VotingController : BaseController
    {
        
        [System.Web.Mvc.HttpGet]
        public ActionResult Up()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.YesAnswer++;

            return View("Thanks");
            //return Content("Success! You have Up Voted.");
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Down()
        {
            ColorHelper.TotalResponse++;
            ColorHelper.NoAnswer++;

            return View("Thanks");
        }

        [System.Web.Mvc.HttpGet]
        public ActionResult Reset()
        {
            ColorHelper.TotalResponse = 0;
            ColorHelper.NoAnswer = 0;
            ColorHelper.YesAnswer = 0;
            return View();
        }

    }
}
