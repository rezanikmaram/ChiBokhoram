using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using Entities.DTOs.Web.Hirman.VoyageReport;
using DNTPersianUtils.Core;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd, FinanceUser")]
    public class VoyageReportController : Controller
    {
        private readonly IVoyageReportService _voyageReportService;

        public VoyageReportController(IVoyageReportService voyageReportService)
        {
            _voyageReportService = voyageReportService ?? throw new ArgumentNullException(nameof(voyageReportService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(VoyageReportFilterDto filter = null, CancellationToken token = default)
        {
            if (filter == null)
            {
                filter = new VoyageReportFilterDto
                {
                    DateFromFa = DateTimeOffset.Now.AddMonths(-1).ToShortPersianDateString(),
                    DateToFa = DateTimeOffset.Now.ToShortPersianDateString()
                };
            }
            else
            {
                // Set default dates if not provided
                if (string.IsNullOrEmpty(filter.DateFromFa) || string.IsNullOrEmpty(filter.DateToFa))
                {
                    filter.DateFromFa = DateTimeOffset.Now.AddMonths(-1).ToShortPersianDateString();
                    filter.DateToFa = DateTimeOffset.Now.ToShortPersianDateString();
                }
            }

            var data = await _voyageReportService.GetVoyageReportList(filter, token);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> VoyageDetail(Guid voyageId, CancellationToken token = default)
        {
            var data = await _voyageReportService.GetVoyageDetail(voyageId, token);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> SeoDetail(Guid seoId, CancellationToken token = default)
        {
            var data = await _voyageReportService.GetSeoDetail(seoId, token);
            if (data == null)
                return RedirectToAction(nameof(Index));

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> ExportVoyageDetailExcel(Guid voyageId, CancellationToken token = default)
        {
            var data = await _voyageReportService.ExportVoyageDetailExcel(voyageId, token);
            var fileName = $"VoyageDetail_{voyageId}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportSeoDetailExcel(Guid seoId, CancellationToken token = default)
        {
            var data = await _voyageReportService.ExportSeoDetailExcel(seoId, token);
            var fileName = $"SeoDetail_{seoId}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ExportVoyageReportExcel(VoyageReportFilterDto filter, CancellationToken token = default)
        {
            var data = await _voyageReportService.ExportVoyageReportExcel(filter, token);
            if (data == null)
                return Ok();

            var fileName = $"VoyageReport_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }
}
