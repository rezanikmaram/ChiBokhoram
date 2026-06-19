using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.CustomAttribute;
using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class SeoDocumentController : Controller
    {
        public ISeoInsuranceService SeoInsuranceService { get; }

        private ISeoCustomsService SeoCustomsService { get; }

        public SeoDocumentController(ISeoInsuranceService seoInsuranceService, ISeoCustomsService seoCustomsService)
        {
            SeoInsuranceService = seoInsuranceService;
            SeoCustomsService = seoCustomsService;
        }

        public async Task<IActionResult> InsuranceList(Guid seoId, CancellationToken token = default)
        {
            var dto = await SeoInsuranceService.GetSeoInsurancePage(seoId, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> GetCreateSeoInsuranceDtoById(long seoInsuranceId, CancellationToken token, Guid? seoId = null)
        {
            CreateSeoInsuranceDto dto = null;

            if (seoInsuranceId != 0)
                dto = await SeoInsuranceService.GetCreateSeoInsuranceDto(seoInsuranceId, token);
            else
                dto = new CreateSeoInsuranceDto { SeoId = seoId.Value };

            return PartialView("_CreateInsurance", dto);
        }

        public async Task<IActionResult> CreateSeoInsurance(CreateSeoInsuranceDto insuranceDto, CancellationToken token = default)
        {
            await SeoInsuranceService.AddOrUpdateSeoInsurance(insuranceDto, token);

            return RedirectToAction(nameof(InsuranceList), new { seoId = insuranceDto.SeoId });
        }

        public async Task<IActionResult> DeleteSeoInsurance(long seoInsuranceId, CancellationToken token)
        {
            var seoId = await SeoInsuranceService.DeleteSeoInsurance(seoInsuranceId, token);
            return RedirectToAction(nameof(InsuranceList), new { seoId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoInsuranceDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await SeoInsuranceService.GetFileById(docId, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileInfo?.FileTitle);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        #region seoCustoms
        public async Task<IActionResult> CustomsList(Guid seoId, CancellationToken token = default)
        {
            var dto = await SeoCustomsService.GetSeoCustomsPage(seoId, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> GetCreateSeoCustomsDtoById(Guid seoCustomsId, CancellationToken token, Guid? seoId = null)
        {
            CreateSeoCustomsDto dto = null;

            if (seoCustomsId != Guid.Empty)
                dto = await SeoCustomsService.GetCreateSeoCustomsDto(seoCustomsId, token);
            else
                dto = new CreateSeoCustomsDto { SeoId = seoId.Value };

            return PartialView("_CreateCustoms", dto);
        }

        public async Task<IActionResult> CreateSeoCustoms(CreateSeoCustomsDto seoCustomsDto, CancellationToken token = default)
        {
            await SeoCustomsService.AddOrUpdateSeoCustoms(seoCustomsDto, token);

            return RedirectToAction(nameof(CustomsList), new { seoId = seoCustomsDto.SeoId });
        }

        public async Task<IActionResult> DeleteSeoCustoms(Guid seoCustomsId, CancellationToken token)
        {
            var seoId = await SeoCustomsService.DeleteSeoCustoms(seoCustomsId, token);
            return RedirectToAction(nameof(CustomsList), new { seoId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoCustomsDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await SeoCustomsService.GetFileById(docId, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileInfo?.FileTitle);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        #endregion
    }
}
