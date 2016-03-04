using RedAlert.API.Models;
using System;
using System.Collections.Generic;
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
                client.BaseAddress = new Uri("http://localhost:8080/");
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
        public ActionResult Get(DeviceModel model)
        {
            
            
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://localhost:8080/");
                var response = client.GetAsync("api/DeviceIdentity?=" + model.SerialNumber);
                
                //ViewData["deviceKey"] = response.Result;
            }
           
            return View();
        }
    }
}