using System;
using System.Threading;
using System.Threading.Tasks;
using Common.enumeration;
using Common.Exceptions;
using DNTPersianUtils.Core;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using SeoService = Services.WebService.Abstract.ISeoService;

namespace WebPanel.Controllers
{
    [Authorize]
    public class AllocationOfEquipmentController : Controller
    {
public IAllocationEquiPreFactorService AllocationEquipmentService { get; }
        public IAllocationEquReportService AllocationEquReportService { get; }
        public IAllocationEquFileService AllocationEquFileService { get; }
        public IAllocationEquChangeStateService AllocationEquChangeStateService { get; }
        public IAllocationEquDocFileService AllocationEquDocFileService { get; }
        public IAllocationEquPaymentService AllocationEquPaymentService { get; }
        public IAllocationEquToEndService AllocationEquToEndService { get; }
        public IAllocationEquPublicService AllocationEquPublicService { get; }
        public ISpecialGearTariffPriceCalculatorService SpecialGearTariffPriceCalculatorService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }
        public ISeoService SeoService { get; }
        public IVoyageService VoyageService { get; }

public AllocationOfEquipmentController(
            IAllocationEquiPreFactorService allocationEquipmentService,
            IAllocationEquReportService allocationEquReportService,
            IAllocationEquFileService allocationEquFileService,
            IAllocationEquChangeStateService allocationEquChangeStateService,
            IAllocationEquDocFileService allocationEquDocFileService,
            IAllocationEquPaymentService allocationEquPaymentService,
            IAllocationEquToEndService allocationEquToEndService,
            IAllocationEquPublicService allocationEquPublicService,
            ISpecialGearTariffPriceCalculatorService specialGearTariffPriceCalculatorService,
            ICurrentUserService<UserPanelInfoDto> currentUserService,
            ISeoService seoService,
            IVoyageService voyageService
        )
        {
            AllocationEquipmentService = allocationEquipmentService;
            AllocationEquReportService = allocationEquReportService;
            AllocationEquFileService = allocationEquFileService;
            AllocationEquChangeStateService = allocationEquChangeStateService;
            AllocationEquDocFileService = allocationEquDocFileService;
            AllocationEquPaymentService = allocationEquPaymentService;
            AllocationEquToEndService = allocationEquToEndService;
            AllocationEquPublicService = allocationEquPublicService;
            SpecialGearTariffPriceCalculatorService = specialGearTariffPriceCalculatorService;
            CurrentUserService = currentUserService;
            SeoService = seoService;
            VoyageService = voyageService;
        }

        //[Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        //public async Task<IActionResult> ListAllocationOfEqu(int? p = null,
        //    AllocationEquListFilterDto filter = null,
        //    CancellationToken token = default)
        //{
        //    var list = await AllocationEquipmentService.GetAllocationEqu(filter, p ?? 1, token);
        //    return View(list);
        //}


        //[Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        //public async Task<IActionResult> CreateAllocationEqu(Guid allocationEquId,
        //    CancellationToken token = default)
        //{
        //    var data = await AllocationEquipmentService.GetById(allocationEquId, token);
        //    return View(data);
        //}

        //[Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        //public async Task<IActionResult> CreateAllocationEqu2(Guid allocationEquId,
        //    CancellationToken token = default)
        //{
        //    var data = await AllocationEquipmentService.GetById(allocationEquId, token);
        //    return View(data);
        //}

