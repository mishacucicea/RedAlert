﻿using RedAlert.API.BL;
using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace RedAlert.API.Controllers
{
    public class DeviceController : Controller
    {

        private string ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];

        // GET: Device
        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        public  ActionResult Create(DeviceModel model)
        {
            HttpResponseMessage response;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                response = client.PostAsJsonAsync("api/DeviceIdentity", model.SerialNumber).Result;
                ViewData["deviceKey"] =  response.Content.ReadAsStringAsync().Result;
            }
            return View();
        }

        public ActionResult Get()
        {

            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Get(DeviceModel model)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiUrl);
                var response =  await client.GetAsync("api/DeviceIdentity/" + model.SerialNumber);
                
                ViewData["deviceKey"] = response.Content.ReadAsStringAsync().Result;
            }
           
            return View();
        }

        public async Task<ActionResult> List()
        {
            var devices = await IotHubHelper.GetDevices();

            return View(devices);
        }

        public ActionResult ViewSendMessage()
        {
            return View("SendMessage");
        }

        public async Task<ActionResult> SendMessage(string id, string color)
        {
            await IotHubHelper.SendMessage(id, color);

            return View("SendMessage");
        }
    }
}