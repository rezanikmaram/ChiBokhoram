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
    public class GearTypeController : Controller
    {
        public GearTypeController(IGearTypeService gearTypeService)
        {
            _gearTypeService = gearTypeService;
        }

        private IGearTypeService _gearTypeService { get; }


        #region GearType
        public async Task<IActionResult> Index(CancellationToken token = default, int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await _gearTypeService.GetGearTypes(pagenumaber, token);
            return View(result);
        }
        public async Task<IActionResult> CreateGearType(int gearTypeId, CancellationToken token = default)
        {
            if (gearTypeId > 0)
            {
                var dto = await _gearTypeService.GetGearTypeById(gearTypeId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGearType(CreateGearTypeDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _gearTypeService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteGearType(int gearTypeId, CancellationToken token = default)
        {
            TempData["msg"] = await _gearTypeService.DeleteGearType(gearTypeId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetGearTypeById(int gearTypeId, CancellationToken token = default)
        {
            var find = await _gearTypeService.GetGearTypeById(gearTypeId, token);
            return PartialView("_CreateGearType", find);
        }
        #endregion


    }
}
