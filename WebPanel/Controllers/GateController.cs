using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    // دروازه ها
    [Authorize(Roles = "admin, adminOnePort")]
    public class GateController : Controller
    {
        private IGateService _gateService { get; set; }

        public GateController(IGateService gateService)
        {
            _gateService = gateService;
        }

        #region Gear
        public async Task<IActionResult> Index(GetGateFilterDto filter, CancellationToken token = default)
        {
            var result = await _gateService.GetGates(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateGate(Guid gateId, CancellationToken token = default)
        {
            if (gateId != Guid.Empty)
            {
                var dto = await _gateService.GetGateById(gateId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGate(CreateGateDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _gateService.AddOrUpdate(dto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                    return View(dto);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> DeleteGate(Guid gateId, CancellationToken token = default)
        {
            TempData["msg"] = await _gateService.DeleteGate(gateId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetGateById(Guid gateId, CancellationToken token = default)
        {
            var find = await _gateService.GetGateById(gateId, token);
            return PartialView("_CreateGate", find);
        }

        #endregion
    }
}
