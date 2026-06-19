using Entities.DTOs;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class WaterfrontController : Controller
    {
        private readonly IWarehouseService warehouseService;
        private readonly IExcelToolsService excelTools;

        public IWaterfrontService WaterfrontService { get; }

        public WaterfrontController(IWaterfrontService waterfrontService, IWarehouseService warehouseService, IExcelToolsService excelTools)
        {
            WaterfrontService = waterfrontService ?? throw new ArgumentNullException(nameof(waterfrontService));
            this.warehouseService = warehouseService;
            this.excelTools = excelTools;
        }
        public async Task<IActionResult> Index(CancellationToken token = default)
        {
            var result = await WaterfrontService.GetWaterfrontList(token);
            return View(result);
        }
         
        [HttpPost]
        public async Task<IActionResult> CreateWaterfront(CreateWaterfrontDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty) ModelState.Remove("Id");
            if (ModelState.IsValid)
                await WaterfrontService.AddOrUpdate(dto, token);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetCreateWaterfrontPageById(Guid waterfrontId,
        CancellationToken token = default)
        {
            var find = await WaterfrontService.GetWaterfrontById(waterfrontId, token);

            return PartialView("_CreateWaterfront", find);
        }

        #region Distance
        [HttpPost]
        public async Task<IActionResult> Distanc(DistanceWaterfrontToWarehouseDto createDto,
            CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty) ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                await WaterfrontService.SetDistanceWaterfrontToWarehouse(createDto, token);

            }

            return RedirectToAction(nameof(GetDistance));
        }

        public async Task<IActionResult> GetDistance(CancellationToken token = default)
        {
            var result = await WaterfrontService.GetDistanceWaterfrontToWarehouse(token);

            var warehouse = await warehouseService.GetWarehousDropDownByActivePort(token);
            var waterfronf = await WaterfrontService.GetWaterfrontDropDownByActivePort(token);
            result.CreateDistancePage.Waterfronts = waterfronf.ToSelectList();
            result.CreateDistancePage.Warehouses = warehouse.ToSelectList();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetDistancePageById(Guid distanceId,
        CancellationToken token = default)
        {
            var find = await WaterfrontService.GetCreateDistancePage(distanceId, token);

            var warehouse = await warehouseService.GetWarehousDropDownByActivePort(token);
            var waterfronf = await WaterfrontService.GetWaterfrontDropDownByActivePort(token);
            find.Waterfronts = waterfronf.ToSelectList();
            find.Warehouses = warehouse.ToSelectList();

            return PartialView("_CreateDistanc", find);
        }

        public async Task<IActionResult> GetExcellDistance(CancellationToken token = default)
        {
            var excells = await WaterfrontService.GetDistanceWaterfrontToWarehouse(token);
            var result = await excelTools.GetExcelDistanceWaterfrontToWarehouse(excells.DistanceList);
            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DistanceWaterfrontToWarehouse.xlsx");
        }
        #endregion




    }
}
