﻿using RedAlert.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RedAlert.API.BL;
using RedAlert.API.DAL;

namespace RedAlert.API.Controllers
{
    [Authorize]
    public class GroupController : BaseController
    {
        DeviceManagement dm = new DeviceManagement();
        private RedAlertContext _db;
        public GroupController()
        {
            _db = new RedAlertContext();
        }

        //
        // GET: /Group/
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(string groupName)
        {
            var group = await _db.DeviceGroups.SingleOrDefaultAsync(x => x.Name == groupName);
            if (group != null)
            {
                ModelState.AddModelError("groupName", "This group already exists");
            }
            else
            {
                _db.DeviceGroups.Add(new DeviceGroup() { Name = groupName });
                await _db.SaveChangesAsync();

                ViewBag.Result = $"Group {groupName} has been added";
            }
            return View();
        }
        public ActionResult Get(DeviceGroup model)
        {

            return View();
        }

        [HttpGet]
        public ViewResult SendMessageToGroup()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> SendMessageToGroup(string groupName, string color)
        {
            var group = await _db.DeviceGroups.Include(d => d.Devices).SingleOrDefaultAsync(x => x.Name == groupName);
            if (group == null)
            {
                ModelState.AddModelError("groupName", "This group does not exist");
            }
            else
            {
                var devices = group.Devices.ToList();
                try
                {
                    await GroupHelper.SendGroupMessage(devices, color);
                    ViewBag.Result = $"The message was send to the group";
                }
                catch (Exception e)
                {
                    ViewBag.Result = $"Something went wrong, the error is {e.Message}";
                    Logger.Error(e);
                }
            }
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddDeviceToGroup(string serialNumber, string groupName)
        {
            var gn = await _db.DeviceGroups.Include(d => d.Devices).SingleOrDefaultAsync(x => x.Name == groupName);
            if (gn == null)
            {
                ModelState.AddModelError("groupName", "Invalid Group Name");
            }

            var device = await _db.Devices.SingleOrDefaultAsync(x => x.SerialNumber == serialNumber);
            
            if (device == null)
            {
                ModelState.AddModelError("serialNumber", "Invalid Serial ");
            }
            if (ModelState.IsValid)
            {
                if (gn.Devices.Exists(x => x.SerialNumber == serialNumber))
                {
                    ViewBag.Result = "This group has already this added device ";
                }
                else
                {
                    gn.Devices.Add(device);
                    TryUpdateModel(gn);
                    await _db.SaveChangesAsync();
                    ViewBag.Result = "Device has been added";
                }
            }
           
            return View(GroupHelper.GetDevices());
            
        }
        [HttpGet]
        public ActionResult AddDeviceToGroup()
        {
            return View(GroupHelper.GetDevices());
        }

    }
}