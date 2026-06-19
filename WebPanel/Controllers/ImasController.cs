using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin")]
    public class ImasController : Controller
    {
        private readonly ILogger<ImasController> _logger;
        private readonly IImasService imasService;
        private readonly IImasVeseelService imasVeseelService;

		public IImasVeseelReportService ImasVeseelReportService { get; }

		public ImasController(ILogger<ImasController> logger,
            IImasService imasService,
            IImasVeseelService imasVeseelService,
			IImasVeseelReportService imasVeseelReportService)
        {
            _logger = logger;
            this.imasService = imasService;
            this.imasVeseelService = imasVeseelService;
			ImasVeseelReportService = imasVeseelReportService;
		}

        public async Task<IActionResult> VesselList(
            ImasVesselFilterDto filter,
            int? p = null,
            CancellationToken token = default)
        {
            var result = await imasVeseelService.GetVessel(filter, p ?? 1, token);
            return View(result);
        }

        public async Task<IActionResult> UpdateVessel(CancellationToken token = default)
        {
            await imasService.UpdateVesselInfoFromImass(token);
            return RedirectToAction(nameof(VesselList));
        }


		[HttpPost]
		public async Task<IActionResult> GetReportList(ImasVesselFilterDto filter = null,
			ReportType reportType = ReportType.Excel,
			CancellationToken token = default)
		{
			try
			{
				var fileDto = await ImasVeseelReportService.GetImasVeseelReport(filter, token, reportType);

				if (fileDto == null)
				{
					return Ok();
				}

				return File(fileDto.FileStream, "application/octet-stream", fileDto.FileTitleWithExtention);
			}
			catch (Exception ex)
			{
				ErrorLog.SaveError(ex);
				throw;
			}
		}
	}
}
