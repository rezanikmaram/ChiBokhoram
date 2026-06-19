using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.DTOs.Public;
using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }
        public IDashboardAdminService DashboardAdminService { get; }

        public DashboardController(ICurrentUserService<UserPanelInfoDto> currentUserService, IDashboardAdminService dashboardAdminService)
        {
            CurrentUserService = currentUserService;
            DashboardAdminService = dashboardAdminService;
        }

        [Authorize(Roles = "admin, adminOnePort, DashboardAdmin")]
        public async Task<IActionResult> Index(DashboardAdminFilterDto filter, CancellationToken token = default)
        {
            var data = await DashboardAdminService.GetDashboardAdminPage(filter, token);
            return View(data);
        }

        [Authorize(Roles = "admin, adminOnePort, DashboardAdmin")]
        public async Task<IActionResult> MonthlyReport(DashboardAdminFilterDto filter, CancellationToken token = default)
        {
            // Ensure default 1-month range on initial load if not provided
            if (filter == null)
                filter = new DashboardAdminFilterDto();

            if (filter.FilterDate == null)
                filter.FilterDate = new FilterDateDto();

            if (string.IsNullOrWhiteSpace(filter.FilterDate.FromDate) || string.IsNullOrWhiteSpace(filter.FilterDate.ToDate))
            {
                var to = DateTimeOffset.Now;
                var from = to.AddMonths(-1);
                filter.FilterDate.FromDate = from.ToShortPersianDateString();
                filter.FilterDate.ToDate = to.ToShortPersianDateString();
            }
            var report = await DashboardAdminService.GetMonthlyDashboardReport(filter, token);
            return View(report);
        }

        [Authorize(Roles = "admin, adminOnePort, DashboardAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetMonthlyReportData([FromQuery] DashboardAdminFilterDto filter, CancellationToken token = default)
        {
            // Fallback to default 1-month range if client didn't send dates
            if (filter == null)
                filter = new DashboardAdminFilterDto();

            if (filter.FilterDate == null)
                filter.FilterDate = new FilterDateDto();

            if (string.IsNullOrWhiteSpace(filter.FilterDate.FromDate) || string.IsNullOrWhiteSpace(filter.FilterDate.ToDate))
            {
                var to = DateTimeOffset.Now;
                var from = to.AddMonths(-1);
                filter.FilterDate.FromDate = from.ToShortPersianDateString();
                filter.FilterDate.ToDate = to.ToShortPersianDateString();
            }
            var report = await DashboardAdminService.GetMonthlyDashboardReport(filter, token);
            return Json(report);
        }

        [Authorize(Roles = "admin, adminOnePort, DashboardAdmin")]
        public async Task<IActionResult> Monitoring(CancellationToken token = default)
        {
            var model = await DashboardAdminService.GetMonitoringSnapshot(token);
            return View(model);
        }
    }
}