        //[Authorize(Roles = "admin")]
        //public async Task<IActionResult> UpdateAllPriceFactor(CancellationToken token = default)
        //{
        //    await AllocationEquPublicService.UpdateAllPriceFactor(token);
        //    return Ok();
        //}

        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        public async Task<IActionResult> PreFactor(Guid allocationEquId, CancellationToken token = default)
        {
            var data = await AllocationEquipmentService.GetAllocationOfEquServicePageDtoById(allocationEquId, token);
            return View(data);
        }

        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        [HttpPost]
        public async Task<IActionResult> GetReportListAllocationOfEqu(
            SeoListFilterDto filter = null,
            ReportType reportType = ReportType.Excel,
            CancellationToken token = default
        )
        {
            try
            {
                var fileDto = await AllocationEquipmentService.GetReportListAllocationOfEqu(filter, reportType, token);

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

        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        [HttpPost]
        public async Task<IActionResult> GetDailyReportPierOperationsExcel(
            AllocationEquDailyReportPierOperationsFilterDto filter = null,
            CancellationToken token = default
        )
        {
            try
            {
                var portId = (await CurrentUserService.GetCurrentUser())?.PortId;
                filter ??= new AllocationEquDailyReportPierOperationsFilterDto
                {
                    DateReport = DateTimeOffset.Now.ToShortPersianDateString(),
                    PortId = portId,
                };

                if (filter.PortId == null)
                    filter.PortId = portId;

                var fileDto = await AllocationEquReportService.GetDailyReportPierOperationsExcel(filter, token);

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
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> BackToPendingState(Guid allocationEquId, CancellationToken token = default)
        {
            await AllocationEquipmentService.BackToPendingState(allocationEquId, token);

            return RedirectToAction(nameof(PreFactor), new { allocationEquId });
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> EndAllocationPreFactor(Guid allocationEquId, CancellationToken token = default)
        {
            await AllocationEquipmentService.EndAllocationPreFactor(allocationEquId, token);

            return RedirectToAction(nameof(PreFactor), new { allocationEquId });
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> CreateEquService(CreateAllocationOfEquServiceDto createDto, CancellationToken token = default)
        {
            if (createDto == null)
            {
                return RedirectToAction("Index", "Seo");
            }

            if (createDto?.Id == Guid.Empty)
            {
                ModelState.Remove("Id");
            }

            var result = new AllocationOfEquServicePageDto { AllocationOfEquId = createDto.AllocationOfEquipmentId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await AllocationEquipmentService.AddOrEditEquService(createDto, token);
                return PartialView("_ListEqupmentService", result);
            }

            return BadRequest();
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> DeleteEquService(Guid equServiceId, CancellationToken token = default)
        {
            var result = await AllocationEquipmentService.DeleteEquService(equServiceId, token);

            return PartialView("_ListEqupmentService", result);
        }

        [HttpPost]
        public async Task<JsonResult> GetSpecialGearPrice(
            SpecialGearTariffPriceByAllocationEquIdCalculatorDto filter,
            CancellationToken token = default
        )
        {
            var result = await SpecialGearTariffPriceCalculatorService.DetectTariff(filter, token);

            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetAllocationEquipmentServiceById(Guid allocationOfEquServiceId, CancellationToken token = default)
        {
            var find = await AllocationEquipmentService.GetAllocationOfEquServiceById(allocationOfEquServiceId, token);
            if (find == null)
            {
                find = new AllocationOfEquServiceDto();
            }

            return Json(find);
        }

        [HttpPost]
        public async Task<IActionResult> GoToStart(Guid allocationEquId, int percent, CancellationToken token = default)
        {
            await AllocationEquipmentService.GoToStart(allocationEquId, percent, token);

            return RedirectToAction("ToEnd", "AllocationOfEquToEnd", new { allocationEquId });
        }

#region Allocation Equipment File
        [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
        public async Task<IActionResult> DocFiles(Guid allocationEquId, int reternPage = 0, CancellationToken token = default)
        {
            var result = await AllocationEquDocFileService.GetDocFilePage(allocationEquId, token);
            
            if (result.SeoId.HasValue)
            {
                result.SeoInfo = await SeoService.GetSeoInfoDto(result.SeoId.Value, token);
            }
            else if (result.VoyageId.HasValue)
            {
                result.VoyageInfo = await VoyageService.GetVoyageInfo(result.VoyageId.Value, token);
            }
            
            ViewBag.reternPage = reternPage;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(
            Guid allocationEquId,
            AllocationEquFileType type,
            IFormFile file,
            string description = null,
            int? goToPageCreate = null,
            bool overwritingFile = true,
            long price = 0,
            string dateFa = null,
            CancellationToken token = default
        )
        {
            try
            {
                if (allocationEquId == Guid.Empty || file == null || file?.Length == 0)
                {
                    return RedirectToAction("Index", "Seo");
                }
                var dateFile = dateFa?.ToGregorianDateTimeOffset() ?? DateTimeOffset.Now;

                var result = await AllocationEquFileService.SaveIFormFile(
                    new Entities.DTOs.Public.FileInfoDto.AllocationEquSaveIFormFileDto
                    {
                        AllocationOfEquId = allocationEquId,
                        Type = type,
                        OverwritingFile = overwritingFile,
                        File = file,
                        Price = price,
                        Description = description,
                        Date = dateFile,
                    },
                    token
                );

                if (result == null)
                    return RedirectToAction("Index", "Seo");

                await AllocationEquPublicService.UpdateRemainderPrice(allocationEquId, token);

                if (goToPageCreate == null)
                    return RedirectToAction(nameof(PreFactor), new { allocationEquId });
                else
                    return RedirectToAction(nameof(DocFiles), new { allocationEquId });
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> GetFile(Guid allocationOfEquId, AllocationEquFileType type, CancellationToken token = default)
        {
            try
            {
                var fileDto = await AllocationEquFileService.ReadFile(allocationOfEquId, type, token);

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

        public async Task<IActionResult> GetFileById(Guid docFileId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await AllocationEquFileService.ReadFileById(docFileId, token);

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

        public async Task<IActionResult> DeleteFileById(Guid docFileId, CancellationToken token = default)
        {
            try
            {
                var allocationEquId = await AllocationEquFileService.DeleteFileById(docFileId, token);

                return RedirectToAction(nameof(DocFiles), new { allocationEquId });
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        //preview factor
        public async Task<IActionResult> GetPreviewFactor(AllocationOfEquPreviewReportFilterDto filter, CancellationToken token = default)
        {
            try
            {
                var fileDto = await AllocationEquReportService.GetPreviewFactor(filter, token);

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
        #endregion


        #region تایید پرداخت در مرحله پیش فاکتور
        [HttpPost]
        public async Task<IActionResult> AcceptPaymentPreFactor(AllocationOfEquAcceptPaymentDto AcceptPaymentDto, CancellationToken token = default)
        {
            var seoId = await AllocationEquChangeStateService.AcceptPaymentPreFactor(AcceptPaymentDto, null, token);

            if (seoId == Guid.Empty)
                return RedirectToAction("index", "Seo");
            else
                return RedirectToAction("SeoAllocationPage", "Seo", new { seoId = seoId });
        }

        #endregion


        #region لیست واریزی های
        public async Task<IActionResult> Payments(AllocationEquPaymentsFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await AllocationEquPaymentService.GetPayments(filter, token, p ?? 1);
            return View(result);
        }

        //گزارش واریزی ها در قالب فایل اکسل

        public async Task<IActionResult> GetAllocationPaymentsFile(AllocationEquPaymentsFilterDto filter = null, CancellationToken token = default)
        {
            var portId = (await CurrentUserService.GetCurrentUser()).PortId;
            filter ??= new AllocationEquPaymentsFilterDto { PortId = portId };
            if (filter?.PortId == null)
            {
                filter.PortId = portId;
            }

            var fileDto = await AllocationEquReportService.GetPaymentsExcelReport2(filter, token);

            if (fileDto == null)
            {
                return Ok();
            }

            return File(fileDto, "application/vnd.ms-excel", $"PaymentsReport.xlsx");
        }

        [HttpPost]
        public async Task<IActionResult> GetAllocationPaymentsCompactFile(
            AllocationEquPaymentsFilterDto filter = null,
            CancellationToken token = default
        )
        {
            var portId = (await CurrentUserService.GetCurrentUser()).PortId;
            filter ??= new AllocationEquPaymentsFilterDto { PortId = portId };
            if (filter?.PortId == null)
            {
                filter.PortId = portId;
            }

            var fileDto = await AllocationEquReportService.GetPaymentsCompactExcelReport(filter, token);

            if (fileDto == null)
            {
                return Ok();
            }

            return File(fileDto, "application/vnd.ms-excel", $"PaymentsReportCompact.xlsx");
        }
        #endregion

        #region بستانکاری صاحب کالا
        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> SaveCreditorPrice(CreditorPriceDto creditorPriceDto, CancellationToken token = default)
        {
            await AllocationEquPublicService.SaveCreditorPrice(creditorPriceDto, token);

            if (creditorPriceDto.RedirectPage == 0)
                return RedirectToAction(nameof(PreFactor), new { allocationEquId = creditorPriceDto.AllocationOfEquId });
            else
                return RedirectToAction("Factor", "AllocationOfEquipmentFactor", new { allocationEquId = creditorPriceDto.AllocationOfEquId });
        }
        #endregion

        #region گزارشات مقایسه ای ماهانه و فصلی
        public async Task<IActionResult> GetReportMonthlyComparisonExcelReport(
            ReportMonthlyFilterDto filterMonthly = null,
            CancellationToken token = default
        )
        {
            var portId = (await CurrentUserService.GetCurrentUser()).PortId;
            filterMonthly ??= new ReportMonthlyFilterDto { PortId = portId };
            if (filterMonthly?.PortId == null || filterMonthly?.PortId == Guid.Empty)
            {
                filterMonthly.PortId = portId;
            }

            var fileDto = await AllocationEquReportService.GetReportMonthlyComparisonExcelReport(filterMonthly, token);

            if (fileDto == null)
            {
                return Ok();
            }

            return File(fileDto.FileStream, "application/vnd.ms-excel", fileDto.FileTitleWithExtention);
        }

        public async Task<IActionResult> GetReportSeasonalComparisonExcelReport(
            ReportSeasonalFilterDto filterSeasonal = null,
            CancellationToken token = default
        )
        {
            var portId = (await CurrentUserService.GetCurrentUser()).PortId;
            filterSeasonal ??= new ReportSeasonalFilterDto { PortId = portId };
            if (filterSeasonal?.PortId == null || filterSeasonal?.PortId == Guid.Empty)
            {
                filterSeasonal.PortId = portId;
            }

            var fileDto = await AllocationEquReportService.GetReportSeasonalComparisonExcelReport(filterSeasonal, token);

            if (fileDto == null)
            {
                return Ok();
            }

            return File(fileDto.FileStream, "application/vnd.ms-excel", fileDto.FileTitleWithExtention);
        }
        #endregion

        #region لیست درآمد اجاره تجهیزات ویژه
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> SpecialIncome(AllocationOfEquIncomeReportFilterDto filter, CancellationToken token = default)
        {
            ConfigFilterSpecialIncome(filter);

            var data = await AllocationEquPublicService.GetAllocationOfEquIncomePage(filter, token);

            return View(data);
        }

        private void ConfigFilterSpecialIncome(AllocationOfEquIncomeReportFilterDto filter)
        {
            var now = DateTimeOffset.Now.ToShortPersianDateString();

            filter ??= new AllocationOfEquIncomeReportFilterDto { Page = 1 };
            filter.FilterDate ??= new FilterDateDto { FromDate = now, ToDate = now };
        }

        public async Task<IActionResult> GetSpecialIncomeReport(AllocationOfEquIncomeReportFilterDto filter = null, CancellationToken token = default)
        {
            ConfigFilterSpecialIncome(filter);

            var fileDto = await AllocationEquReportService.GetSpecialIncomeReport(filter, token);

            if (fileDto == null)
                return Ok();

            return File(fileDto.FileStream, "application/vnd.ms-excel", fileDto.FileTitleWithExtention);
        }
        #endregion
    }
}
