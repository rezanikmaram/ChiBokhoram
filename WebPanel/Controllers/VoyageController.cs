using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.enumeration.Hirman;
using Common.Exceptions;
using Common.Utilities;
using Data.DbContext;
using DNTPersianUtils.Core;
using Entities.DTOs.Api;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.DTOs.Web.Hirman.Tali.ListEquipmentPerformance;
using Entities.Entities.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml;
using Services.PublicService;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;
using WebPanel.Infrastructure;
using static System.Net.WebRequestMethods;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd, FinanceUser")]
    public class VoyageController : Controller
    {
        public static object objLock { get; set; } = new object();

        public IVoyageService VoyageService { get; }
        public IVoyageDeletionService VoyageDeletionService { get; }
        public ISeoService SeoService { get; }
        public ITaliService TaliService { get; }
        public ITaliCommonService TaliCommonService { get; }
        public ITaliWeightService TaliWeightService { get; }

        public IPersonelService PersonelService { get; set; }
        public IPersonelPublicService PersonelPublicService { get; }
        public IVoyageTaliGroupService VoyageTaliGroupService { get; }

        private readonly IExcelToolsService excelTools;
        private readonly AppDbContext _context;
        private readonly ICurrentUserService<Entities.DTOs.Web.UserPanelInfoDto> _currentUserService;
        private readonly ISettingService _settingService;

        public VoyageController(
            IVoyageService voyageService,
            IVoyageDeletionService voyageDeletionService,
            ISeoService seoService,
            ITaliService taliService,
            ITaliCommonService taliCommonService,
            ITaliWeightService taliWeightService,
            IPersonelService personelService,
            IPersonelPublicService personelPublicService,
            IExcelToolsService excelTools,
            AppDbContext context,
            ICurrentUserService<Entities.DTOs.Web.UserPanelInfoDto> currentUserService,
            ISettingService settingService,
            IVoyageTaliGroupService voyageTaliGroupService
        )
        {
            VoyageService = voyageService;
            VoyageDeletionService = voyageDeletionService;
            SeoService = seoService;
            TaliService = taliService;
            TaliCommonService = taliCommonService;
            TaliWeightService = taliWeightService;
            PersonelService = personelService;
            PersonelPublicService = personelPublicService;
            this.excelTools = excelTools;
            _context = context;
            _currentUserService = currentUserService;
            _settingService = settingService;
            VoyageTaliGroupService = voyageTaliGroupService;
        }

        [HttpGet]
        public async Task<IActionResult> AllVoyageWeightReport(AllVoyageWeightReportFilterDto filter, CancellationToken token = default)
        {
            if (filter == null)
                filter = new AllVoyageWeightReportFilterDto();

            var data = await VoyageService.GetAllVoyageWeightReport(filter, token);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllVoyageWeightReport(string dateFromFa, string dateToFa, CancellationToken token = default)
        {
            var filter = new AllVoyageWeightReportFilterDto { DateFromFa = dateFromFa, DateToFa = dateToFa };

            var data = await VoyageService.GetAllVoyageWeightReport(filter, token);

            static string Csv(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return "\"\"";
                return "\"" + input.Replace("\"", "\"\"") + "\"";
            }

            var sb = new StringBuilder();

            // ✅ header columns (exactly same order as data)
            sb.AppendLine(
                string.Join(
                    ",",
                    "شماره سفر",
                    "شماره بارنامه (B/L)",
                    "تناژ کل شناور",
                    "تناژ کل بارنامه",
                    "تناژ باسکول (کیلوگرم)",
                    "اختلاف (کیلوگرم)",
                    "از تاریخ",
                    "تا تاریخ",
                    "نوع کالا",
                    "صاحب کالا"
                )
            );

            foreach (var item in data.Items)
            {
                sb.AppendLine(
                    string.Join(
                        ",",
                        Csv(item.VoyageNumber),
                        Csv(item.BlNumber),
                        Csv(item.VoyageTonnage?.ToString() ?? string.Empty),
                        Csv(item.ShipTonnage.ToString() ?? string.Empty),
                        Csv(item.BaskoolTonnage.ToString() ?? string.Empty),
                        Csv(item.Difference.ToString() ?? string.Empty),
                        Csv(item.DateFromFa),
                        Csv(item.DateToFa),
                        Csv(item.ProductCategory),
                        Csv(item.ProductOwner)
                    )
                );
            }

            var content = sb.ToString();
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
            var fileName = $"AllVoyageWeightReport_{DateTimeOffset.Now:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportAllVoyageWeightReportExcel(string dateFromFa, string dateToFa, CancellationToken token = default)
        {
            var filter = new AllVoyageWeightReportFilterDto { DateFromFa = dateFromFa, DateToFa = dateToFa };

            var data = await VoyageService.GetAllVoyageWeightReport(filter, token);
            var stream = await excelTools.GetAllVoyageWeightReportExcel(data.Items, data.Filter?.DateFromFa, data.Filter?.DateToFa);
            if (stream == null)
                return Ok();

            var fileName = $"AllVoyageWeightReport_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> Index(VoyageFilterDto filter = null, int? p = null, CancellationToken token = default)
        {
            var cookieKey = FilterPersistenceHelper.Key(HttpContext, "Voyage", "Index");

            if (HttpContext.Request.Method == "GET")
            {
                if (Request.Query.Count == 0)
                {
                    filter = FilterPersistenceHelper.Get<VoyageFilterDto>(HttpContext, cookieKey);
                }

                if (filter == null)
                    filter = new VoyageFilterDto { };
            }
            else
            {
                if (filter != null)
                {
                    FilterPersistenceHelper.Save(HttpContext, filter, cookieKey);
                }
            }

            var list = await VoyageService.GetList(p ?? 1, filter, token);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> ExportVoyageListExcel(VoyageFilterDto filter, CancellationToken token = default)
        {
            filter ??= new VoyageFilterDto { VoyageState = VoyageState.Open };

            var data = await VoyageService.GetList(null, filter, token);
            var voyages = data?.List?.Data;
            if (voyages == null || voyages.Count == 0)
                return Ok();

            var headers = new List<string>
            {
                "ردیف",
                "شماره سفر",
                "نوع عملیات",
                "نام کشتی",
                "شرکت کشتیرانی",
                "تاریخ ایجاد",
                "وضعیت سفر",
                "وضعیت کشتی",
                "نوع ترافیک",
                "تناژ",
                "تعداد بارنامه",
                "تعداد اسناد",
                "تعداد تالی",
                "تعداد عملیات تکمیل شده",
            };

            var rows = voyages
                .Select(
                    (item, index) =>
                        new object[]
                        {
                            index + 1,
                            item.Number,
                            item.TransportTypeTitle,
                            item.ShipShipName,
                            item.CompanyShippingCompanyName,
                            item.CreatedDateFa,
                            item.VoyageState.ToDisplay(),
                            item.SeoShipStateTitle,
                            item.TrafficTypeTitle,
                            item.Tonnage,
                            item.SeoCount,
                            item.DocCount,
                            item.TaliCount,
                            item.AllocationStateFinishCount,
                        }
                )
                .ToList();

            var stream = await excelTools.GenerateExcelFileAsync(headers, rows, "Voyages");
            if (stream == null)
                return Ok();

            var fileName = $"Voyages_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> CreateVoyage(
            Guid voyageId,
            CreateVoyageReturnPage reternPage = CreateVoyageReturnPage.VoyageIndex,
            CancellationToken token = default
        )
        {
            //if (!User.IsInRole("admin") && voyageId != Guid.Empty)
            //    return Unauthorized();

            var data = await VoyageService.GetCreateVoyagePage(voyageId, token);
            ViewBag.reternPage = reternPage;

            var isAjax =
                string.Equals(Request?.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase)
                || string.Equals(Request?.Query["isModal"].ToString(), "true", StringComparison.OrdinalIgnoreCase);
            if (isAjax)
                return PartialView("_CreateVoyageModal", data);

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVoyage(
            CreateVoyageDto createDto,
            CreateVoyageReturnPage reternPage = CreateVoyageReturnPage.VoyageIndex,
            CancellationToken token = default
        )
        {
            //if (!User.IsInRole("admin") && createDto.Id != Guid.Empty)
            //    return Unauthorized();

            var msg = await VoyageService.AddOrUpdate(createDto, token);

            var isAjax = string.Equals(Request?.Headers["X-Requested-With"].ToString(), "XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
            if (isAjax)
            {
                if (string.IsNullOrWhiteSpace(msg))
                    return Json(new { success = true, voyageId = createDto.Id });
                else
                    return BadRequest(new { success = false, message = msg });
            }

            TempData["msg"] = msg;

            switch (reternPage)
            {
                case CreateVoyageReturnPage.VoyageIndex:
                    return RedirectToAction(nameof(Index));
                case CreateVoyageReturnPage.VoyageCreate:
                    return RedirectToAction(nameof(Index));
                case CreateVoyageReturnPage.SeoIndex:
                    return RedirectToAction("Index", "Seo");
                default:
                    return RedirectToAction(nameof(Index));
            }
        }

        //اعتبارسنجی در سمت کلاینت - شماره دریایی
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniquePmoNumber(CreateVoyageDto createDto, CancellationToken token = default)
        {
            var result = await VoyageService.UniquePmoNumberValidation(createDto.PmoNumber, createDto.Id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        //[HttpPost]
        //public async Task<IActionResult> VoyageDriverManage()


        public async Task<IActionResult> VoyageSeo(Guid voyageId, CancellationToken token = default)
        {
            try
            {
                var data = await VoyageService.GetVogegSeoPage(voyageId, token);
                return View(data);
            }
            catch (Exception ex) when (ex.Message == "سفر مورد نظر در بندر جاری شما ثبت نشده است")
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> VoyageSeo(VoyageSeoFilterDto filter, Guid voyageId, int? p = null, CancellationToken token = default)
        {
            try
            {
                if (filter == null)
                {
                    filter = new VoyageSeoFilterDto();
                }

                int pageNumber = p ?? 1;
                var data = await VoyageService.GetVogegSeoPageFiltered(filter, voyageId, pageNumber, token);

                return View(data);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> VoyageLogbook(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.GetVoyageLogbookPage(voyageId, token);
            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportVoyageLogbookExcel(Guid voyageId, CancellationToken token = default)
        {
            var bytes = await VoyageService.ExportVoyageLogbookExcel(voyageId, token);
            if (bytes == null || bytes.Length == 0)
                return Ok();

            var fileName = $"VoyageLogbook_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GetVoyageInfo(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.GetVoyageInfo(voyageId, token);

            return PartialView("_VoyageInfo", data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportVoyageSeoExcel(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.GetVogegSeoPage(voyageId, token);
            if (data?.List == null || data.List.Count == 0)
                return Ok();

            var headers = new List<string>
            {
                "ردیف",
                "شماره بارنامه",
                "صاحب کالا",
                "نام کشتی",
                "اسکله",
                "نام کالا",
                "تعداد",
                "نوع بسته بندی",
                "وزن",
                "حجم",
                "تعداد تالی",
                "تعداد کالای ثبت شده",
                "شرکت کشتیرانی",
                "تاریخ شروع",
                "تاریخ ورود",
                "رویه گمرک",
                "نوع عملیات",
                "اخرین قبض انبار",
                "وضعیت",
            };

            var rows = data
                .List.Select(
                    (item, index) =>
                        new object[]
                        {
                            index + 1,
                            item.Number,
                            item.ProductCompanyName,
                            item.ShipShipName,
                            item.WaterfrontWaterfrontName,
                            item.ProductProductName,
                            item.Count,
                            item.ProductPackingName ?? "",
                            item.ShipTonnage,
                            item.Volume,
                            item.TaliCount,
                            item.SeoProductCount,
                            item.ShippingCompanyName,
                            item.Start,
                            item.ShipEntranceDate,
                            item.CustomsProcedureTitle,
                            item.TransportTypeTitle,
                            item.LastSwrNumber,
                            item.State switch
                            {
                                SeoState.Active => "درحال عملیات",
                                SeoState.Stoped => "متوقف شده",
                                SeoState.Finish => "پایان یافته",
                                _ => item.State.ToString(),
                            },
                        }
                )
                .ToList();

            var stream = await excelTools.GenerateExcelFileAsync(headers, rows, "VoyageSeo");
            if (stream == null)
                return Ok();

            var voyageNumber = data.VogageInfo?.VoyageNumber;
            var fileName = string.IsNullOrWhiteSpace(voyageNumber)
                ? $"VoyageSeo_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx"
                : $"VoyageSeo_{voyageNumber}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> CloseVoyage(CloseVoyageDto closeVoyageDto, CancellationToken token = default)
        {
            var result = await VoyageService.CloseVoyage(closeVoyageDto, token);
            TempData["msg"] = result;

            if (string.IsNullOrWhiteSpace(result))
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction(nameof(VoyageSeo), new { voyageId = closeVoyageDto.VoyageId });
        }

        public async Task<IActionResult> FastCloseVoyage(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.FastCloseVoyage(voyageId, token);

            return RedirectToAction(nameof(VoyageSeo), new { voyageId });
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> DeleteVoyage(Guid voyageId, CancellationToken token = default)
        {
            TempData["msg"] = await VoyageDeletionService.DeleteVoyage(voyageId, token);

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> GetAllDeletedVoyages(CancellationToken token = default)
        {
            var data = await VoyageDeletionService.GetAllDeletedVoyages(token);
            return View(data);
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> CascadeSoftDeleteDeletedVoyages(CancellationToken token = default)
        {
            TempData["msg"] = await VoyageDeletionService.CascadeSoftDeleteDeletedVoyages(token);
            return RedirectToAction(nameof(GetAllDeletedVoyages));
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> CascadeHardDeleteDeletedVoyages(CancellationToken token = default)
        {
            TempData["msg"] = await VoyageDeletionService.CascadeHardDeleteDeletedVoyages(token);
            return RedirectToAction(nameof(GetAllDeletedVoyages));
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> DeleteAllSeo(Guid voyageId, CancellationToken token = default)
        {
            var seoIds = await VoyageService.GetAllSeoIdsByVoyageId(voyageId, token);

            foreach (var seoId in seoIds)
            {
                await SeoService.DeleteSeo(seoId, token);
            }

            return RedirectToAction(nameof(VoyageSeo), new { voyageId });
        }

        #region "مدیریت کامیون ها"


        public async Task<IActionResult> TruckManage(TruckVoyageManageFilterDto filterDto, Guid voyageId, CancellationToken token = default)
        {
            var dto = await VoyageService.ManageVoyageTruck(filterDto, token);
            if (dto?.VoyageInfo == null)
                return RedirectToAction(nameof(Index));
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetTruckForVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.SetTruckForVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { voyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTruckFormVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.RemoveTruckFromVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { voyageId = voyageId });
        }

        public async Task<IActionResult> ActivationTruck(Guid truckId, Guid voyageId, CancellationToken token = default)
        {
            await VoyageService.ActivationTruck(voyageId, truckId, token);
            return RedirectToAction(nameof(TruckManage), new { voyageId = voyageId });
        }
        #endregion



        #region مدیریت انبار ها
        public async Task<IActionResult> ManageWarehouse(VoyageWareHouseFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.VoyageId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات سفر وارد صفحه شده اید"));
                return RedirectToAction(nameof(Index));
            }

            var dto = await VoyageService.ManageVoyageWarehouse(filterDto, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetWareHouseForVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.SetWarehouseForVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(ManageWarehouse), new { voyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWareHouseFormVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.RemoveWarehouseFromVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(ManageWarehouse), new { voyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultWareHouse(Guid id, Guid voyageId, CancellationToken token = default)
        {
            if (id.GuidHasValue() && voyageId.GuidHasValue())
            {
                await VoyageService.SetDefaultWarehouse(id, voyageId, token);
            }
            return RedirectToAction(nameof(ManageWarehouse), new { voyageId = voyageId });
        }
        #endregion



        #region مدیریت  کاربران موبایل سفر
        public async Task<IActionResult> ManageUser(VoyageMobileFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.VoyageId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات سفر وارد صفحه شده اید"));
                return RedirectToAction(nameof(Index));
            }

            var dto = await VoyageService.ManageVoyageUser(filterDto, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserForVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.SetUserForVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { voyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromVoyage(string ids, Guid voyageId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.RemoveUserFromVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { voyageId = voyageId });
        }

        #endregion


        #region Tali - بارشماری
        public async Task<IActionResult> CompactTali(TaliCompactFilterDto filter, int returnPage = 0, CancellationToken token = default)
        {
            var data = await TaliService.GetCompactTali(filter, token);
            ViewBag.returnPage = returnPage;

            return View(data);
        }

        public async Task<IActionResult> DetailShipTali(ShipTaliFilterDto filter, CancellationToken token = default)
        {
            var data = await TaliService.DetailShipTali(filter, token);
            return View(data);
        }

        public async Task<IActionResult> DetailTali(TaliDetailFilterDto filter, int returnPage = 0, CancellationToken token = default)
        {
            var data = await TaliService.GetTaliDetail(filter, token);

            // Apply custom filters
            data.List = ApplyCustomFilters(data.List, filter);

            ViewBag.returnPage = returnPage;
            ViewBag.FilterMode = "Voyage";

            // Get CustomerName setting
            var customerName = await _settingService.GetSettingValue(Common.enumeration.Hirman.SystemSettingKey.CustomerName.ToString());
            ViewBag.IsAriaBanader = customerName == "AriaBanader";

            // Get all Tali IDs from the data
            var taliIds = data.List.Select(g => g.DockTaliId).Distinct().ToList();

            // Fetch validation status history for these Tali items
            var historyData = await _context
                .TaliValidationStatusHistories.Where(h => taliIds.Contains(h.TaliId))
                .Include(h => h.ChangedByUser)
                .OrderByDescending(h => h.ChangedDate)
                .Select(h => new TaliValidationStatusHistoryDto
                {
                    TaliId = h.TaliId,
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    Description = h.Description,
                    ChangedDate = h.ChangedDate,
                    ChangedByUserFullName = $"{h.ChangedByUser.FirstName} {h.ChangedByUser.LastName}",
                })
                .ToListAsync(token);

            ViewBag.ValidationStatusHistory = historyData;

            return View(data);
        }

        [HttpGet]
        public async Task<IActionResult> DetailTaliDateRange(TaliDetailFilterDto filter, CancellationToken token = default)
        {
            filter ??= new TaliDetailFilterDto();
            ViewBag.returnPage = 0;
            ViewBag.FilterMode = "DateRange";

            // Get CustomerName setting
            var customerName = await _settingService.GetSettingValue(Common.enumeration.Hirman.SystemSettingKey.CustomerName.ToString());
            ViewBag.IsAriaBanader = customerName == "AriaBanader";

            var hasVoyageFilter = filter.VoyageId.HasValue && filter.VoyageId.Value.GuidHasValue();
            var hasSeoFilter = filter.SeoId.HasValue && filter.SeoId.Value.GuidHasValue();
            if (string.IsNullOrWhiteSpace(filter.DateFromFa))
                filter.DateFromFa = DateTime.Now.ToShortPersianDateString();
            var hasDateFilter = !string.IsNullOrWhiteSpace(filter.DateFromFa) || !string.IsNullOrWhiteSpace(filter.DateToFa);

            TaliDetailPageDto data;

            if (hasVoyageFilter || hasSeoFilter || hasDateFilter)
            {
                data = await TaliService.GetTaliDetail(filter, token);

                // Apply custom filters
                data.List = ApplyCustomFilters(data.List, filter);

                // Filter out invalid talis in date range mode (if no specific filter set)
                if (data?.List != null && !filter.ValidationStatus.HasValue)
                {
                    data.List = data.List.Where(truck => truck.ValidationStatus == Common.enumeration.Hirman.ValidationStatus.Valid).ToList();
                }
            }
            else
            {
                data = new TaliDetailPageDto { Filter = filter, List = new List<TaliDetailGroupTruckDto>() };
            }

            return View(nameof(DetailTali), data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportDetailTaliDateRangeExcel(TaliDetailFilterDto filter, CancellationToken token = default)
        {
            try
            {
                filter ??= new TaliDetailFilterDto();
                var data = await TaliService.GetTaliDetail(filter, token);

                // Apply custom filters
                data.List = ApplyCustomFilters(data.List, filter);

                // Debug: Check if data exists
                if (data?.List == null || !data.List.Any())
                {
                    // Return empty Excel file with headers only
                    var emptyStream = await CreateEmptyTaliExcel();
                    if (emptyStream != null)
                    {
                        var emptyFileName = $"DetailTaliDateRange_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
                        return File(emptyStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", emptyFileName);
                    }
                    return Ok();
                }

                // Get CustomerName setting for column hiding
                var customerName = await _settingService.GetSettingValue(Common.enumeration.Hirman.SystemSettingKey.CustomerName.ToString());
                var hideAriaBanaderColumns = customerName == "AriaBanader";

                var stream = await excelTools.GetTaliDetailDateRangeExcel(data?.List, data?.Filter?.DateFromFa, data?.Filter?.DateToFa);
                if (stream == null)
                {
                    // Fallback to custom Excel generation
                    stream = await CreateCustomTaliDetailExcel(data?.List, hideAriaBanaderColumns);
                }

                if (stream == null)
                    return Ok();

                var fileName = $"DetailTaliDateRange_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                // Log error if needed
                return Ok();
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportDetailTaliExcel(TaliDetailFilterDto filter, CancellationToken token = default)
        {
            var data = await TaliService.GetTaliDetail(filter, token);

            // Apply custom filters
            data.List = ApplyCustomFilters(data.List, filter);

            // Get CustomerName setting for column hiding
            var customerName = await _settingService.GetSettingValue(Common.enumeration.Hirman.SystemSettingKey.CustomerName.ToString());
            var hideAriaBanaderColumns = customerName == "AriaBanader";

            // Create custom Excel export with column hiding
            var stream = await CreateCustomTaliDetailExcel(data?.List, hideAriaBanaderColumns);
            if (stream == null)
                return Ok();

            var fileName = $"DetailTali_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private async Task<MemoryStream> CreateCustomTaliDetailExcel(List<TaliDetailGroupTruckDto> items, bool hideAriaBanaderColumns)
        {
            try
            {
                // ✅ Get CustomerName inside the method (as requested)
                if (!hideAriaBanaderColumns)
                {
                    var customerName = await _settingService.GetSettingValue(Common.enumeration.Hirman.SystemSettingKey.CustomerName.ToString());
                    hideAriaBanaderColumns = customerName == "AriaBanader";
                }

                var memory = new MemoryStream();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var pck = new ExcelPackage();
                var sheet = pck.Workbook.Worksheets.Add("TaliDetails");

                int row = 1;
                sheet.Cells[row, 1].Value = "گزارش تالی ها";
                row += 2;

                // ==================== DYNAMIC HEADERS ====================
                var headers = new List<string> { "#", "تاریخ", "نام شناور", "نوع شمارش", "نوع حمل" };

                if (!hideAriaBanaderColumns)
                    headers.Add("چند سره بودن");

                headers.Add("وسیله جا به جایی");

                if (!hideAriaBanaderColumns)
                    headers.Add("تفکیک یا صفافی");

                headers.Add("صاحب کالا");

                if (!hideAriaBanaderColumns)
                    headers.Add("گروه کارگری");

                headers.AddRange(
                    new[]
                    {
                        "تعداد تجهیز استفاده شده",
                        "نوع کالا",
                        "نوع ترانزیت",
                        "پلاک کامیون",
                        "شماره شاسی",
                        "وزن خالی",
                        "وزن پر",
                        "وزن کالا",
                        "بارنامه",
                        "کالا",
                        "نوع انتقال کانتینر",
                        "شماره کانتینر",
                        "پلمپ کانتینر",
                        "اندازه کانتینر",
                        "نوع کانتینر",
                        "انبار کشتی",
                        "شمارش اسکله",
                        "مشکل اسکله",
                        "کاربر اسکله",
                        "کاربر محوطه",
                        "انبار",
                        "بخش انبار",
                        "شمارش محوطه",
                        "مشکل محوطه",
                    }
                );

                // Write headers
                for (int i = 0; i < headers.Count; i++)
                    sheet.Cells[row, i + 1].Value = headers[i];

                var headerRow = row;
                var dataRow = headerRow + 1;
                var truckIndex = 1;
                var truckList = items ?? new List<TaliDetailGroupTruckDto>();

                foreach (var truck in truckList)
                {
                    if (truck?.TaliProducts == null || truck.TaliProducts.Count == 0)
                        continue;

                    var truckStartRow = dataRow;
                    var totalTruckRows = 0;

                    foreach (var product in truck.TaliProducts)
                    {
                        var areas =
                            (product.TaliDetailAreaProducts != null && product.TaliDetailAreaProducts.Count > 0)
                                ? product.TaliDetailAreaProducts
                                : new List<TaliDetailGroupTruckDto.TaliDetailGroupTaliProduct.TaliDetailAreaProduct> { null };

                        totalTruckRows += areas.Count;
                        var productStartRow = dataRow;

                        foreach (var area in areas)
                        {
                            var currentRow = dataRow;
                            int col = 1;

                            if (currentRow == truckStartRow)
                            {
                                sheet.Cells[currentRow, col++].Value = truckIndex;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliCreatedDate.ToShortPersianDateTimeString();
                                sheet.Cells[currentRow, col++].Value = truck.ShipName;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliTaliType.ToDisplay();
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliUnLoadType.ToDisplay(DisplayProperty.Name);

                                if (!hideAriaBanaderColumns)
                                    sheet.Cells[currentRow, col++].Value = truck.DockTaliUnloadingStageCount;

                                sheet.Cells[currentRow, col++].Value = truck.DockTaliIsSelfTransportTitle;

                                if (!hideAriaBanaderColumns)
                                    sheet.Cells[currentRow, col++].Value = truck.DockTaliIsSeparatedOrSorted == true ? "بلی" : "خیر";

                                sheet.Cells[currentRow, col++].Value = truck.ProductOwnerCompanyName;

                                if (!hideAriaBanaderColumns)
                                    sheet.Cells[currentRow, col++].Value = truck.DockTaliLaborWorkingGroupName;

                                sheet.Cells[currentRow, col++].Value = truck.DockTaliGearCount;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliEntryProductType?.ToDisplay(DisplayProperty.Name);
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliTrafficType?.ToDisplay(DisplayProperty.Name);
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliIsSelfTransported
                                    ? truck.DockTaliIsSelfTransportTitle
                                    : truck.DockTaliTruckCarPlates;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliSeoProductVehicleIdentificationNumber;

                                sheet.Cells[currentRow, col++].Value = truck.DockTaliWeightFree;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliWeightFull;
                                sheet.Cells[currentRow, col++].Value = truck.DockTaliWeightLoad;
                            }

                            if (currentRow == productStartRow)
                            {
                                sheet.Cells[currentRow, col++].Value = product.SeoBillOfLadingNumber;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductProductName;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductContainerMoveType;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductContainerNumber;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductContainerSealNumber;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductContainerSize;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliSeoProductContainerType;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliProductShipWarehouseNumber;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliProductCount;
                                sheet.Cells[currentRow, col++].Value = product.DockTaliProductCountProblem;
                                sheet.Cells[currentRow, col++].Value =
                                    $"{product.DockTaliPersonelFirstName} {product.DockTaliPersonelLastName}".Trim();
                            }

                            sheet.Cells[currentRow, col++].Value =
                                string.IsNullOrWhiteSpace(area?.AreaPersonelFirstName) && string.IsNullOrWhiteSpace(area?.AreaPersonelLastName)
                                    ? ""
                                    : $"{area?.AreaPersonelFirstName} {area?.AreaPersonelLastName}".Trim();

                            sheet.Cells[currentRow, col++].Value = area?.AreaTaliWarehouseName;
                            sheet.Cells[currentRow, col++].Value = area?.AreaWarehouseSegmentName;
                            sheet.Cells[currentRow, col++].Value = area?.AreaTaliProductCount;
                            sheet.Cells[currentRow, col++].Value = area?.AreaTaliProductCountProblem;

                            dataRow++;
                        }

                        // Merge logic (adjusted for hidden columns)
                        if (areas.Count > 1)
                        {
                            int startMergeCol = hideAriaBanaderColumns ? 16 : 19;
                            for (int c = startMergeCol; c <= headers.Count; c++)
                            {
                                sheet.Cells[productStartRow, c, dataRow - 1, c].Merge = true;
                            }
                        }
                    }

                    if (totalTruckRows > 1)
                    {
                        int truckMergeCols = hideAriaBanaderColumns ? 15 : 18;
                        for (int c = 1; c <= truckMergeCols; c++)
                        {
                            sheet.Cells[truckStartRow, c, dataRow - 1, c].Merge = true;
                        }
                    }

                    truckIndex++;
                }

                if (sheet.Dimension != null)
                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var fi = new FileInfo(tempPath);
                pck.SaveAs(fi);

                using (var fs = new FileStream(tempPath, FileMode.Open))
                {
                    await fs.CopyToAsync(memory);
                }
                System.IO.File.Delete(tempPath);

                memory.Position = 0;
                return memory;
            }
            catch (Exception ex)
            {
                // Log error if needed
                return null;
            }
        }

        private async Task<MemoryStream> CreateEmptyTaliExcel()
        {
            try
            {
                var memory = new MemoryStream();

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using var pck = new ExcelPackage();
                var sheet = pck.Workbook.Worksheets.Add("TaliDetails");

                int row = 1;
                sheet.Cells[row, 1].Value = "گزارش تالی ها - هیچ داده‌ای یافت نشد";
                row += 2;

                // Add headers
                var headers = new[]
                {
                    "#",
                    "تاریخ",
                    "نام شناور",
                    "نوع شمارش",
                    "نوع حمل",
                    "چند سره بودن",
                    "وسیله جا به جایی",
                    "تفکیک یا صفافی",
                    "صاحب کالا",
                    "گروه کارگری",
                    "تعداد تجهیز استفاده شده",
                    "نوع کالا",
                    "نوع ترانزیت",
                    "پلاک کامیون",
                    "شماره شاسی",
                    "وزن خالی",
                    "وزن پر",
                    "وزن کالا",
                    "بارنامه",
                    "کالا",
                    "نوع انتقال کانتینر",
                    "شماره کانتینر",
                    "پلمپ کانتینر",
                    "اندازه کانتینر",
                    "نوع کانتینر",
                    "انبار کشتی",
                    "شمارش اسکله",
                    "مشکل اسکله",
                    "کاربر اسکله",
                    "کاربر محوطه",
                    "انبار",
                    "بخش انبار",
                    "شمارش محوطه",
                    "مشکل محوطه",
                };

                for (int i = 0; i < headers.Length; i++)
                    sheet.Cells[row, i + 1].Value = headers[i];

                if (sheet.Dimension != null)
                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var fi = new FileInfo(tempPath);
                pck.SaveAs(fi);

                using (var fs = new FileStream(tempPath, FileMode.Open))
                {
                    await fs.CopyToAsync(memory);
                }
                System.IO.File.Delete(tempPath);

                memory.Position = 0;
                return memory;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<TaliDetailGroupTruckDto> ApplyCustomFilters(List<TaliDetailGroupTruckDto> items, TaliDetailFilterDto filter)
        {
            if (items == null || !items.Any())
                return items;

            var filteredItems = items.AsEnumerable();

            // Filter by ProductOwnerCompanyName (صاحب کالا)
            if (!string.IsNullOrWhiteSpace(filter.ProductOwnerCompanyName))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.ProductOwnerCompanyName != null
                    && truck.ProductOwnerCompanyName.Contains(filter.ProductOwnerCompanyName, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Filter by ProductCategory (نوع کالا)
            if (!string.IsNullOrWhiteSpace(filter.ProductCategory))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.DockTaliEntryProductType != null
                    && truck
                        .DockTaliEntryProductType.ToDisplay(DisplayProperty.Name)
                        .Contains(filter.ProductCategory, StringComparison.OrdinalIgnoreCase)
                );
            }

            // Filter by TruckPlateOrChassis (پلاک/شاسی)
            if (!string.IsNullOrWhiteSpace(filter.TruckPlateOrChassis))
            {
                filteredItems = filteredItems.Where(truck =>
                    (
                        truck.DockTaliTruckCarPlates != null
                        && truck.DockTaliTruckCarPlates.Contains(filter.TruckPlateOrChassis, StringComparison.OrdinalIgnoreCase)
                    )
                    || (
                        truck.DockTaliSeoProductVehicleIdentificationNumber != null
                        && truck.DockTaliSeoProductVehicleIdentificationNumber.Contains(
                            filter.TruckPlateOrChassis,
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
                );
            }

            // Filter by ProductName (کالا)
            if (!string.IsNullOrWhiteSpace(filter.ProductName))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.TaliProducts != null
                    && truck.TaliProducts.Any(product =>
                        product.DockTaliSeoProductProductName != null
                        && product.DockTaliSeoProductProductName.Contains(filter.ProductName, StringComparison.OrdinalIgnoreCase)
                    )
                );
            }

            // Filter by DockTaliPersonelFirstName (کاربر اسکله)
            if (!string.IsNullOrWhiteSpace(filter.DockTaliPersonelFirstName))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.TaliProducts != null
                    && truck.TaliProducts.Any(product =>
                        ($"{product.DockTaliPersonelFirstName} {product.DockTaliPersonelLastName}")
                            .Trim()
                            .Contains(filter.DockTaliPersonelFirstName, StringComparison.OrdinalIgnoreCase)
                    )
                );
            }

            // Filter by AreaPersonelFirstName (کاربر محوطه)
            if (!string.IsNullOrWhiteSpace(filter.AreaPersonelFirstName))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.TaliProducts != null
                    && truck.TaliProducts.Any(product =>
                        product.TaliDetailAreaProducts != null
                        && product.TaliDetailAreaProducts.Any(area =>
                            ($"{area.AreaPersonelFirstName} {area.AreaPersonelLastName}")
                                .Trim()
                                .Contains(filter.AreaPersonelFirstName, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                );
            }

            // Filter by AreaTaliWarehouseName (انبار)
            if (!string.IsNullOrWhiteSpace(filter.AreaTaliWarehouseName))
            {
                filteredItems = filteredItems.Where(truck =>
                    truck.TaliProducts != null
                    && truck.TaliProducts.Any(product =>
                        product.TaliDetailAreaProducts != null
                        && product.TaliDetailAreaProducts.Any(area =>
                            area.AreaTaliWarehouseName != null
                            && area.AreaTaliWarehouseName.Contains(filter.AreaTaliWarehouseName, StringComparison.OrdinalIgnoreCase)
                        )
                    )
                );
            }

            // Filter by ValidationStatus (وضعیت تایید)
            if (filter.ValidationStatus.HasValue)
            {
                filteredItems = filteredItems.Where(truck => (int)truck.ValidationStatus == filter.ValidationStatus.Value);
            }

            return filteredItems.ToList();
        }

        [HttpGet]
        public async Task<IActionResult> GetEquipmentPerformance(ListEquipmentPerformanceFilterDto filter, CancellationToken token = default)
        {
            var data = await VoyageService.GetEquipmentPerformance(filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetEquipmentPerformanceFiltering(ListEquipmentPerformanceFilterDto filter, CancellationToken token = default)
        {
            var data = await VoyageService.GetEquipmentPerformance(filter, token);
            return View(nameof(GetEquipmentPerformance), data);
        }

        [HttpGet]
        public async Task<IActionResult> GearPerformanceReport(GearPerformanceReportFilterDto filter, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(filter.DateFromFa) && string.IsNullOrEmpty(filter.DateToFa))
            {
                filter.DateToFa = DateTimeOffset.Now.ToShortPersianDateString();
                filter.DateFromFa = DateTimeOffset.Now.AddMonths(-1).ToShortPersianDateString();
            }

            var data = await VoyageService.GetGearPerformanceReport(filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GearPerformanceReportFiltering(GearPerformanceReportFilterDto filter, CancellationToken token = default)
        {
            var data = await VoyageService.GetGearPerformanceReport(filter, token);
            return View(nameof(GearPerformanceReport), data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportGearPerformanceReportExcel(
            string dateFromFa,
            string dateToFa,
            string search,
            CancellationToken token = default
        )
        {
            var filter = new GearPerformanceReportFilterDto
            {
                DateFromFa = dateFromFa,
                DateToFa = dateToFa,
                Search = search,
            };

            var data = await VoyageService.GetGearPerformanceReport(filter, token);
            var stream = await excelTools.GetGearPerformanceReportExcel(data.Data, data.Filter?.DateFromFa, data.Filter?.DateToFa);
            if (stream == null)
                return Ok();

            var fileName = $"GearPerformanceReport_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GetCheckListFactorTali(Guid taliId, CancellationToken token = default)
        {
            var items = await TaliService.GetCheckListFactorTali(taliId, token);

            return PartialView("_CheckListFactorTali", items);
        }

        [HttpPost]
        public async Task<IActionResult> GetFilesTali(Guid taliId, CancellationToken token = default)
        {
            var items = await TaliService.GetFilesTali(taliId, token);

            return PartialView("_GetFilesTali", items);
        }

        [HttpPost]
        public async Task<IActionResult> GetTaliGears(Guid taliId, CancellationToken token = default)
        {
            var items = await TaliService.GetTaliGears(taliId, token);

            return PartialView("_GetTaliGears", items);
        }

        [HttpGet]
        public async Task<IActionResult> VoyageWeightReport(VoyageWeightReportFilterDto filter, CancellationToken token = default)
        {
            if (filter == null || filter.VoyageId == Guid.Empty)
                return RedirectToAction(nameof(Index));

            var data = await VoyageService.GetVoyageWeightReport(filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> VoyageWeightReportFiltering(VoyageWeightReportFilterDto filter, CancellationToken token = default)
        {
            var data = await VoyageService.GetVoyageWeightReport(filter, token);
            return View(nameof(VoyageWeightReport), data);
        }

        [HttpGet]
        public async Task<IActionResult> ExportVoyageWeightReport(
            Guid voyageId,
            string dateFromFa,
            string dateToFa,
            CancellationToken token = default
        )
        {
            var filter = new VoyageWeightReportFilterDto
            {
                VoyageId = voyageId,
                DateFromFa = dateFromFa,
                DateToFa = dateToFa,
            };

            var data = await VoyageService.GetVoyageWeightReport(filter, token);

            static string Csv(string input) => "\"" + (input ?? string.Empty).Replace("\"", "\"\"") + "\"";

            var sb = new StringBuilder();
            sb.AppendLine("شماره سفر,شماره بارنامه (B/L),تناژ کل شناور,تناژ کل بارنامه,تناژ باسکول (کیلوگرم),اختلاف (کیلوگرم),از تاریخ,تا تاریخ");

            foreach (var item in data.Items)
            {
                sb.AppendLine(
                    string.Join(
                        ",",
                        Csv(item.VoyageNumber),
                        Csv(item.BlNumber),
                        item.VoyageTonnage?.ToString() ?? string.Empty,
                        item.ShipTonnage.ToString(),
                        item.BaskoolTonnage.ToString(),
                        item.Difference.ToString(),
                        Csv(item.DateFromFa),
                        Csv(item.DateToFa)
                    )
                );
            }

            var content = sb.ToString();
            var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(content)).ToArray();
            var fileName = $"VoyageWeightReport_{data.VogageInfo?.VoyageNumber}_{DateTimeOffset.Now:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv; charset=utf-8", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportVoyageWeightReportExcel(
            Guid voyageId,
            string dateFromFa,
            string dateToFa,
            CancellationToken token = default
        )
        {
            var filter = new VoyageWeightReportFilterDto
            {
                VoyageId = voyageId,
                DateFromFa = dateFromFa,
                DateToFa = dateToFa,
            };

            var data = await VoyageService.GetVoyageWeightReport(filter, token);
            var stream = await excelTools.GetVoyageWeightReportExcel(data.Items, data.VogageInfo);
            if (stream == null)
                return Ok();

            var fileName = $"VoyageWeightReport_{data.VogageInfo?.VoyageNumber}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GetListTaliOneProduct(Guid voyageId, Guid? seoProductId, bool step, CancellationToken token = default)
        {
            var find = await TaliService.GetListTaliOneProduct(voyageId, seoProductId, step, token);

            if (step)
                return PartialView("_TaliList", find);
            else
                return PartialView("_TaliListWithWarehouse", find);
        }

        [HttpPost]
        public async Task<IActionResult> GetTaliGearList(Guid voyageId, Guid? seoProductId, bool step, CancellationToken token = default)
        {
            var page = await TaliService.GetTaliGearsList(voyageId, seoProductId, step, token);
            return PartialView("_TaliGearList", page);
        }

        [HttpPost]
        public async Task<IActionResult> GetWeightTaliList(Guid taliId, CancellationToken token = default)
        {
            var list = await TaliService.GetWeightTaliList(taliId, token);

            return PartialView("_GetWeightTaliList", list);
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoProductInfo(Guid seoProductId, CancellationToken token = default)
        {
            var find = await TaliService.GetSeoProductInfo(seoProductId, token);

            return PartialView("_TaliProductInfo", find);
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoInfo(Guid seoId, CancellationToken token = default)
        {
            var find = await TaliService.GetSeoInfo(seoId, token);
            return PartialView("_SeoInfo2", find);
        }

        public async Task<IActionResult> TaliFile(GetFileTaliDto fileInfo, CancellationToken token = default)
        {
            try
            {
                var fileDto = await TaliCommonService.GetFile(fileInfo, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileInfo.FileTitle);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> FindWeightTali(Guid voyageId)
        {
            await TaliWeightService.StartJobFindTaliWithoutWeight();

            return RedirectToAction(nameof(CompactTali), new TaliCompactFilterDto { VoyageId = voyageId });
        }

        public async Task<IActionResult> SetActiveVoyage(Guid personelId, Guid voyageId, CancellationToken token = default)
        {
            await PersonelPublicService.SetActiveVoyage(personelId, voyageId, token);
            return RedirectToAction(nameof(ManageUser), new { voyageId = voyageId });
        }

        public async Task<IActionResult> SetDisableVoyage(Guid personelId, Guid voyageId, CancellationToken token = default)
        {
            await this.PersonelService.SetDisableVoyage(personelId, voyageId, token);
            return RedirectToAction(nameof(ManageUser), new { voyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeTaliValidationStatus(
            Guid taliId,
            ValidationStatus validationStatus,
            string description,
            CancellationToken token = default
        )
        {
            using var transaction = await _context.Database.BeginTransactionAsync(token);
            try
            {
                var tali = await _context.Talis.Include(x => x.TaliProducts).FirstOrDefaultAsync(x => x.Id == taliId, token);

                if (tali != null)
                {
                    var oldStatus = tali.ValidationStatus;

                    // Get current user
                    var currentUser = await _currentUserService.GetCurrentUser();
                    if (currentUser == null)
                    {
                        return Json(new { success = false, message = "کاربر شناسایی نشد" });
                    }

                    // Update tali counts based on status change
                    await UpdateTaliCounts(tali, oldStatus, validationStatus, token);

                    tali.ValidationStatus = validationStatus;

                    // Create history record
                    var history = new TaliValidationStatusHistory
                    {
                        TaliId = taliId,
                        OldStatus = oldStatus,
                        NewStatus = validationStatus,
                        ChangedByUserId = currentUser.UserId,
                        Description = description,
                        ChangedDate = DateTimeOffset.UtcNow,
                    };

                    _context.TaliValidationStatusHistories.Add(history);
                    await _context.SaveChangesAsync(token);

                    await transaction.CommitAsync(token);
                    return Json(new { success = true, message = "وضعیت تأیید با موفقیت تغییر کرد" });
                }
                return Json(new { success = false, message = "تالی یافت نشد" });
            }
            catch
            {
                await transaction.RollbackAsync(token);
                return Json(new { success = false, message = "خطا در به‌روزرسانی وضعیت تالی" });
            }
        }

        private async Task UpdateTaliCounts(Tali tali, ValidationStatus oldStatus, ValidationStatus newStatus, CancellationToken token)
        {
            // Only process if status actually changed
            if (oldStatus == newStatus)
                return;

            var seoProductIds = tali.TaliProducts.Where(x => x.SeoProductId != null).Select(x => x.SeoProductId.Value).Distinct().ToList();

            if (!seoProductIds.Any())
                return;

            var seoProducts = await _context.SeoProducts.Where(x => seoProductIds.Contains(x.Id)).ToListAsync(token);

            // Get mohavate tali data upfront if this is a waterfront tali
            List<SeoProduct> mohavateSeoProducts = null;
            if (tali.TaliType == TaliType.Waterfront)
            {
                var mohavateTali = await _context
                    .Talis.Include(x => x.TaliProducts)
                    .FirstOrDefaultAsync(x => x.ParentTaliId == tali.Id && x.TaliType == TaliType.Mohavate, token);

                if (mohavateTali != null)
                {
                    var mohavateSeoProductIds = mohavateTali
                        .TaliProducts.Where(x => x.SeoProductId != null)
                        .Select(x => x.SeoProductId.Value)
                        .Distinct()
                        .ToList();

                    if (mohavateSeoProductIds.Any())
                    {
                        mohavateSeoProducts = await _context.SeoProducts.Where(x => mohavateSeoProductIds.Contains(x.Id)).ToListAsync(token);
                    }
                }
            }

            lock (objLock)
            {
                if (tali.TaliType == TaliType.Waterfront)
                {
                    // Update TaliCount1 for waterfront talis
                    foreach (var taliProduct in tali.TaliProducts.Where(x => x.SeoProductId != null))
                    {
                        var seoProduct = seoProducts.FirstOrDefault(x => x.Id == taliProduct.SeoProductId);
                        if (seoProduct == null)
                            continue;

                        if (oldStatus == ValidationStatus.Valid && newStatus != ValidationStatus.Valid)
                        {
                            // Changing from Valid to Unvalid - decrease TaliCount1
                            seoProduct.TaliCount1 -= taliProduct.Count;
                        }
                        else if (oldStatus != ValidationStatus.Valid && newStatus == ValidationStatus.Valid)
                        {
                            // Changing from Unvalid to Valid - increase TaliCount1
                            seoProduct.TaliCount1 += taliProduct.Count;
                        }
                    }

                    // Handle linked mohavate (safaf) tali counts
                    UpdateMohavateTaliCountsSync(tali.Id, oldStatus, newStatus, mohavateSeoProducts);
                }
                else if (tali.TaliType == TaliType.Mohavate)
                {
                    // Update TaliCount2 for mohavate talis
                    foreach (var taliProduct in tali.TaliProducts.Where(x => x.SeoProductId != null))
                    {
                        var seoProduct = seoProducts.FirstOrDefault(x => x.Id == taliProduct.SeoProductId);
                        if (seoProduct == null)
                            continue;

                        if (oldStatus == ValidationStatus.Valid && newStatus != ValidationStatus.Valid)
                        {
                            // Changing from Valid to Unvalid - decrease TaliCount2
                            seoProduct.TaliCount2 -= taliProduct.Count;
                        }
                        else if (oldStatus != ValidationStatus.Valid && newStatus == ValidationStatus.Valid)
                        {
                            // Changing from Unvalid to Valid - increase TaliCount2
                            seoProduct.TaliCount2 += taliProduct.Count;
                        }
                    }
                }

                _context.SaveChanges();
            }
        }

        private void UpdateMohavateTaliCountsSync(
            Guid waterfrontTaliId,
            ValidationStatus oldStatus,
            ValidationStatus newStatus,
            List<SeoProduct> mohavateSeoProducts
        )
        {
            if (mohavateSeoProducts == null || !mohavateSeoProducts.Any())
                return;

            // Find the linked mohavate (safaf) tali and its products
            var mohavateTali = _context
                .Talis.Include(x => x.TaliProducts)
                .FirstOrDefault(x => x.ParentTaliId == waterfrontTaliId && x.TaliType == TaliType.Mohavate);

            if (mohavateTali == null)
                return;

            // Update TaliCount2 for the linked mohavate tali
            foreach (var taliProduct in mohavateTali.TaliProducts.Where(x => x.SeoProductId != null))
            {
                var seoProduct = mohavateSeoProducts.FirstOrDefault(x => x.Id == taliProduct.SeoProductId);
                if (seoProduct == null)
                    continue;

                if (oldStatus == ValidationStatus.Valid && newStatus != ValidationStatus.Valid)
                {
                    // Waterfront tali changing from Valid to Unvalid - decrease TaliCount2
                    seoProduct.TaliCount2 -= taliProduct.Count;
                }
                else if (oldStatus != ValidationStatus.Valid && newStatus == ValidationStatus.Valid)
                {
                    // Waterfront tali changing from Unvalid to Valid - increase TaliCount2
                    seoProduct.TaliCount2 += taliProduct.Count;
                }
            }
        }

        #endregion

        #region اسناد

        public async Task<IActionResult> GetVoyageDocs(Guid voyageId, CancellationToken token = default)
        {
            var model = await this.VoyageService.GetAllVoyageDocsPage(voyageId, token);
            return View(model);
        }

        public async Task<IActionResult> CreateVoyageDocument(CreateVoyageDocumentDto createVoyageDocument, CancellationToken token = default)
        {
            await this.VoyageService.CreateVoyageDocument(createVoyageDocument, token);
            return RedirectToAction(nameof(GetVoyageDocs), new { voyageId = createVoyageDocument.VoyageId });
        }

        [HttpPost]
        public async Task<IActionResult> GetVoyageDocumentFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await VoyageService.GetFileById(docId, token);

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

        public async Task<IActionResult> DeleteVoyageDocument(long voyageDocumentId, CancellationToken token)
        {
            var result = await this.VoyageService.DeleteVoyageDocument(voyageDocumentId, token);
            return RedirectToAction(nameof(GetVoyageDocs), new { voyageId = result });
        }
        #endregion

        #region "مدیریت تجهیزات"
        public async Task<IActionResult> EquipmentManage(VoyageEquipmentFilterDto filterDto, int returnPage, CancellationToken token = default)
        {
            var result = await VoyageService.ManageVoyageEquipment(filterDto, token);
            ViewBag.returnPage = returnPage;

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSeoEquipment(Guid voyageId, CancellationToken token = default)
        {
            await VoyageService.UpdateSeoEquipment(voyageId, token);

            return RedirectToAction(nameof(EquipmentManage), new { VoyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> SetEquipmentForVoyage(
            string ids,
            Guid voyageId,
            int returnPage,
            bool usedInWaterfrontSide = false,
            bool usedInPortSide = false,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await VoyageService.SetEquipmentForVoyage(ids, voyageId, usedInWaterfrontSide, usedInPortSide, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { VoyageId = voyageId, returnPage = returnPage });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEquipmentFormVoyage(string ids, Guid voyageId, int returnPage, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                TempData["msg"] = await VoyageService.RemoveEquipmentFromVoyage(ids, voyageId, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { VoyageId = voyageId, returnPage = returnPage });
        }

        public async Task<IActionResult> ActivationEquipment(int equipmentId, Guid voyageId, CancellationToken token = default)
        {
            await VoyageService.ActivationEquipment(voyageId, equipmentId, token);
            return RedirectToAction(nameof(EquipmentManage), new { VoyageId = voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEquipmentSide(
            Guid voyageId,
            int equipmentId,
            string side,
            bool isChecked,
            CancellationToken token = default
        )
        {
            await VoyageService.UpdateEquipmentSide(voyageId, equipmentId, side, isChecked, token);
            return Json(new { success = true });
        }
        #endregion

        #region صورتحساب اجاره تجهیزات
        public async Task<IActionResult> VoyageAllocationPage(Guid voyageId, CancellationToken token = default)
        {
            var result = await VoyageService.GetVoyageAllocationPage(voyageId, token);

            return View(result);
        }
        #endregion

        #region تعیین تکلیف تالی آفلاین
        public async Task<IActionResult> OfflineTaliConversionPage(Guid voyageId, CancellationToken token = default)
        {
            var result = await VoyageService.GetOfflineTaliConversionPage(voyageId, token);
            return View(result);
        }

        public async Task<IActionResult> OfflineTaliConversionDetail(Guid voyageId, Guid offlineTaliId, CancellationToken token = default)
        {
            var result = await VoyageService.GetOfflineTaliConversionDetail(voyageId, offlineTaliId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ConvertOfflineTali(ConvertOfflineTaliSubmitDto dto, CancellationToken token = default)
        {
            var res = await VoyageService.ConvertOfflineTali(dto, token);
            TempData["msg"] = res;
            return RedirectToAction(nameof(OfflineTaliConversionPage), new { voyageId = dto.VoyageId });
        }
        #endregion

        #region مدیریت همه کالاهای سفر
        public async Task<IActionResult> VoyageProduct(ProductVoyageManageFilterDto filter, CancellationToken token)
        {
            var data = await VoyageService.GetProductManagePage(filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeSeoProductNeedsSpecialEquipment(Guid seoProductId, CancellationToken token)
        {
            var data = await VoyageService.ChangeSeoProductNeedsSpecialEquipment(seoProductId, token);
            if (data)
                return Ok();
            return BadRequest();
        }
        #endregion

        #region Voyage SEO Detail Page
        public async Task<IActionResult> VoyageSeoDetail(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.GetVoyageSeoDetailPage(voyageId, token);
            return View(data);
        }
        #endregion

        #region Export/Import Voyage Seo Data
        [HttpGet]
        public async Task<IActionResult> ExportVoyageSeoDataExcel(Guid voyageId, CancellationToken token = default)
        {
            var data = await VoyageService.GetVogegSeoPage(voyageId, token);
            if (data?.List == null || data.List.Count == 0)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "هیچ بارنامه‌ای برای خروجی یافت نشد"));
                return RedirectToAction(nameof(VoyageSeo), new { voyageId });
            }

            var headers = new List<string>
            {
                "شناسه (برای بروزرسانی خالی نگذارید)",
                "شماره بارنامه",
                "کد ملی شرکت صاحب کالا",
                "HS Code",
                "تعداد",
                "وزن (کیلوگرم)",
                "حجم",
                "تاریخ ورود (YYYY/MM/DD)",
                "شماره دریایی",
                "توضیحات",
                "MARKS",
                "نوع محموله (کد: 0=فله, 1=کالای عمومی, 2=کانتینری, 3=نفتی)",
                "رویه گمرکی (کد: 0=واردات, 1=صادرات, 2=ترانزیت داخلی, 3=ترانزیت خارجی, 4=ترانشیپ داخلی, 5=ترانشیپ خارجی, 6=کاپوتاژ, 7=برگشت از واردات, 8=برگشت از صادرات, 9=صادرات مجدد)",
                "بسته بندی (نام انگلیسی)",
                "--- فیلدهای زیر فقط برای نمایش (مشاهده) ---",
                "نوع تردد (فقط نمایش)",
                "رویه گمرکی (فقط نمایش)",
                "نام بسته بندی (فقط نمایش)",
                "نام شرکت صاحب کالا (فقط نمایش)",
                "نام کشتی (فقط نمایش)",
                "نام اسکله (فقط نمایش)",
                "تاریخ ثبت (فقط نمایش)",
            };

            var rows = data
                .List.Select(
                    (item, index) =>
                        new object[]
                        {
                            item.Id.ToString(),
                            item.BillOfLadingNumber ?? "",
                            item.ProductOwnerMeliCode ?? "",
                            item.HSCode ?? "",
                            item.Count.ToString(),
                            item.ShipTonnage.ToString("N0"),
                            item.Volume.ToString("N0"),
                            item.ShipEntranceDate ?? "",
                            item.PmoNumber ?? "",
                            item.Description ?? "",
                            item.Marks ?? "",
                            item.TrafficType.HasValue ? ((int)item.TrafficType.Value).ToString() : "",
                            item.CustomsProcedure.HasValue ? ((int)item.CustomsProcedure.Value).ToString() : "",
                            item.ProductPackingEnglishName ?? "",
                            "", // Separator column
                            item.TrafficTypeTitle ?? "",
                            item.CustomsProcedureTitle ?? "",
                            item.ProductPackingName ?? "",
                            item.ProductCompanyName ?? "",
                            item.ShipName ?? "",
                            item.WaterfrontName ?? "",
                            item.CreatedDateDisplay ?? "",
                        }
                )
                .ToList();

            var stream = await excelTools.GenerateExcelFileAsync(headers, rows, "VoyageSeoData");
            if (stream == null)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "خطا در ایجاد فایل Excel"));
                return RedirectToAction(nameof(VoyageSeo), new { voyageId });
            }

            var voyageNumber = data.VogageInfo?.VoyageNumber ?? "Unknown";
            var fileName = $"VoyageSeoData_{voyageNumber}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> ImportVoyageSeoDataExcel(Guid voyageId, IFormFile excelFile, CancellationToken token = default)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "فایل Excel انتخاب نشده است" });
            }

            if (!voyageId.GuidHasValue())
            {
                return Json(new { success = false, message = "شناسه سفر نامعتبر است" });
            }

            try
            {
                var voyage = await VoyageService.GetVoyageById(voyageId, token);
                if (voyage == null)
                {
                    return Json(new { success = false, message = "سفر یافت نشد" });
                }

                using var stream = new MemoryStream();
                await excelFile.CopyToAsync(stream, token);
                stream.Position = 0;

                using var package = new OfficeOpenXml.ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet == null)
                {
                    return Json(new { success = false, message = "فایل Excel خالی یا نامعتبر است" });
                }

                int rowCount = worksheet.Dimension?.Rows ?? 0;
                if (rowCount < 2)
                {
                    return Json(new { success = false, message = "فایل Excel حداقل باید دارای یک سطر هدر و یک سطر داده باشد" });
                }

                int insertedCount = 0;
                int updatedCount = 0;
                int errorCount = 0;
                var errors = new List<string>();

                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        string idValue = worksheet.Cells[row, 1]?.Text?.Trim() ?? "";
                        Guid? existingSeoId = null;
                        bool isUpdate = false;

                        if (!string.IsNullOrEmpty(idValue) && Guid.TryParse(idValue, out Guid parsedId))
                        {
                            existingSeoId = parsedId;
                            isUpdate = true;
                        }

                        string billOfLadingNumber = worksheet.Cells[row, 2]?.Text?.Trim() ?? "";
                        string meliCode = worksheet.Cells[row, 3]?.Text?.Trim() ?? "";
                        string hsCode = worksheet.Cells[row, 4]?.Text?.Trim() ?? "";

                        // Parse numbers with Persian to English conversion and remove thousand separators
                        string countText = worksheet.Cells[row, 5]?.Text?.Fa2En()?.Replace(",", "") ?? "1";
                        string weightText = worksheet.Cells[row, 6]?.Text?.Fa2En()?.Replace(",", "") ?? "0";
                        string volumeText = worksheet.Cells[row, 7]?.Text?.Fa2En()?.Replace(",", "") ?? "0";

                        int.TryParse(countText, out int count);
                        decimal.TryParse(weightText, out decimal weight);
                        decimal.TryParse(volumeText, out decimal volume);

                        string shipEntranceDateFa = worksheet.Cells[row, 8]?.Text?.Trim() ?? "";
                        string pmoNumber = worksheet.Cells[row, 9]?.Text?.Trim() ?? "";
                        string description = worksheet.Cells[row, 10]?.Text?.Trim() ?? "";
                        string marks = worksheet.Cells[row, 11]?.Text?.Trim() ?? "";

                        // Parse new fields: TrafficType, CustomsProcedure, ProductPackingEnglishName
                        string trafficTypeText = worksheet.Cells[row, 12]?.Text?.Trim() ?? "";
                        string customsProcedureText = worksheet.Cells[row, 13]?.Text?.Trim() ?? "";
                        string productPackingEnglishName = worksheet.Cells[row, 14]?.Text?.Trim() ?? "";

                        // Lookup ProductPacking by EnglishName
                        Guid? productPackingId = null;
                        if (!string.IsNullOrEmpty(productPackingEnglishName))
                        {
                            productPackingId = await VoyageService.GetProductPackingIdByEnglishName(productPackingEnglishName, token);
                        }

                        if (string.IsNullOrEmpty(billOfLadingNumber))
                        {
                            errors.Add($"سطر {row}: شماره بارنامه خالی است");
                            errorCount++;
                            continue;
                        }

                        Guid? companyId = null;
                        if (!string.IsNullOrEmpty(meliCode))
                        {
                            var company = await VoyageService.GetCompanyByMeliCode(meliCode, token);
                            if (company == null)
                            {
                                company = await VoyageService.CreateCompanyFromMeliCode(meliCode, token);
                            }
                            companyId = company?.Id;
                        }

                        TrafficType? trafficType = null;
                        if (!string.IsNullOrEmpty(trafficTypeText) && int.TryParse(trafficTypeText, out int trafficTypeCode))
                            trafficType = (TrafficType)trafficTypeCode;

                        CustomsProcedure? customsProcedure = null;
                        if (
                            !string.IsNullOrEmpty(customsProcedureText)
                            && int.TryParse(customsProcedureText, out int customsProcedureCode)
                            && customsProcedureCode >= 0
                        )
                            customsProcedure = (CustomsProcedure)customsProcedureCode;

                        var createDto = new CreateSeoFromVoyageWithDataDto
                        {
                            VoyageId = voyageId,
                            BillOfLadingNumber = billOfLadingNumber,
                            ProductOwnerCompanyId = companyId,
                            HSCode = hsCode,
                            Count = count,
                            ShipTonnage = weight,
                            Volume = volume,
                            ShipEntranceDateFa = shipEntranceDateFa,
                            PmoNumber = string.IsNullOrEmpty(pmoNumber) ? voyage.PmoNumber : pmoNumber,
                            Description = description,
                            Marks = marks,
                            TrafficType = trafficType,
                            CustomsProcedure = customsProcedure,
                            ProductPackingId = productPackingId,
                            ExistingSeoId = existingSeoId,
                            IsUpdate = isUpdate,
                        };

                        var result = await VoyageService.CreateOrUpdateSeoFromExcel(createDto, token);

                        if (string.IsNullOrEmpty(result.ErrorMessage))
                        {
                            if (isUpdate)
                                updatedCount++;
                            else
                                insertedCount++;
                        }
                        else
                        {
                            errors.Add($"سطر {row}: {result.ErrorMessage}");
                            errorCount++;
                        }
                    }
                    catch (Exception rowEx)
                    {
                        errors.Add($"سطر {row}: {rowEx.Message}");
                        errorCount++;
                    }
                }

                string resultMessage = $"عملیات با موفقیت انجام شد. ثبت جدید: {insertedCount} رکورد، بروزرسانی: {updatedCount} رکورد";
                if (errorCount > 0)
                {
                    resultMessage += $"، خطا: {errorCount} رکورد";
                }

                return Json(
                    new
                    {
                        success = true,
                        message = resultMessage,
                        inserted = insertedCount,
                        updated = updatedCount,
                        errors = errorCount,
                        errorDetails = errors.Take(10).ToList(),
                    }
                );
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"خطا در بارگذاری فایل Excel: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> DownloadVoyageSeoTemplate(Guid voyageId, CancellationToken token = default)
        {
            var headers = new List<string>
            {
                "شناسه (برای بروزرسانی حتماً وارد کنید - برای ایجاد جدید خالی بگذارید)",
                "شماره بارنامه *",
                "کد ملی شرکت صاحب کالا",
                "HS Code",
                "تعداد",
                "وزن (کیلوگرم)",
                "حجم",
                "تاریخ ورود (YYYY/MM/DD)",
                "شماره دریایی",
                "توضیحات",
                "MARKS",
                "نوع محموله (کد: 0=فله, 1=کالای عمومی, 2=کانتینری, 3=نفتی)",
                "رویه گمرکی (کد: 0=واردات, 1=صادرات, 2=ترانزیت داخلی, 3=ترانزیت خارجی, 4=ترانشیپ داخلی, 5=ترانشیپ خارجی, 6=کاپوتاژ, 7=برگشت از واردات, 8=برگشت از صادرات, 9=صادرات مجدد)",
                "بسته بندی (نام انگلیسی - مثال: BOX, PALLET, CONTAINER)",
            };

            // Use a sample English name for the template
            var samplePackingName = "BOX";

            var sampleData = new List<object[]>
            {
                new object[]
                {
                    "",
                    "BL-2024-001",
                    "1234567890",
                    "12345678",
                    "100",
                    "50000",
                    "100",
                    "1403/01/01",
                    "PMO-001",
                    "توضیحات نمونه",
                    "MARKS-001",
                    "1",
                    "0",
                    samplePackingName,
                },
            };

            var stream = await excelTools.GenerateExcelFileAsync(headers, sampleData, "VoyageSeoTemplate");
            if (stream == null)
            {
                return BadRequest("خطا در ایجاد قالب");
            }

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VoyageSeoTemplate.xlsx");
        }
        #endregion


        #region TaliGroupByDay
        public async Task<IActionResult> TaliGroupByDay(TaliGroupByDayFilterDto filterDto, Guid voyageId, CancellationToken token = default)
        {
            var dto = await VoyageTaliGroupService.GetTaliGroupedByDay(filterDto, token);

            dto.VoyageInfo = await VoyageService.GetVoyageInfo(filterDto.VoyageId, token);

            return View(dto);
        }

        #endregion
    }
}
