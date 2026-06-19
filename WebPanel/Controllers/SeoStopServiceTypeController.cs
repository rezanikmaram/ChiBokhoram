using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd, FinanceUser")]
    public class SeoStopServiceTypeController : Controller
    {
        public ISeoStopServiceLogService SeoStopServiceLogService { get; }

        public SeoStopServiceTypeController(ISeoStopServiceLogService seoStopServiceLogService)
        {
            SeoStopServiceLogService = seoStopServiceLogService;
        }

        public async Task<IActionResult> Index(SeoStopServiceTypeFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await SeoStopServiceLogService.GetStopTypeList(filter, p ?? 1, token);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(SeoStopServiceTypeFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await SeoStopServiceLogService.GetStopTypeList(filter, p ?? 1, token);
            return PartialView("_SeoStopServiceTypeTable", result);
        }

        [HttpGet]
        public async Task<IActionResult> GetStopTypeCount(CancellationToken token = default)
        {
            var count = await SeoStopServiceLogService.GetStopTypeCount(token);
            return Json(new { count });
        }

        [HttpPost]
        public async Task<IActionResult> ActivationStopType(int stopTypeId, CancellationToken token = default)
        {
            await SeoStopServiceLogService.ActivationStopType(stopTypeId, token);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CreateStopTypeModal(int? stopTypeId, CancellationToken token = default)
        {
            if (stopTypeId.HasValue)
            {
                var dto = await SeoStopServiceLogService.GetSeoStopServiceTypeById(stopTypeId.Value, token);
                return PartialView("_CreateSeoStopServiceType", dto);
            }
            return PartialView("_CreateSeoStopServiceType", new SeoStopServiceTypeDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateSeoStopServiceType(SeoStopServiceTypeDto stopType, CancellationToken token = default)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateSeoStopServiceType", stopType);

            await SeoStopServiceLogService.AddOrUpdateSeoStopServiceType(stopType, token);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStopType(int stopTypeId, CancellationToken token = default)
        {
            await SeoStopServiceLogService.DeleteStopType(stopTypeId, token);
            return Json(new { success = true });
        }
    }
}
