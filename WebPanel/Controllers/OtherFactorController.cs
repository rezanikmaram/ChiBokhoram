using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Entities.Entities.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports, FinanceUser")]
    public class OtherFactorController : Controller
    {
        public ISeoFactorService SeoFactorService { get; }
        public IOtherFactorService OtherFactorService { get; }
        public IOtherFactorReportService OtherFactorReportService { get; }
        public IGeneralCargoProductTariffCalculatorService GeneralCargoProductTariffCalculatorService { get; }
        public IFactorPublicService factorPublicService { get; }

        public OtherFactorController(
            ISeoFactorService seoFactorService,
            IOtherFactorService otherFactorService,
            IOtherFactorReportService otherFactorReportService,
            IFactorPublicService factorPublicService
        )
        {
            SeoFactorService = seoFactorService;
            OtherFactorService = otherFactorService;
            OtherFactorReportService = otherFactorReportService;
            this.factorPublicService = factorPublicService;
        }

        #region   صورتحساب سایر درآمدها
        public async Task<IActionResult> List(int? p, OtherFactorFilterDto filter, CancellationToken token = default)
        {
            var data = await OtherFactorService.GetList(p ?? 1, filter, token);

            return View(data);
        }

        public async Task<IActionResult> CreateOtherFactor(CreateOtherFactorDto createDto, CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                createDto.FactorType = FactorType.Other;
                var id = await OtherFactorService.AddOrUpdateOtherFactor(createDto, token);
                return RedirectToAction(nameof(Details), new { factorId = id });
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> GetOtherFactorById(Guid factorId, CancellationToken token = default)
        {
            var find = await OtherFactorService.GetById(factorId, token);
            if (find == null)
                throw new Exception("موردی یافت نشد");

            return Json(find);
        }

        public async Task<IActionResult> DeleteFactor(Guid factorId, CancellationToken token = default)
        {
            await OtherFactorService.DeleteFactor(factorId, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Details(Guid factorId, CancellationToken token = default)
        {
            var data = await OtherFactorService.Details(factorId, token);

            return View(data);
        }
        #endregion

        #region Factor
        public async Task<IActionResult> Factor(Guid factorId, CancellationToken token = default)
        {
            var data = await OtherFactorService.GetFactorPage(factorId, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EndFactor(Guid factorId, CancellationToken token = default)
        {
            await OtherFactorService.EndFactor(factorId, token);

            return RedirectToAction(nameof(Factor), new { factorId });
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> BackToPendingState(Guid factorId, CancellationToken token = default)
        {
            await SeoFactorService.BackToPendingState(factorId, token);

            return RedirectToAction(nameof(Factor), new { factorId });
        }

        public async Task<IActionResult> GetFactorFile(Guid factorId, AllocationEquFileType type, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this.OtherFactorService.GetFactorByType(factorId, type, token);

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

        public async Task<IActionResult> GetPreviewFactor(SeoFactorPreviewReportFilterDto filter, CancellationToken token = default)
        {
            try
            {
                var fileDto = await OtherFactorReportService.GetPreviewFactor(filter, token);

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

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> CreateFactorService(CreateOtherFactorServiceDto createService, CancellationToken token = default)
        {
            if (createService?.Id == Guid.Empty)
                ModelState.Remove("Id");

            var result = new OtherFactorServicePageDto { SeoFactorId = createService.SeoFactorId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await OtherFactorService.AddOrEditService(createService, token);
            }

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> DeleteFactorService(Guid otherFactorServiceId, CancellationToken token = default)
        {
            var result = await OtherFactorService.DeleteService(otherFactorServiceId, token);

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> GetServiceById(Guid seoFactorServiceId, CancellationToken token = default)
        {
            var find = await OtherFactorService.GetServiceById(seoFactorServiceId, token);

            if (find == null)
            {
                find = new SeoFactorServiceDto();
            }

            return Json(find);
        }

        public async Task<IActionResult> GetModalAddService(SeoFactorServiceFilterDto filter, CancellationToken token = default)
        {
            if (filter.SeoFactorType == SeoFactorType.Loading)
                filter.SeoFactorType = SeoFactorType.Cleaning;

            var data = await OtherFactorService.GetAddServiceData(filter, token);

            switch (filter.SeoFactorType)
            {
                default:
                    return PartialView("Partials/_CommonOptionsPartial", data);
            }
        }

        #endregion


        #region بستانکاری صاحب کالا
        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> SaveCreditorPrice(CreditorPriceSeoFactorDto creditorPriceDto, CancellationToken token = default)
        {
            await OtherFactorService.SaveCreditorPrice(creditorPriceDto, token);

            return RedirectToAction(nameof(Factor), new { factorId = creditorPriceDto.SeoFactorId });
        }
        #endregion

        #region DocFile
        public async Task<IActionResult> DocFiles(Guid factorId, CancellationToken token = default)
        {
            var model = await this.OtherFactorService.GetFactorDocPage(factorId, token);
            return View(model);
        }

        public async Task<IActionResult> UploadFactorDoc(CreateOtherFactorFileDto CreateDto, CancellationToken token = default)
        {
            await this.OtherFactorService.AddFactorDoc(CreateDto, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = CreateDto.SeoFactorId });
        }

        [HttpPost]
        public async Task<IActionResult> GetOtherFactorDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this.OtherFactorService.GetFileById(docId, token);

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

        [HttpPost]
        public async Task<IActionResult> RemoveOtherFactorDocFileById(long docId, Guid factorId, CancellationToken token = default)
        {
            await this.OtherFactorService.RemoveFactorDoc(docId, factorId, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = factorId });
        }
        #endregion
    }
}
