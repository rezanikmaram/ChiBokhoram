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
using Stimulsoft.Base.Excel;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports, FinanceUser")]
    public class VoyageFactorController : Controller
    {
        public ISeoFactorService SeoFactorService { get; }
        public IVoyageFactorService VoyageFactorService { get; }
        public IVoyageFactorReportService VoyageFactorReportService { get; }
        public IGeneralCargoProductTariffCalculatorService GeneralCargoProductTariffCalculatorService { get; }
        public IFactorPublicService factorPublicService { get; }

        public VoyageFactorController(
            ISeoFactorService seoFactorService,
            IVoyageFactorService voyageFactorService,
            IVoyageFactorReportService voyageFactorReportService,
            IFactorPublicService factorPublicService
        )
        {
            SeoFactorService = seoFactorService;
            VoyageFactorService = voyageFactorService;
            VoyageFactorReportService = voyageFactorReportService;
            this.factorPublicService = factorPublicService;
        }

        #region   صورتحساب های کشتی
        public async Task<IActionResult> List(int? p, VoyageFactorFilterDto filter, CancellationToken token = default)
        {
            var data = await VoyageFactorService.GetList(p ?? 1, filter, token);

            return View(data);
        }

        public async Task<IActionResult> CreateVoyageFactor(CreateVoyageFactorDto createDto, CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                createDto.FactorType = FactorType.Voyage;
                var id = await VoyageFactorService.AddOrUpdateVoyageFactor(createDto, token);
                return RedirectToAction(nameof(Details), new { factorId = id });
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(List), new { voyageId = createDto.VoyageId });
        }

        [HttpPost]
        public async Task<IActionResult> GetVoyageFactorById(Guid factorId, CancellationToken token = default)
        {
            var find = await VoyageFactorService.GetById(factorId, token);
            if (find == null)
                throw new Exception("موردی یافت نشد");

            return Json(find);
        }

        public async Task<IActionResult> DeleteFactor(Guid factorId, Guid voyageId, CancellationToken token = default)
        {
            await VoyageFactorService.DeleteFactor(factorId, token);
            return RedirectToAction(nameof(List), new { voyageId });
        }

        public async Task<IActionResult> Details(Guid factorId, CancellationToken token = default)
        {
            var data = await VoyageFactorService.Details(factorId, token);

            return View(data);
        }
        #endregion

        #region Factor
        public async Task<IActionResult> Factor(Guid factorId, CancellationToken token = default)
        {
            var data = await VoyageFactorService.GetFactorPage(factorId, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EndFactor(Guid factorId, CancellationToken token = default)
        {
            try
            {
                await VoyageFactorService.EndFactor(factorId, token);
            }
            catch (Exception ex)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));
            }

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
                var fileDto = await this.VoyageFactorService.GetFactorByType(factorId, type, token);

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
                var fileDto = await VoyageFactorReportService.GetPreviewFactor(filter, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileTitleWithExtention);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));

                return RedirectToAction(nameof(Factor), new { factorId = filter.SeoFactorId });
            }
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> CreateFactorService(CreateVoyageFactorServiceDto createService, CancellationToken token = default)
        {
            if (createService?.Id == Guid.Empty)
                ModelState.Remove("createService.Id");

            if (createService.Type != SeoFactorType.Anbardary)
            {
                ModelState.Remove("createService.WarehousingStartFa");
                ModelState.Remove("createService.WarehousingEndFa");
            }

            var result = new VoyageFactorServicePageDto { SeoFactorId = createService.SeoFactorId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await VoyageFactorService.AddOrEditService(createService, token);
            }

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> DeleteFactorService(Guid VoyageFactorServiceId, CancellationToken token = default)
        {
            var result = await VoyageFactorService.DeleteService(VoyageFactorServiceId, token);

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> DeleteAllFactorServices(Guid seoFactorId, CancellationToken token = default)
        {
            var result = await VoyageFactorService.DeleteAllServices(seoFactorId, token);

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> GetServiceById(Guid seoFactorServiceId, CancellationToken token = default)
        {
            var find = await VoyageFactorService.GetServiceById(seoFactorServiceId, token);

            if (find == null)
            {
                find = new SeoFactorServiceDto();
            }

            return Json(find);
        }

        public async Task<IActionResult> GetModalAddService(SeoFactorServiceFilterDto filter, CancellationToken token = default)
        {
            if (filter.SeoFactorType == SeoFactorType.Loading)
                filter.SeoFactorType = SeoFactorType.AllocationOfEquipment;

            var data = await VoyageFactorService.GetAddServiceData(filter, token);

            switch (filter.SeoFactorType)
            {
                case SeoFactorType.AllocationOfEquipment:
                    return PartialView("Partials/_AllocationEquOptionsPartial", data);
                case SeoFactorType.AncillaryServices:
                    return PartialView("Partials/_AncillaryOptionsPartial", data);
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
            await VoyageFactorService.SaveCreditorPrice(creditorPriceDto, token);

            return RedirectToAction(nameof(Factor), new { factorId = creditorPriceDto.SeoFactorId });
        }
        #endregion

        #region DocFile
        public async Task<IActionResult> DocFiles(Guid factorId, CancellationToken token = default)
        {
            var model = await this.VoyageFactorService.GetFactorDocPage(factorId, token);
            return View(model);
        }

        public async Task<IActionResult> UploadFactorDoc(CreateVoyageFactorFileDto CreateDto, CancellationToken token = default)
        {
            await this.VoyageFactorService.AddFactorDoc(CreateDto, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = CreateDto.SeoFactorId });
        }

        [HttpPost]
        public async Task<IActionResult> GetVoyageFactorDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this.VoyageFactorService.GetFileById(docId, token);

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
        public async Task<IActionResult> RemoveVoyageFactorDocFileById(long docId, Guid factorId, CancellationToken token = default)
        {
            await this.VoyageFactorService.RemoveFactorDoc(docId, factorId, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = factorId });
        }
        #endregion
    }
}
