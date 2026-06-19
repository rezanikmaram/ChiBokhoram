using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearWorkSessionController : Controller
    {
        private readonly IGearWorkSessionService _GearWorkSessionService;

        //private readonly IGearWorkSessionReportService _GearWorkSessionReportService;

        public GearWorkSessionController(
            IGearWorkSessionService GearWorkSessionService
        // IGearStateService gearStateService,
        // IGearWorkSessionReportService GearWorkSessionReportService
        )
        {
            _GearWorkSessionService = GearWorkSessionService;

            //_GearWorkSessionReportService = GearWorkSessionReportService;
        }

        #region GearWorkSession
        public async Task<IActionResult> Index(GearWorkSessionWebFilterDto filter, CancellationToken token = default)
        {
            var result = await _GearWorkSessionService.GetGearWorkSessionPage(filter, token);
            return View(result);
        }

        //[HttpPost]
        //public async Task<IActionResult> GetGearStateReportList(
        //    GearWorkSessionFilterDto filter = null,
        //    ReportType reportType = ReportType.Excel,
        //    CancellationToken token = default
        //)
        //{
        //    try
        //    {
        //        var fileDto = await _GearWorkSessionReportService.GetGearStateReportList(filter, token, reportType);

        //        if (fileDto == null)
        //        {
        //            return Ok();
        //        }

        //        return File(fileDto.FileStream, "application/octet-stream", fileDto.FileTitleWithExtention);
        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorLog.SaveError(ex);
        //        throw;
        //    }
        //}

        #endregion
    }
}
