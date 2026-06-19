using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.enumeration;
using DNTPersianUtils.Core;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
    public class ReportController : Controller
    {
        public ReportController(
            ISeoService seoService,
            ISeoBarshomarService seoBarshomarService,
            IReportService reportService,
            ISeoReportService seoReportService,
            IShipInLangargahService shipInLangargahService,
            ILaborWorkingGroupService laborWorkingGroupService,
            ICurrentUserService<UserPanelInfoDto> currentUserService
        )
        {
            SeoService = seoService;
            SeoBarshomarService = seoBarshomarService;
            ReportService = reportService;
            SeoReportService = seoReportService;
            ShipInLangargahService = shipInLangargahService;
            LaborWorkingGroupService = laborWorkingGroupService;
            CurrentUserService = currentUserService;
        }

        public ISeoService SeoService { get; }
        public ISeoBarshomarService SeoBarshomarService { get; }
        public IReportService ReportService { get; }
        public ISeoReportService SeoReportService { get; }
        public IShipInLangargahService ShipInLangargahService { get; }
        public ILaborWorkingGroupService LaborWorkingGroupService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }

        public IActionResult SeoReport()
        {
            return View();
        }

        //SeoViewTemplate.xlsx
        public async Task<IActionResult> GetSeoViewExcelFile(Guid seoId, CancellationToken token = default)
        {
            var res = await ReportService.GetSeoDetailExcellFile(seoId, token);
            var code = await SeoService.GetSeoCode(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{code}.xlsx");
        }

        //SeoTemplate.xlsx
        public async Task<IActionResult> GetSeoExcellFile(string fromdate, string todate, CancellationToken token = default)
        {
            var res = await ReportService.GetSeoExcellFile(fromdate, todate, token);

            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"seofrom{fromdate}.xlsx");
        }

        //گزارش کامیون های درون سئو
        //SeoTruck.xlsx
        public async Task<IActionResult> GetSeoTruckExcelFile(Guid seoId, CancellationToken token = default)
        {
            var seo = await SeoService.GetSeoCode(seoId, token);
            var res = await ReportService.GetSeoTruckExcelFile(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Trucks-{seo}.xlsx");
        }

        //گزارش راننده های درون سئو
        //SeoDriver.xlsx
        public async Task<IActionResult> GetSeoDriverExcelFile(Guid seoId, CancellationToken token = default)
        {
            var seo = await SeoService.GetSeoCode(seoId, token);
            var res = await ReportService.GetSeoDriverExcelFile(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"drivers-{seo}.xlsx");
        }

        //گزارش سرویس های بارشمار
        //SeoCounterServiceTemplate.xlsx
        public async Task<IActionResult> CounterService(Guid seoId, CancellationToken token = default)
        {
            var seo = await SeoService.GetSeoCode(seoId, token);
            var res = await ReportService.GetSeoCounterServiceExcel(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CounterService-{seo}.xlsx");
        }

        //گزارش سرویس های انباردار
        //SeoWarehouseServiceTemplate.xlsx
        public async Task<IActionResult> WarehouseService(Guid seoId, CancellationToken token = default)
        {
            var seo = await SeoService.GetSeoCode(seoId, token);
            var res = await ReportService.GetSeoWarehouseServiceExcel(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"WarehouseService-{seo}.xlsx");
        }

        //گزارش سرویس های تکمیل شده
        //SeoCompletedServiceTemplate.xlsx
        public async Task<IActionResult> CompletedService(Guid seoId, CancellationToken token = default)
        {
            var seo = await SeoService.GetSeoCode(seoId, token);
            var res = await ReportService.GetSeoCompletedeServiceExcel(seoId, token);
            return File(res, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CompletedService-{seo}.xlsx");
        }

        public async Task<IActionResult> GetEquipmentUsageExcel(EquipmentUsageFilterDto filter = null, CancellationToken token = default)
        {
            var res = await SeoReportService.GetEquipmentUsageExcel(filter, token);
            if (res == null || res?.FileStream?.Length == 0)
                return Ok();

            return File(res.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", res.FileTitleWithExtention);
        }

        public async Task<IActionResult> GetReportShipInLangargah(SeoShipInLangargahReportInputDto reportInput, CancellationToken token = default)
        {
            var fileDto = await ShipInLangargahService.GetShipInLangargahReport(null, token, reportInput);

            if (fileDto == null)
            {
                return Ok();
            }

            return File(fileDto.FileStream, "application/octet-stream", fileDto.FileTitleWithExtention);
        }

        #region گزارش گروه کارگری
        public async Task<IActionResult> LaborWorkingGroupReport(LaborWorkingGroupReportFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterLaborWorkingGroupReport(filter);

            var data = await LaborWorkingGroupService.GetLaborWorkingGroupReport(filter, token);

            return View(data);
        }

        private async Task ConfigFilterLaborWorkingGroupReport(LaborWorkingGroupReportFilterDto filter)
        {
            var user = await CurrentUserService.GetCurrentUser();
            var now = DateTimeOffset.Now.ToShortPersianDateString();
            var dayStart = DateTimeOffset.Now.AddDays(-30).ToShortPersianDateString();

            filter ??= new LaborWorkingGroupReportFilterDto { Page = 1 };
            if (filter.Page <= 0)
                filter.Page = 1;
            filter.FilterDate ??= new FilterDateDto { FromDate = dayStart, ToDate = now };
            filter.PortId = user.PortId;
        }

        [HttpPost]
        public async Task<IActionResult> GetLaborWorkingGroupReportFile(LaborWorkingGroupReportFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterLaborWorkingGroupReport(filter);

            var stream = await LaborWorkingGroupService.GetLaborWorkingGroupReportFile(filter, token);
            if (stream == null)
                return Ok();

            var fileName = $"LaborWorkingGroup_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        #endregion
    }
}
