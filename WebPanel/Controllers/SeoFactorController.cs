using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.Exceptions;
using DNTPersianUtils.Core;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports, FinanceUser")]
    public class SeoFactorController : Controller
    {
        public ISeoFactorService SeoFactorService { get; }
        public ISeoFactorItemService SeoFactorItemService { get; }
        public ISeoFactorReportService SeoFactorReportService { get; }
        public IGeneralCargoProductTariffCalculatorService GeneralCargoProductTariffCalculatorService { get; }
        public IFactorPublicService SeoFactorPublicService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }
        public ISeoFactorDeleteService SeoFactorDeleteService { get; }

        public SeoFactorController(
            ISeoFactorService seoFactorService,
            ISeoFactorItemService seoFactorItemService,
            ISeoFactorReportService seoFactorReportService,
            IGeneralCargoProductTariffCalculatorService generalCargoProductTariffCalculatorService,
            IFactorPublicService seoFactorPublicService,
            ICurrentUserService<UserPanelInfoDto> currentUserService,
            ISeoFactorDeleteService seoFactorDeleteService
        )
        {
            SeoFactorService = seoFactorService;
            SeoFactorItemService = seoFactorItemService;
            SeoFactorReportService = seoFactorReportService;
            GeneralCargoProductTariffCalculatorService = generalCargoProductTariffCalculatorService;
            SeoFactorPublicService = seoFactorPublicService;
            CurrentUserService = currentUserService;
            SeoFactorDeleteService = seoFactorDeleteService;
        }

        #region   صورتحساب های بارنامه
        public async Task<IActionResult> List(int? p, SeoFactorFilterDto filter, CancellationToken token = default, int? returnPage = null)
        {
            var data = await SeoFactorService.GetList(p ?? 1, filter, token);

            if (data.SeoInfo.InlandExportManifestId != null)
                returnPage = 100;

            ViewBag.returnPage = returnPage;

            return View(data);
        }

        public async Task<IActionResult> CreateSeoFactor(CreateSeoFactorDto createDto, CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                createDto.FactorType = FactorType.Seo;
                var id = await SeoFactorService.AddOrUpdateSeoFactor(createDto, token);
                return RedirectToAction(nameof(Details), new { factorId = id });
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(List), new { seoId = createDto.SeoId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoFactorById(Guid factorId, CancellationToken token = default)
        {
            var find = await SeoFactorService.GetById(factorId, token);
            if (find == null)
                throw new Exception("موردی یافت نشد");

            return Json(find);
        }

        public async Task<IActionResult> DeleteFactor(Guid factorId, Guid seoId, CancellationToken token = default)
        {
            await SeoFactorDeleteService.DeleteSeoFactor(factorId, token);
            return RedirectToAction(nameof(List), new { seoId });
        }

        public async Task<IActionResult> Details(Guid factorId, CancellationToken token = default)
        {
            var data = await SeoFactorService.Details(factorId, token);

            return View(data);
        }
        #endregion

        #region Factor
        public async Task<IActionResult> Factor(Guid factorId, CancellationToken token = default)
        {
            var data = await SeoFactorService.GetFactorPage(factorId, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EndFactor(Guid factorId, CancellationToken token = default)
        {
            try
            {
                await SeoFactorService.EndFactor(factorId, token);
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
                var fileDto = await this.SeoFactorService.GetFactorByType(factorId, type, token);

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

        public async Task<IActionResult> GetFactorBimeFile(Guid factorId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this.SeoFactorService.GetFactorBime(factorId, token);

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
                var fileDto = await SeoFactorReportService.GetPreviewFactor(filter, token);

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

        public async Task<IActionResult> GetPreviewBimeFactor(SeoFactorPreviewReportFilterDto filter, CancellationToken token = default)
        {
            try
            {
                var fileDto = await SeoFactorReportService.GetPreviewBimeFactor(filter, token);

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
        public async Task<IActionResult> CreateFactorService(CreateSeoFactorServiceDto createService, CancellationToken token = default)
        {
            if (createService?.Id == Guid.Empty)
                ModelState.Remove("createService.Id");

            if (createService.Type != SeoFactorType.Anbardary)
            {
                ModelState.Remove("createService.WarehousingStartFa");
                ModelState.Remove("createService.WarehousingEndFa");
            }

            var result = new SeoFactorServicePageDto { SeoFactorId = createService.SeoFactorId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await SeoFactorItemService.AddOrEditService(createService, token);
            }

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> DeleteFactorService(Guid seoFactorServiceId, CancellationToken token = default)
        {
            var result = await SeoFactorItemService.DeleteService(seoFactorServiceId, token);

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> DeleteAllFactorServices(Guid seoFactorId, CancellationToken token = default)
        {
            var result = await SeoFactorItemService.DeleteAllServices(seoFactorId, token);
            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> GetServiceById(Guid seoFactorServiceId, CancellationToken token = default)
        {
            var find = await SeoFactorItemService.GetServiceById(seoFactorServiceId, token);

            if (find == null)
            {
                find = new SeoFactorServiceDto();
            }

            return Json(find);
        }

        public async Task<IActionResult> GetModalAddService(SeoFactorServiceFilterDto filter, CancellationToken token = default)
        {
            var data = await SeoFactorService.GetAddServiceData(filter, token);

            switch (filter.SeoFactorType)
            {
                case SeoFactorType.Loading:
                    return PartialView("Partials/_LoadingOptionsPartial", data);
                case SeoFactorType.LoadingChadory:
                    return PartialView("Partials/_LoadingChadoryOptionsPartial", data);
                case SeoFactorType.Anbardary:
                    return PartialView("Partials/_WarehouseOptionsPartial", data);
                case SeoFactorType.StripContainer:
                    return PartialView("Partials/_StripContinerOptionsPartial", data);
                case SeoFactorType.Khankary:
                    return PartialView("Partials/_KargariOptionsPartial", data);
                case SeoFactorType.ThcContainer:
                    return PartialView("Partials/_ThcContainerOptionsPartial", data);
                case SeoFactorType.AllocationOfEquipment:
                    return PartialView("Partials/_AllocationEquOptionsPartial", data);
                default:
                    return PartialView("Partials/_CommonOptionsPartial", data);
            }
        }

        public async Task<IActionResult> GetSeoFactorInfo(Guid factorId, CancellationToken token = default)
        {
            var data = await SeoFactorService.GetSeoFactorInfo(factorId, token);
            return PartialView("Partials/_FactorInfo", data);
        }

        [HttpPost]
        public async Task<IActionResult> SetInfoSeoFactor(SetInfoSeoFactorDto setInfo, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                await SeoFactorService.SetSeoFactorInfo(setInfo, token);
            }

            return Ok();
        }

        #endregion


        #region بستانکاری صاحب کالا
        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
        public async Task<IActionResult> SaveCreditorPrice(CreditorPriceSeoFactorDto creditorPriceDto, CancellationToken token = default)
        {
            await SeoFactorService.SaveCreditorPrice(creditorPriceDto, token);

            return RedirectToAction(nameof(Factor), new { factorId = creditorPriceDto.SeoFactorId });
        }
        #endregion

        #region DocFile
        public async Task<IActionResult> DocFiles(Guid factorId, CancellationToken token = default)
        {
            var model = await this.SeoFactorService.GetFactorDocPage(factorId, token);
            return View(model);
        }

        public async Task<IActionResult> UploadFactorDoc(CreateSeoFactorFileDto CreateDto, CancellationToken token = default)
        {
            await this.SeoFactorService.AddFactorDoc(CreateDto, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = CreateDto.SeoFactorId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoFactorDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this.SeoFactorService.GetFileById(docId, token);

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
        public async Task<IActionResult> RemoveSeoFactorDocFileById(long docId, Guid factorId, CancellationToken token = default)
        {
            await this.SeoFactorService.RemoveFactorDoc(docId, factorId, token);
            return RedirectToAction(nameof(DocFiles), new { factorId = factorId });
        }
        #endregion

        #region استعلام تعرفه باربری
        [HttpPost]
        public async Task<JsonResult> GetGeneralCargoProductTariffPrice(
            GeneralCargoProductTariffCalculatorBySwrProductDto filter,
            CancellationToken token = default
        )
        {
            var result = await SeoFactorService.GetTariffBarbari(filter, token);

            return Json(result);
        }

        /// <summary>
        /// تعرفه باربری غیر کانتینری بدون قبض انبار
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> GetGeneralCargoProductTariffPriceWithoutSwr(
            GeneralCargoProductTariffCalculatorWithoutSwrProductDto filter,
            CancellationToken token = default
        )
        {
            var result = await SeoFactorService.GetTariffGeneralCargoWithoutSwr(filter, token);

            return Json(result);
        }
        #endregion

        #region استعلام تعرفه انبارداری
        [HttpPost]
        public async Task<JsonResult> GetTariffWarehousePrice(
            NonContainerWarehouseProductPriceBySwrProductDto filter,
            CancellationToken token = default
        )
        {
            var result = await SeoFactorService.GetTariffWarehouse(filter, token);

            return Json(result);
        }
        #endregion

        #region استعلام تعرفه استریپ کانتینر
        [HttpPost]
        public async Task<JsonResult> GetTariffStripPrice(
            ContainerStripProductTariffCalculatorBySwrProductDto filter,
            CancellationToken token = default
        )
        {
            var result = await SeoFactorService.GetTariffStrip(filter, token);

            return Json(result);
        }
        #endregion


        #region استعلام تعرفه باربری چادری - منطقه ازاد
        [HttpPost]
        public async Task<JsonResult> GetTariffLoadingChadory(LoadingChadoryTariffCalculatorDto filter, CancellationToken token = default)
        {
            var result = await GeneralCargoProductTariffCalculatorService.GetPriceChadory(filter, token);

            return Json(result);
        }
        #endregion

        #region گزارش از کل درامد ها - خروجی اکسل
        [Authorize(Roles = "admin, adminOnePort, incomeReports, FinanceUser")]
        public async Task<IActionResult> ReportIncome(SeoFactorIncomeReportFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterReportIncome(filter);

            var data = await SeoFactorPublicService.GetSeoFactorIncomePage(filter, token);

            return View(data);
        }

        private async Task ConfigFilterReportIncome(SeoFactorIncomeReportFilterDto filter)
        {
            var user = await CurrentUserService.GetCurrentUser();
            var now = DateTimeOffset.Now.ToShortPersianDateString();
            var dayStart = DateTimeOffset.Now.AddDays(-30).ToShortPersianDateString();

            filter ??= new SeoFactorIncomeReportFilterDto { Page = 1 };
            filter.FilterDate ??= new FilterDateDto { FromDate = dayStart, ToDate = now };
            filter.PortId = user.PortId;
        }

        [Authorize(Roles = "admin, adminOnePort, incomeReports, FinanceUser")]
        public async Task<IActionResult> GetIncomeReportExcel(SeoFactorIncomeReportFilterDto filter = null, CancellationToken token = default)
        {
            await ConfigFilterReportIncome(filter);

            var fileDto = await SeoFactorReportService.GetIncomeReportExcel(filter, token);

            if (fileDto == null)
                return Ok();

            return File(fileDto.FileStream, "application/vnd.ms-excel", fileDto.FileTitleWithExtention);
        }
        #endregion

        #region گزارش ار صورت حساب ها با انبارداری همراه با معافیت
        public async Task<IActionResult> FeeExemptReport(FeeExemptReportFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterFeeExemptReport(filter);

            var data = await SeoFactorPublicService.GetFeeExemptReport(filter, token);

            return View(data);
        }

        private async Task ConfigFilterFeeExemptReport(FeeExemptReportFilterDto filter)
        {
            var user = await CurrentUserService.GetCurrentUser();
            var now = DateTimeOffset.Now.ToShortPersianDateString();
            var dayStart = DateTimeOffset.Now.AddDays(-30).ToShortPersianDateString();

            filter ??= new FeeExemptReportFilterDto { Page = 1 };
            if (filter.Page <= 0)
                filter.Page = 1;
            filter.FilterDate ??= new FilterDateDto { FromDate = dayStart, ToDate = now };
            filter.PortId = user.PortId;
        }
        #endregion
    }
}
