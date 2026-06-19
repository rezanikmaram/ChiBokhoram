
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    //دستگاه های FPN
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearProduceController : Controller
    {
        public GearProduceController(IGearProduceService gearProduceService)
        {
            _gearProduceService = gearProduceService;
        }

        private IGearProduceService _gearProduceService { get; }


        #region GearProduce
        public async Task<IActionResult> Index(CancellationToken token = default, int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await _gearProduceService.GetGearProduces(pagenumaber, token);
            return View(result);
        }
        public async Task<IActionResult> CreateGearProduce(int gearProduceId, CancellationToken token = default)
        {
            if (gearProduceId > 0)
            {
                var dto = await _gearProduceService.GetGearProduceById(gearProduceId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGearProduce(CreateGearProduceDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _gearProduceService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteGearProduce(int gearProduceId, CancellationToken token = default)
        {
            TempData["msg"] = await _gearProduceService.DeleteGearProduce(gearProduceId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetGearProduceById(int gearProduceId, CancellationToken token = default)
        {
            var find = await _gearProduceService.GetGearProduceById(gearProduceId, token);
            return PartialView("_CreateGearProduce", find);
        }
        #endregion


    }
}
