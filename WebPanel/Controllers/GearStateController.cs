using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    //دستگاه های FPN
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearStateController : Controller
    {
        public GearStateController(IGearStateService gearStateService)
        {
            _gearStateService = gearStateService;
        }

        private IGearStateService _gearStateService { get; }


        #region GearState
        public async Task<IActionResult> Index(CancellationToken token = default, int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await _gearStateService.GetGearStates(pagenumaber, token);
            return View(result);
        }
        public async Task<IActionResult> CreateGearState(int gearStateId, CancellationToken token = default)
        {
            if (gearStateId > 0)
            {
                var dto = await _gearStateService.GetGearStateById(gearStateId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGearState(CreateGearStateDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _gearStateService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteGearState(int gearStateId, CancellationToken token = default)
        {
            TempData["msg"] = await _gearStateService.DeleteGearState(gearStateId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetGearStateById(int gearStateId, CancellationToken token = default)
        {
            var find = await _gearStateService.GetGearStateById(gearStateId, token);
            return PartialView("_CreateGearState", find);
        }
        #endregion


    }
}
