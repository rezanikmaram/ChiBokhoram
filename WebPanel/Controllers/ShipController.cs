using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
    public class ShipController : Controller
    {
        public ShipController(IShipService shipService)
        {
            ShipService = shipService ?? throw new ArgumentNullException(nameof(shipService));
        }

        public IShipService ShipService { get; }

        public async Task<IActionResult> Index(ShipFilterDto filter, CancellationToken token = default, int? p = null)
        {
            var result = await ShipService.GetShips(filter, p ?? 1, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateShip(CreateShipDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty)
                ModelState.Remove("Id");
            if (ModelState.IsValid)
                await ShipService.AddOrUpdate(dto, token);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid shipId, CancellationToken token = default)
        {
            TempData["msg"] = await ShipService.Delete(shipId, token);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetCreateShipPageById(Guid shipId, CancellationToken token = default)
        {
            var find = await ShipService.GetShipById(shipId, token);

            return PartialView("_CreateShip", find);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ImoNumberExist(CreateShipDto createShipDto, CancellationToken token = default)
        {
            var result = await ShipService.ImoNumberExist(createShipDto, token);
            if (result)
                return Json("شناسه کشتی تکراری است");
            else
                return Json(true);
        }
    }
}
