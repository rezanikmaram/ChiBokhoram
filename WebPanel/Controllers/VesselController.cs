using Common;
using Common.enumeration.Hirman;
using Common.Exceptions;
using Entities.DTOs;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class VesselController : Controller
    {
        private readonly IVesselService vesselService;
        private readonly ILogger _logger;
        public IPortService PortService { get; }
        public VesselController(IVesselService vesselService, IPortService portService, ILogger<VesselController> logger)
        {
            this.vesselService = vesselService;
            PortService = portService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? p = null, VesselFilterDto filter = null, CancellationToken token = default)
        {
            ViewBag.Ports = await PortService.GetPortDropDown(token);
            int pagenumaber = p ?? 1;
            var result = await vesselService.GetVessel(filter, pagenumaber, token);

            return View(result);
        }
        public async Task<IActionResult> VesselDetail(Guid vesselId, CancellationToken token = default)
        {
            //var dd = await seoService.GetSeoDto(seoId, token);
            var dto = await vesselService.GetVesselDetail(vesselId, token);
            return View(dto);
        }
        public async Task<IActionResult> GetVesselById(Guid vesselId, DateTimeOffset date, bool isJustStatus = false, CancellationToken token = default)
        {
            if (!isJustStatus)
            {
                ViewBag.Ports = await PortService.GetPortDropDown(token);
                var model = await vesselService.GetVesselDto(vesselId, token);
                return PartialView("_CreateVessel", model);
            }
            ViewBag.ListStatus = await GetStatusDropDown();

            var statusmodel = await vesselService.GetVesselStatusDto(vesselId, date, token);
            return PartialView("_ChangeStatusVessel", statusmodel);
        }
        [HttpPost]
        public async Task<IActionResult> CreateVessel(CreateVesselDto dto, CancellationToken token = default)
        {
            try
            {
                ModelState.Remove("Id");
                if (ModelState.IsValid)
                {
                    await vesselService.AddOrUpdate(dto, token);
                    TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Success, "عملیات با موفقیت انجام شد"));
                    return RedirectToAction(nameof(Index));
                }

                ViewBag.Ports = await PortService.GetPortDropDown(token);

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError("VesselCreate: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }

        }


        public async Task<IActionResult> DeleteVessel(Guid vesselId, CancellationToken token = default)
        {
            TempData["msg"] = await vesselService.DeleteVessel(vesselId, token);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeStatusVessel(ChangeVesselStatusDto dto, CancellationToken token = default)
        {
            // var dd = await seoService.GetSeoDto(seoId, token);
            await vesselService.ChangeStatusVessel(dto, token);
            return RedirectToAction(nameof(DashboardVessel), new { date = dto.LogDate });
        }

        public async Task<IActionResult> DashboardVessel(DateTimeOffset date, Guid? vesselId, int? p = null, CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var result = await vesselService.GetDashboardVessel(date, vesselId, pagenumaber, token);

            return View(result);
        }
        private async Task<SelectList> GetStatusDropDown()
        {
            List<DropDownIntDto> result = new();
            foreach (var field in typeof(EquipmentStatus).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();

                if (displayAttribute != null)
                {
                    result.Add(new DropDownIntDto
                    {
                        Id = Convert.ToInt32(field.GetValue(typeof(EquipmentStatus))),
                        Title = displayAttribute.Name
                    });
                }
            }


            return result.ToSelectList();
        }
    }
}
