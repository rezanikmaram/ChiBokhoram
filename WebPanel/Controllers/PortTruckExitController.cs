using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Concrete;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class PortTruckExitController : Controller
    {
        public PortTruckExitController(IPortTruckExitService portTruckExitService,
			ITaliService taliService
            )
        {
            PortTruckExitService = portTruckExitService;
			TaliService = taliService;
		}

        public IPortTruckExitService PortTruckExitService { get; }
		public ITaliService TaliService { get; }

		public async Task<IActionResult> Index(PortTruckExitFilterDto filter = null, CancellationToken token = default)
        {
            var result = await PortTruckExitService.GetPage(filter, token);

            return View(result);
        }

        public async Task<IActionResult> CreateReportTruckExitPermit(PortTruckExitFilterDto filter, CancellationToken token = default)
        {
            try
            {
                var fileDto = await PortTruckExitService.CreateReportTruckExitPermit(filter, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", $"{fileDto.FileInfo.FileTitle}{fileDto.FileInfo.FileExtention}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> ArchivePage(PortTruckExitPermitFilterDto filter = null, CancellationToken token = default)
        {
            var result = await PortTruckExitService.GetArchive(filter, token);

            return View(result);
        }

		public async Task<IActionResult> TaliWithoutWeight(TaliWithoutWeightFilterDto filter = null, CancellationToken token = default)
		{
			var result = await TaliService.GetTaliWithoutWeight(filter, token);

			return View(result);
		}

		[HttpPost]
		public async Task<IActionResult> TaliWithoutWeightExcel(
			TaliWithoutWeightFilterDto filter = null,
			CancellationToken token = default
		)
		{
			var fileDto = await TaliService.GetTaliWithoutWeightExcel(filter, token);

			if (fileDto == null)
			{
				return Ok();
			}

			return File(fileDto, "application/vnd.ms-excel", $"TaliWithoutWeight.xlsx");
		}
	}
}
