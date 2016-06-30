﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using RedAlert.API.BL;

namespace RedAlert.API.Controllers
{
    public class ColorController : BaseController
    {
        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
        // GET: Color
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Voting()
        {
            return View();
        }
        [HttpPost]
        public async System.Threading.Tasks.Task<ActionResult> Voting(string senderKey)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                var color = ColorHelper.CreateColorFromPercent();
                
                await client.GetAsync("/api/message/send?senderkey=" + senderKey + "&color=" + color + "&pattern=fixed");
                         
            }
            ViewData["Votes"] = ColorHelper.TotalResponse;
            ViewData["Up"] = ColorHelper.YesAnswer;
            ViewData["Down"] = ColorHelper.NoAnswer;
            return View();
        }
    }
}