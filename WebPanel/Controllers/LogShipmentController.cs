using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class LogShipmentController : Controller
    {
        private readonly ILogShipmentService _logShipmentService;
        private readonly ILogger<LogShipmentController> _logger;

        public LogShipmentController(ILogShipmentService logShipmentService, ILogger<LogShipmentController> logger)
        {
            _logShipmentService = logShipmentService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? p = null, LogShipmentFilterDto filter = null, CancellationToken token = default)
        {
            var pageNumber = p ?? 1;
            var result = await _logShipmentService.GetLogShipments(filter, pageNumber, token);
            return View(result);
        }

        public async Task<IActionResult> Detail(long id, CancellationToken token = default)
        {
            var dto = await _logShipmentService.GetLogShipment(id, token);
            if (dto == null)
            {
                _logger.LogWarning("LogShipment with id {Id} not found", id);
                return NotFound();
            }

            return View(dto);
        }
    }
}
