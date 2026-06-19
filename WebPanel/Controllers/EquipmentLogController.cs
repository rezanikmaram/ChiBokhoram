using Common.enumeration.Log;
using Entities.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.PublicService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class EquipmentLogController : Controller
    {
        private readonly IEquipmentLogService equipmentLogService;
        private readonly ILogger _logger;

        public EquipmentLogController(IEquipmentLogService equipmentLogService, ILogger<EquipmentLogController> logger)
        {
            _logger = logger;
            this.equipmentLogService = equipmentLogService;
        }

        public async Task<IActionResult> Index(int? p = null, EquipmentStatusLogFilterDto filter = null, CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var result = await equipmentLogService.GetEquipmentStatusLog(filter, pagenumaber, token);
            return View(result);
        }

        public async Task<IActionResult> EquipmentLogDetail(long equipmentLogId, CancellationToken token = default)
        {
            // var dd = await seoService.GetSeoDto(seoId, token);
            var dto = await equipmentLogService.GetEquipmentStatusLogDetail(equipmentLogId, token);
            return View(dto);
        }

        public async Task<IActionResult> DashboardEquipmentStatus(DashboardEquipmentFilterDto filter, CancellationToken token = default)
        {
            var dto = await equipmentLogService.GetDashboardData(filter, token);
            return View(dto);
        }


    }
}
