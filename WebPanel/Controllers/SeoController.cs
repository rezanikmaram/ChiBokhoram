using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.enumeration.Hirman;
using Common.Exceptions;
using Common.Utilities;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Concrete;
using WebPanel.Infrastructure;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd, FinanceUser")]
    public class SeoController : Controller
    {
        private readonly IShipService shipService;
        private readonly IWarehouseService warehouseService;
        private readonly IProductService productService;
        private readonly ICompanyService companyService;
        private readonly IWaterfrontService waterfrontService;
        private readonly ISeoService seoService;
        private readonly ILogger _logger;
        private readonly IExcelToolsService excelTools;

        public UserManager<ApplicationUser> UserManager { get; }
        public ISeoStopServiceLogService SeoStopServiceLogService { get; }
        public IPortService PortService { get; }
        public ISeoPublicService SeoPublicService { get; }
        public IPublicSeoService PublicSeoService { get; }
        public IProductPackingService ProductPackingService { get; }
        public ISeoWarehouseReceiptService SeoWarehouseReceiptService { get; }
        public ISeoWarehouseReceiptReportService SeoWarehouseReceiptReportService { get; }

        public SeoController(
            IShipService shipService,
            IWarehouseService warehouseService,
            IProductService productService,
            ICompanyService companyService,
            IWaterfrontService waterfrontService,
            ISeoService seoService,
            UserManager<ApplicationUser> userManager,
            ISeoStopServiceLogService seoStopServiceLogService,
            ILogger<SeoController> logger,
            IPortService portService,
            ISeoPublicService seoPublicService,
            IPublicSeoService publicSeoService,
            IProductPackingService productPackingService,
            ISeoWarehouseReceiptService seoWarehouseReceiptService,
            ISeoWarehouseReceiptReportService seoWarehouseReceiptReportService,
            IExcelToolsService excelTools
        )
        {
            this.shipService = shipService ?? throw new ArgumentNullException(nameof(shipService));
            this.warehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            this.productService = productService ?? throw new ArgumentNullException(nameof(productService));
            this.companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            this.waterfrontService = waterfrontService ?? throw new ArgumentNullException(nameof(waterfrontService));
            this.seoService = seoService ?? throw new ArgumentNullException(nameof(seoService));
            UserManager = userManager;
            SeoStopServiceLogService = seoStopServiceLogService;
            _logger = logger;
            PortService = portService;
            SeoPublicService = seoPublicService;
            PublicSeoService = publicSeoService;
            ProductPackingService = productPackingService;
            SeoWarehouseReceiptService = seoWarehouseReceiptService;
            SeoWarehouseReceiptReportService = seoWarehouseReceiptReportService;
            this.excelTools = excelTools;
        }

        public async Task<IActionResult> Index(SeoListFilterDto filter = null, int? p = null, CancellationToken token = default)
        {
            var cookieKey = FilterPersistenceHelper.Key(HttpContext, "Seo", "Index");

            if (HttpContext.Request.Method == "GET")
            {
                if (Request.Query.Count == 0)
                {
                    filter = FilterPersistenceHelper.Get<SeoListFilterDto>(HttpContext, cookieKey);
                }

                if (filter == null || filter?.StateInt == 0)
                {
                    filter = new SeoListFilterDto { StateInt = (int)SeoState.Active };
                }
            }
            else
            {
                if (filter != null)
                {
                    FilterPersistenceHelper.Save(HttpContext, filter, cookieKey);
                }
            }

            int pagenumaber = p ?? 1;
            var result = await seoService.GetSeo(pagenumaber, filter, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> ExportSeoListExcel(SeoListFilterDto filter, CancellationToken token = default)
        {
            filter ??= new SeoListFilterDto { StateInt = (int)SeoState.Active };

            var data = await PublicSeoService.GetSeoList(null, filter, token);
            var seos = data?.Data;
            if (seos == null || seos.Count == 0)
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
                .Data.Select(
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

            var stream = await excelTools.GenerateExcelFileAsync(headers, rows, "SeoList");
            if (stream == null)
                return Ok();

            var fileName = $"SeoList_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        public async Task<IActionResult> SeoAccessPage(Guid seoId, int returnPage = 0, CancellationToken token = default)
        {
            SeoAccessPageDto result = new();
            var data = await seoService.GetSeoInfoDto(seoId, token);

            result = new SeoAccessPageDto
            {
                SeoInfo = data,
                Companies = await companyService.GetCompanySelectList(token),
                FristStepSeo = await seoService.GetFirstStepSeo(seoId, token),
                AlloMinInfo = await SeoPublicService.GetAllocationEquMinInfoFromSeoId(seoId, token),
                Products = await productService.GetProductWithCategorySelectList(token),
            };

            ViewBag.IsEmpty = Services.Helper.ModelHelper.CheckEmpty(result.FristStepSeo); //todo: sjadi
            ViewBag.returnPage = returnPage;

            ViewBag.waterfront = await waterfrontService.GetWaterfrontDropDown(token);
            ViewBag.productPacking = await ProductPackingService.GetProductPackingSelectList(token);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetFirstStepSeo(FristStepSeoDto fristStepSeo, CancellationToken token = default)
        {
            try
            {
                if (fristStepSeo?.Id == Guid.Empty)
                {
                    ModelState.Remove("Id");
                }

                if (ModelState.IsValid)
                {
                    var result = await seoService.SetFirstStepSeo(fristStepSeo, token);
                    TempData["msg"] = result;
                    if (fristStepSeo?.Id == Guid.Empty)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction(nameof(SeoAccessPage), new { seoId = fristStepSeo.Id });
                    }
                }

                return RedirectToAction(nameof(SeoAccessPage), new { seoId = fristStepSeo.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError("SeoCreate: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> SeoPortOperatorPage(Guid seoId, CancellationToken token = default)
        {
            var result = await seoService.GetSeoInfoDto(seoId, token);

            return View(result);
        }

        public async Task<IActionResult> SeoAllocationPage(Guid seoId, CancellationToken token = default)
        {
            var result = await seoService.GetSeoAllocationEquPage(seoId, token);
            result.SecondStepSeo = await seoService.GetSecondStepSeo(seoId, token);
            result.Companies = await companyService.GetCompanySelectList(token);

            ViewBag.IsEmpty = Services.Helper.ModelHelper.CheckEmpty(result.SecondStepSeo);

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetSecondStepSeo(SecondStepSeoDto secondStepSeo, CancellationToken token = default)
        {
            try
            {
                if (secondStepSeo?.Id == Guid.Empty)
                {
                    ModelState.Remove("Id");
                }

                if (ModelState.IsValid)
                {
                    var result = await seoService.SetSecondStepSeo(secondStepSeo, token);
                    TempData["msg"] = result;
                    if (secondStepSeo?.Id == Guid.Empty)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction(nameof(SeoAllocationPage), new { seoId = secondStepSeo.Id });
                    }
                }

                return RedirectToAction(nameof(SeoAllocationPage), new { seoId = secondStepSeo.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError("SeoCreate: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> SeoView(Guid seoId, CancellationToken token = default)
        {
            // var dd = await seoService.GetSeoDto(seoId, token);
            var dto = await seoService.GetSeoDetail(seoId, token);
            return View(dto);
        }

        #region مدیریت کاربران CHO
        public async Task<IActionResult> SeoUsers(Guid seoId, CancellationToken token = default)
        {
            // var dd = await seoService.GetSeoDto(seoId, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await seoService.GetSeoCode(seoId, token);
            var dto = await seoService.SeoMobileUsers(seoId, token);
            return View(dto);
        }

        public async Task<IActionResult> ExitUserOnSeo(Guid personId, Guid seoId, CancellationToken token = default)
        {
            await seoService.ExitUserOnSeo(personId, seoId, token);
            return RedirectToAction(nameof(SeoUsers), new { seoId = seoId });
        }

        public async Task<IActionResult> SeoUserManage(Guid seoId, CancellationToken token = default)
        {
            var dto = await seoService.ManageSeoUser(new SeoMobileFilterDto { SeoId = seoId }, token);
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
            ViewBag.seoId = seoId;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SeoUserManage(SeoMobileFilterDto filterDto, Guid seoId, CancellationToken token = default)
        {
            if (filterDto == null)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات بارنامه وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(Index));
            }

            var dto = await seoService.ManageSeoUser(filterDto, token);
            ViewBag.seocode = await seoService.GetSeoCode(filterDto.SeoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(filterDto.SeoId, token);
            ViewBag.seoId = seoId;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserForSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.SetUserForSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(SeoUserManage), new { seoId = seoId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFormSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.RemoveUserFromSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(SeoUserManage), new { seoId = seoId });
        }
        #endregion

        #region مدیریت انبار ها
        public async Task<IActionResult> ManageSeoWareHouse(SeoWareHouseFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات بارنامه وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(Index));
            }

            var dto = await seoService.ManageSeoWareHouse(filterDto, token);
            ViewBag.seocode = await seoService.GetSeoCode(filterDto.SeoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(filterDto.SeoId, token);
            ViewBag.seoId = filterDto.SeoId;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetWareHouseForSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.SetWareHouseForSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(ManageSeoWareHouse), new { seoId = seoId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWareHouseFormSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.RemoveWareHouseFromSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(ManageSeoWareHouse), new { seoId = seoId });
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultWareHouse(Guid id, Guid seoId, CancellationToken token = default)
        {
            if (id.GuidHasValue() && seoId.GuidHasValue())
            {
                await seoService.SetDefaultWareHouse(id, seoId, token);
            }
            return RedirectToAction(nameof(ManageSeoWareHouse), new { seoId = seoId });
        }
        #endregion
        public async Task<IActionResult> GetWareHouseAsJson(Guid companyId, CancellationToken token = default)
        {
            var result = await warehouseService.GetWarehousDropDown(companyId, token);
            return Json(result);
        }

        public async Task<IActionResult> GetProductTypeAsJson(Guid categoryId, CancellationToken token = default)
        {
            var result = await productService.GetProductDropDown(categoryId, token);
            return Json(result);
        }

        public async Task<IActionResult> CreateSeoOld(Guid seoId, CancellationToken token = default)
        {
            var AllCompany = await companyService.GetCompanySelectList(token);
            ViewBag.drops = AllCompany;
            //ViewBag.site = await warehouseService.GetSiteDropDown(token);
            ViewBag.ship = await shipService.GetShipDropDown(token);
            ViewBag.productCategory = await productService.GetProductCategoryDropDown(token);
            ViewBag.waterfront = await waterfrontService.GetWaterfrontDropDown(token);
            //ViewBag.Ports = await PortService.GetPortDropDown(token);
            if (seoId.GuidHasValue())
            {
                var model = await seoService.GetCreateSeoDto(seoId, token);
                //ViewBag.Selectedwarehouses = await warehouseService.GetWarehousDropDown(
                //    model.WareHousCompanyId,
                //    token);
                return View(model);
            }

            //if (AllCompany?.WareHouse?.Count > 0 && AllCompany?.WareHouse[0]?.Id != null)
            //{
            //    ViewBag.Selectedwarehouses = await warehouseService.GetWarehousDropDown(
            //       AllCompany.WareHouse[0].Id.Value,
            //        token);
            //}

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSeoOld(CreateSeoDto dto, CancellationToken token = default)
        {
            try
            {
                if (dto?.Id == Guid.Empty)
                {
                    ModelState.Remove("Id");
                }

                if (ModelState.IsValid)
                {
                    var result = await seoService.AddOrUpdate(dto, token, null, true);
                    TempData["msg"] = result;
                    if (dto?.Id == Guid.Empty)
                    {
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return RedirectToAction(nameof(SeoAccessPage), new { seoId = dto.Id });
                    }
                }
                ViewBag.drops = await companyService.GetCompanySelectList(token);
                ViewBag.site = await warehouseService.GetSiteDropDown(token);
                ViewBag.ship = await shipService.GetShipDropDown(token);
                ViewBag.productCategory = await productService.GetProductCategoryDropDown(token);
                ViewBag.waterfront = await waterfrontService.GetWaterfrontDropDown(token);
                ViewBag.Ports = await PortService.GetPortDropDown(token);

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError("SeoCreate: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> CreateSeo(Guid voyageId, int returnPage = 0, CancellationToken token = default)
        {
            var result = await seoService.CreateSeo(voyageId, token);
            TempData["msg"] = result;

            if (returnPage == 0)
                return RedirectToAction(nameof(Index));
            else
                return RedirectToAction("VoyageSeo", "Voyage", new { voyageId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateSeoFromVoyage(
            CreateSeoFromVoyageDto createDto,
            int returnPage = 100,
            CancellationToken token = default
        )
        {
            if (createDto == null || !ModelState.IsValid)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "اطلاعات وارد شده صحیح نیست"));
                return RedirectToAction("VoyageSeo", "Voyage", new { voyageId = createDto?.VoyageId ?? Guid.Empty });
            }

            var (message, seoId) = await seoService.CreateSeoFromVoyage(createDto, token);
            TempData["msg"] = message;

            if (seoId == null)
                return RedirectToAction("VoyageSeo", "Voyage", new { voyageId = createDto.VoyageId });

            return RedirectToAction(nameof(SeoAccessPage), new { seoId, returnPage });
        }

        public async Task<IActionResult> EditSeo(Guid seoId, CancellationToken token = default)
        {
            var AllCompany = await companyService.GetCompanySelectList(token);
            ViewBag.drops = AllCompany;
            //ViewBag.site = await warehouseService.GetSiteDropDown(token);
            ViewBag.ship = await shipService.GetShipDropDown(token);
            ViewBag.productCategory = await productService.GetProductCategoryDropDown(token);
            ViewBag.waterfront = await waterfrontService.GetWaterfrontDropDown(token);
            //ViewBag.Ports = await PortService.GetPortDropDown(token);


            if (seoId.GuidHasValue())
            {
                var model = await seoService.GetEditSeoDto(seoId, token);
                //ViewBag.Selectedwarehouses = await warehouseService.GetWarehousDropDown(
                //    model.WareHousCompanyId,
                //    token);
                return View(model);
            }

            //if (AllCompany?.WareHouse?.Count > 0 && AllCompany?.WareHouse[0]?.Id != null)
            //{
            //    ViewBag.Selectedwarehouses = await warehouseService.GetWarehousDropDown(
            //       AllCompany.WareHouse[0].Id.Value,
            //        token);
            //}

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditSeo(EditSeoDto dto, CancellationToken token = default)
        {
            try
            {
                if (dto?.Id == Guid.Empty)
                {
                    ModelState.Remove("Id");
                }

                if (ModelState.IsValid)
                {
                    var result = await seoService.Update(dto, token);
                    TempData["msg"] = result;
                    return RedirectToAction(nameof(SeoAllocationPage), new { seoId = dto.Id });
                }
                ViewBag.drops = await companyService.GetCompanySelectList(token);
                ViewBag.site = await warehouseService.GetSiteDropDown(token);
                ViewBag.ship = await shipService.GetShipDropDown(token);
                ViewBag.productCategory = await productService.GetProductCategoryDropDown(token);
                ViewBag.waterfront = await waterfrontService.GetWaterfrontDropDown(token);
                ViewBag.Ports = await PortService.GetPortDropDown(token);

                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError("SeoCreate: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> DeleteSeo(Guid seoId, Guid? voyageId = null, CancellationToken token = default)
        {
            TempData["msg"] = await seoService.DeleteSeo(seoId, token);

            if (voyageId != null && voyageId != Guid.Empty)
                return RedirectToAction("VoyageSeo", "Voyage", new { voyageId = voyageId });

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ClosingSeo(Guid seoId, CancellationToken token = default)
        {
            if (seoId.GuidHasValue())
            {
                //var allo = await SeoPublicService.GetAllocationEquMinInfoFromSeoId(seoId, token);
                //if (allo == null || allo?.CanCloseSeo == false)
                //{
                //	TempData["msg"] = JsonConvert.SerializeObject(
                //		new ResponseMessage(ResponseMessageType.Error, "تخلیه و بارگیری به اتمام نرسیده است. امکان بستن بارنامه را ندارید")
                //	);
                //	return RedirectToAction(nameof(Index));
                //}

                ViewBag.seoCode = await seoService.GetSeoCode(seoId, token);
                ViewBag.seoId = seoId;
                return View();
            }

            // await seoService.ClosingSeo(seoId, token);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ClosingSeo(Guid seoId, string date, string timeEnd, CancellationToken token = default)
        {
            TempData["msg"] = await seoService.ClosingSeo(seoId, date, timeEnd, token);
            return RedirectToAction(nameof(Index));
        }

        #region توقف و از سرگیری توقف - گزارشات توقف
        public async Task<IActionResult> StartSeo(Guid seoId, CancellationToken token = default)
        {
            var userFind = await UserManager.GetUserAsync(User);
            TempData["msg"] = await seoService.StartSeo(userFind.Id, seoId, token);
            return RedirectToAction(nameof(SeoAccessPage), new { seoId });
        }

        public async Task<IActionResult> SeoStopServiceLog(Guid? seoId, int? p = null, CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoStopServiceLogService.GetSeoStopServiceLog(new SeoStopServiceLogFilterDto { SeoId = seoId }, pagenumaber, token);
            ViewBag.seoId = seoId;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SeoStopServiceLog(SeoStopServiceLogFilterDto filterDto, int? p = null, CancellationToken token = default)
        {
            var result = await SeoStopServiceLogService.GetSeoStopServiceLog(filterDto, p ?? 1, token);
            return View(result);
        }

        #endregion

        #region "مدیریت کامیون ها"
        public async Task<IActionResult> TruckManage(Guid seoId, CancellationToken token = default)
        {
            var dto = await seoService.ManageSeoTruck(new TruckManageFilterDto { SeoId = seoId }, token);
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
            ViewBag.seoId = seoId;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> TruckManage(TruckManageFilterDto filterDto, Guid seoId, CancellationToken token = default)
        {
            var dto = await seoService.ManageSeoTruck(filterDto, token);
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetTruckForSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.SetTruckForSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { seoId = seoId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTruckFormSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.RemoveTruckFromSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { seoId = seoId });
        }

        public async Task<IActionResult> ActivationTruck(Guid truckId, Guid seoId, CancellationToken token = default)
        {
            await seoService.ActivationTruck(seoId, truckId, token);
            return RedirectToAction(nameof(TruckManage), new { seoId = seoId });
        }
        #endregion

        #region مدیریت راننده ها
        public async Task<IActionResult> DriverManage(Guid seoId, CancellationToken token = default)
        {
            var dto = await seoService.ManageSeoDriver(new DriverManageFilterDto { SeoId = seoId }, token);
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
            ViewBag.seoId = seoId;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> DriverManage(DriverManageFilterDto filterDto, Guid seoId, CancellationToken token = default)
        {
            var dto = await seoService.ManageSeoDriver(filterDto, token);
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetDriverForSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.SetDriverForSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(DriverManage), new { seoId = seoId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveDriverFormSeo(string ids, Guid seoId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.RemoveDriverFromSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(DriverManage), new { seoId = seoId });
        }

        public async Task<IActionResult> ActivationDriver(Guid driverId, Guid seoId, CancellationToken token = default)
        {
            await seoService.ActivationDriver(seoId, driverId, token);
            return RedirectToAction(nameof(DriverManage), new { seoId = seoId });
        }

        #endregion

        #region "مدیریت تجهیزات"
        public async Task<IActionResult> EquipmentManage(SeoEquipmentFilterDto filterDto, int returnPage, CancellationToken token = default)
        {
            var result = await seoService.ManageSeoEquipment(filterDto, token);
            ViewBag.seocode = await seoService.GetSeoCode(filterDto.SeoId, token);
            ViewBag.seotype = await seoService.GetSeoTransportType(filterDto.SeoId, token);
            ViewBag.returnPage = returnPage;
            ViewBag.seoId = filterDto.SeoId;
            return View(result);
        }

        public async Task<IActionResult> GetSeoEquipmentPerformance(GetSeoEquipmentPerformanceFilter filter, CancellationToken token = default)
        {
            var result = await seoService.GetEquipmentPerformance(filter, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoEquipmentPerformanceFiltering(
            GetSeoEquipmentPerformanceFilter filter,
            CancellationToken token = default
        )
        {
            var result = await seoService.GetEquipmentPerformance(filter, token);
            return View(nameof(GetSeoEquipmentPerformance), result);
        }

        [HttpPost]
        public async Task<IActionResult> SetEquipmentForSeo(string ids, Guid seoId, int returnPage, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await seoService.SetEquipmentForSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { SeoId = seoId, returnPage = returnPage });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEquipmentFormSeo(string ids, Guid seoId, int returnPage, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                TempData["msg"] = await seoService.RemoveEquipmentFromSeo(ids, seoId, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { SeoId = seoId, returnPage = returnPage });
        }

        public async Task<IActionResult> ActivationEquipment(int equipmentId, Guid seoId, CancellationToken token = default)
        {
            await seoService.ActivationEquipment(seoId, equipmentId, token);
            return RedirectToAction(nameof(EquipmentManage), new { SeoId = seoId });
        }
        #endregion

        #region "مدیریت دستگاه های Rfid"
        public async Task<IActionResult> SeoRfidService(Guid seoId, CancellationToken token = default, int? p = null)
        {
            ViewBag.seoId = seoId;
            ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
            int pagenumaber = p ?? 1;
            if (seoId.GuidHasValue())
            {
                var result = await seoService.GetSeoRfidService(seoId, pagenumaber, token);
                return View(result);
            }
            return RedirectToAction(nameof(Index));
        }
        //public async Task<IActionResult> RfidDeviceManage(Guid seoId, CancellationToken token = default)
        //{
        //    //ViewData["Title"] = "مدیریت دستگاه های Rfid";
        //    var result = await seoService.ManageSeoRfidDevice(seoId, token);
        //    ViewBag.seocode = await seoService.GetSeoCode(seoId, token);
        //    ViewBag.seotype = await seoService.GetSeoTransportType(seoId, token);
        //    return View(result);
        //}

        //[HttpPost]
        //public async Task<IActionResult> SetRfidDeviceForSeo(int deviceId, string place,
        //    Guid seoId, CancellationToken token = default)
        //{
        //    var result = await seoService.SetRfidDeviceForSeo(deviceId, place, seoId, token);

        //    // return new ObjectResult(result);
        //    return PartialView("_rfidDeviceSelected", new RfidDeviceSelectedOfSeoDto
        //    {
        //        Selected = result,
        //        SeoId = seoId
        //    });
        //}

        //[HttpPost]
        //public async Task<IActionResult> RemoveRfidDeviceFormSeo(string ids, Guid seoId, CancellationToken token = default)
        //{
        //    if (!string.IsNullOrEmpty(ids))
        //    {
        //        await seoService.RemoveDeviceRfidFromSeo(ids, seoId, token);
        //    }
        //    return RedirectToAction(nameof(RfidDeviceManage), new { seoId = seoId });
        //}

        //public async Task<IActionResult> ActivationRfidDevice(int deviceId, Guid seoId, CancellationToken token = default)
        //{
        //    await seoService.ActivationRfidDevice(seoId, deviceId, token);
        //    return RedirectToAction(nameof(RfidDeviceManage), new { seoId = seoId });
        //}
        #endregion


        #region مدیریت کالا

        public async Task<IActionResult> ProductManage(Guid seoId, CancellationToken token = default)
        {
            var find = await seoService.GetProductManagePage(seoId, token);
            return View(find);
        }

        public async Task<IActionResult> ProductManageModal(Guid seoId, CancellationToken token = default)
        {
            ViewBag.IsModal = true;
            var find = await seoService.GetProductManagePage(seoId, token);
            return PartialView("_ProductManagePartial", find);
        }

        public async Task<IActionResult> GetSeoProductToSwrModal(Guid seoId, CancellationToken token = default)
        {
            var productManage = await seoService.GetProductManagePage(seoId, token);
            var seoInfo = productManage.SeoInfo;

            var model = new Entities.DTOs.Web.Hirman.SeoProductToSwrModalDto
            {
                SeoId = seoId,
                Products = productManage.SelectedProduct,
                TrafficType = seoInfo.TrafficType,
            };

            return PartialView("_SeoProductToSwr_Modal", model);
        }

        [HttpPost]
        public async Task<IActionResult> AddProductsToWarehouseReceipt(
            Guid seoId,
            List<Guid> productIds,
            Guid freightTariffId,
            Guid warehouseTariffId,
            CancellationToken token = default
        )
        {
            try
            {
                // TODO: Implement the actual logic to add products to warehouse receipt
                // This is where you'd call the appropriate service method
                return Json(new { success = true, message = "کالاها با موفقیت به انبار اضافه شدند" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding products to warehouse receipt");
                return Json(new { success = false, message = "خطا در ثبت کالاها" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToSeo(AddProductToSeoDto addProductToSeo, CancellationToken token = default)
        {
            await seoService.AddOrUpdateProductToSeo(addProductToSeo, token);

            return RedirectToAction(nameof(ProductManage), new { seoId = addProductToSeo.SeoId });
        }

        [HttpPost]
        public async Task<IActionResult> AddProductToSeoAjax(AddProductToSeoDto addProductToSeo, CancellationToken token = default)
        {
            await seoService.AddOrUpdateProductToSeo(addProductToSeo, token);

            return Json(new { seoId = addProductToSeo.SeoId });
        }

        public async Task<IActionResult> DeleteProductFromSeo(Guid seoProductId, CancellationToken token = default)
        {
            var seoId = await seoService.DeleteProductFromSeo(seoProductId, token);

            return RedirectToAction(nameof(ProductManage), new { seoId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductFromSeoAjax(Guid seoProductId, CancellationToken token = default)
        {
            var seoId = await seoService.DeleteProductFromSeo(seoProductId, token);

            return Json(new { seoId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoProductById(Guid seoProductId, CancellationToken token = default)
        {
            var find = await seoService.GetSeoProductById(seoProductId, token);
            if (find == null)
                return RedirectToAction(nameof(Index));

            return Json(find);
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoDefaultValues(Guid seoId, CancellationToken token = default)
        {
            var productManage = await seoService.GetProductManagePage(seoId, token);
            var seoInfo = productManage.SeoInfo;
            return Json(
                new
                {
                    productId = seoInfo.ProuductId?.ToString() ?? "",
                    count = seoInfo.Count, // Default count value
                    volume = seoInfo.Volume,
                    weightGross = seoInfo.ShipTonnage,
                    weightNet = seoInfo.ShipTonnage,
                    hsCode = seoInfo.HSCode,
                }
            );
        }

        [HttpPost]
        public async Task<IActionResult> CopySeoProduct(Guid seoProductId, int copyCount, CancellationToken token = default)
        {
            if (copyCount < 1 || copyCount > 100)
            {
                return Json(new { success = false, message = "تعداد کپی باید بین 1 تا 100 باشد" });
            }

            try
            {
                // Get the source product
                var sourceProduct = await seoService.GetSeoProductById(seoProductId, token);
                if (sourceProduct == null)
                {
                    return Json(new { success = false, message = "کالای مورد نظر یافت نشد" });
                }

                // Clear the Id to create new records
                sourceProduct.Id = Guid.Empty;

                // Create copies
                for (int i = 0; i < copyCount; i++)
                {
                    await seoService.AddOrUpdateProductToSeo(sourceProduct, token);
                }

                return Json(new { success = true, seoId = sourceProduct.SeoId });
            }
            catch (Exception ex)
            {
                _logger.LogError($"CopySeoProduct error: {ex.Message}");
                return Json(new { success = false, message = "خطا در کپی کردن کالا" });
            }
        }

        public async Task<IActionResult> ExportProductsToExcel(Guid seoId, CancellationToken token = default)
        {
            try
            {
                var productManage = await seoService.GetProductManagePage(seoId, token);
                var products = productManage.SelectedProduct;

                if (products == null || !products.Any())
                {
                    TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "هیچ کالایی برای خروجی یافت نشد"));
                    return RedirectToAction(nameof(ProductManage), new { seoId });
                }

                var headers = GetHeadersProduct();

                var rows = products
                    .Select(
                        (item, index) =>
                            new object[]
                            {
                                item.Id.ToString(), // ID for update matching - REQUIRED for updates
                                index + 1,
                                item.ProductTitle ?? "",
                                item.ProductHSCodeTitle ?? "",
                                item.CountTitle ?? "",
                                item.WeightGrossTitle ?? "",
                                item.WeightNetTitle ?? "",
                                item.VolumeTitle ?? "",
                                item.LargerWeightOrVolumeTitle ?? "",
                                item.PriceSpecialGearTariffTitle ?? "",
                                // Only export boolean values if they are true, otherwise empty (won't overwrite on re-import)
                                item.IsDangerous
                                    ? "بله"
                                    : "",
                                item.IsBigVolume ? "بله" : "",
                                item.IsUnusual ? "بله" : "",
                                item.IsRawMaterials ? "بله" : "",
                                item.IsNeedsSpecialEquipment ? "بله" : "",
                                item.IsSelfTransported ? "بله" : "",
                                item.ContainerNumber ?? "",
                                item.ContainerSize > 0 ? item.ContainerSize.ToString() : "",
                                item.ContainerSealNumber ?? "",
                                item.ContainerType ?? "",
                                item.VehicleIdentificationNumber ?? "",
                                item.Description ?? "",
                                item.NameFa ?? "",
                                item.NameEn ?? "",
                                item.ModelName ?? "",
                                item.ProductionYear?.ToString() ?? "",
                                item.Color ?? "",
                                item.Manufacturer ?? "",
                                (int)item.ProductCondition,
                            }
                    )
                    .ToList();

                var stream = await excelTools.GenerateExcelFileAsync(headers, rows, $"Products_{seoId}");
                if (stream == null)
                {
                    TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "خطا در ایجاد فایل Excel"));
                    return RedirectToAction(nameof(ProductManage), new { seoId });
                }

                var fileName = $"Products_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ExportProductsToExcel error: {ex.Message}");
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "خطا در خروجی Excel"));
                return RedirectToAction(nameof(ProductManage), new { seoId });
            }
        }

        private List<string> GetHeadersProduct()
        {
            return new List<string>
            {
                "شناسه (برای بروزرسانی خالی نگذارید)",
                "ردیف",
                "نام کالا",
                "HS Code",
                "تعداد",
                "وزن ناخالص",
                "وزن خالص",
                "حجم",
                "وزن/حجم",
                "قیمت تعرفه",
                "خطرناک",
                "ترافیکی",
                "نامتعارف",
                "مواد اولیه",
                "کالای اساسی",
                "خودران",
                "شماره کانتینر",
                "اندازه کانتینر",
                "پلمپ کانتینر",
                "نوع کانتینر",
                "شماره شاسی",
                "توضیحات",
                "نام فارسی",
                "نام انگلیسی",
                "مدل",
                "سال تولید",
                "رنگ",
                "شرکت سازنده",
                ProductConditionType.New.ToDisplayStringWithCode("وضعیت کالا"),
                "سریال موتور",
                "طول",
                "عرض",
                "ارتفاع",
                "نیاز به تجهیزات ویژه",
            };
        }

        public async Task<IActionResult> DownloadProductTemplate(Guid seoId, CancellationToken token = default)
        {
            try
            {
                var headers = GetHeadersProduct();

                var sampleData = new List<object[]>
                {
                    new object[]
                    {
                        "d76aac7d-1436-f011-a03a-99fc5bb88877",
                        "1",
                        "نمونه کالا",
                        "12345678",
                        "10",
                        "1000",
                        "950",
                        "5",
                        "1 تن",
                        "100000",
                        "بله",
                        "",
                        "",
                        "",
                        "",
                        "",
                        "CNT012345",
                        "40",
                        "SEAL0001",
                        "Standard",
                        "VIN0123456",
                        "توضیحات نمونه",
                        "نام فارسی نمونه",
                        "English Name Sample",
                        "Camry",
                        "2024",
                        "قرمز",
                        "تویوتا",
                        "0",
                        "",
                        "0",
                        "0",
                        "0",
                        "خیر",
                    },
                };

                var stream = await excelTools.GenerateExcelFileAsync(
                    headers,
                    sampleData,
                    "ProductTemplate",
                    textColumns: new List<int> { 17, 19, 21 }
                );
                if (stream == null)
                {
                    return BadRequest("خطا در ایجاد قالب");
                }

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProductTemplate.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError($"DownloadProductTemplate error: {ex.Message}");
                return BadRequest("خطا در دانلود قالب");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ImportProductsFromExcel(Guid seoId, IFormFile excelFile, CancellationToken token = default)
        {
            if (excelFile == null || excelFile.Length == 0)
            {
                return Json(new { success = false, message = "فایل Excel انتخاب نشده است" });
            }

            if (!seoId.GuidHasValue())
            {
                return Json(new { success = false, message = "شناسه بارنامه نامعتبر است" });
            }

            try
            {
                _logger.LogInformation($"Import started for SEO {seoId}, file: {excelFile.FileName}");

                // Read Excel file using EPPlus
                using (var stream = new MemoryStream())
                {
                    await excelFile.CopyToAsync(stream, token);
                    stream.Position = 0;

                    using (var package = new OfficeOpenXml.ExcelPackage(stream))
                    {
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

                        // Start from row 2 (skip header)
                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                // Read Id (Column 1) - this is the database ID for updates
                                string idValue = worksheet.Cells[row, 1]?.Text?.Trim() ?? "";
                                Guid? existingId = null;
                                bool hasValidId = false;

                                if (!string.IsNullOrEmpty(idValue))
                                {
                                    if (Guid.TryParse(idValue, out Guid parsedId))
                                    {
                                        existingId = parsedId;
                                        hasValidId = true;
                                    }
                                    else
                                    {
                                        errors.Add($"سطر {row}: شناسه '{idValue}' نامعتبر است. باید یک GUID معتبر باشد");
                                        errorCount++;
                                        continue;
                                    }
                                }

                                // Read other fields - Column mappings must match ExportProductsToExcel format (22 columns total):
                                // Col 1: ID, Col 2: RowNum, Col 3: ProductName, Col 4: HSCode
                                // Col 5-8: Numbers (Count, WeightGross, WeightNet, Volume)
                                // Col 9-10: Display fields (LargerWeightOrVolume, Price) - skip
                                // Col 11-16: Booleans (6 fields)
                                // Col 17-22: Text fields
                                string productName = worksheet.Cells[row, 3]?.Text?.Trim() ?? "";
                                string hsCode = worksheet.Cells[row, 4]?.Text?.Trim() ?? "";

                                if (string.IsNullOrEmpty(productName))
                                {
                                    errors.Add($"سطر {row}: نام کالا خالی است");
                                    errorCount++;
                                    continue;
                                }

                                // Parse numeric values (columns 5-8)
                                int.TryParse(worksheet.Cells[row, 5]?.Text ?? "1", out int count);
                                decimal.TryParse(worksheet.Cells[row, 6]?.Text ?? "0", out decimal weightGross);
                                decimal.TryParse(worksheet.Cells[row, 7]?.Text ?? "0", out decimal weightNet);
                                decimal.TryParse(worksheet.Cells[row, 8]?.Text ?? "0", out decimal volume);
                                // Note: columns 9-10 are display-only fields (LargerWeightOrVolume, Price) - skip these

                                // Parse boolean values (columns 11-16) - mapping to match export
                                // Col 11: خطرناک (IsDangerous), Col 12: ترافیکی (IsBigVolume), Col 13: نامتعارف (IsUnusual)
                                // Col 14: مواد اولیه (IsRawMaterials), Col 15: کالای اساسی (IsNeedsSpecialEquipment), Col 16: خودران (IsSelfTransported)
                                string isDangerousText = worksheet.Cells[row, 11]?.Text?.Trim();
                                string isBigVolumeText = worksheet.Cells[row, 12]?.Text?.Trim();
                                string isUnusualText = worksheet.Cells[row, 13]?.Text?.Trim();
                                string isRawMaterialsText = worksheet.Cells[row, 14]?.Text?.Trim();
                                string isNeedsSpecialEquipmentText = worksheet.Cells[row, 15]?.Text?.Trim();
                                string isSelfTransportedText = worksheet.Cells[row, 16]?.Text?.Trim();

                                // Text fields (columns 17-22)
                                string containerNumber = GetExcelCellText(worksheet.Cells[row, 17]); // Preserve leading zeros
                                int.TryParse(worksheet.Cells[row, 18]?.Text ?? "0", out int containerSize);
                                string containerSealNumber = GetExcelCellText(worksheet.Cells[row, 19]); // Preserve leading zeros
                                string containerType = worksheet.Cells[row, 20]?.Text?.Trim() ?? "";
                                string containerMoveType = "";
                                string vin = GetExcelCellText(worksheet.Cells[row, 21]); // Preserve leading zeros - VIN is column 21
                                string description = worksheet.Cells[row, 22]?.Text?.Trim() ?? "";

                                // Extra fields (columns 23-28)
                                string nameFa = worksheet.Cells[row, 23]?.Text?.Trim() ?? "";
                                string nameEn = worksheet.Cells[row, 24]?.Text?.Trim() ?? "";
                                string modelName = worksheet.Cells[row, 25]?.Text?.Trim() ?? "";
                                string productionYear = worksheet.Cells[row, 26]?.Text?.Trim() ?? "";
                                string color = worksheet.Cells[row, 27]?.Text?.Trim() ?? "";
                                string manufacturer = worksheet.Cells[row, 28]?.Text?.Trim() ?? "";
                                int.TryParse(worksheet.Cells[row, 29]?.Text ?? "0", out int productConditionType);

                                string engineSerial = worksheet.Cells[row, 30]?.Text?.Trim() ?? "";
                                decimal.TryParse(worksheet.Cells[row, 31]?.Text ?? "0", out decimal length);
                                decimal.TryParse(worksheet.Cells[row, 32]?.Text ?? "0", out decimal width);
                                decimal.TryParse(worksheet.Cells[row, 33]?.Text ?? "0", out decimal height);
                                string isNeedsSpecialEquipment = worksheet.Cells[row, 34]?.Text?.Trim();

                                // Try to find existing product by Id first
                                AddProductToSeoDto productDto = null;
                                bool isUpdate = false;

                                if (existingId.HasValue)
                                {
                                    productDto = await seoService.GetSeoProductById(existingId.Value, token);
                                    if (productDto != null)
                                    {
                                        // Security check: ensure the product belongs to current SEO
                                        if (productDto.SeoId != seoId)
                                        {
                                            errors.Add($"سطر {row}: شناسه '{existingId}' متعلق به بارنامه دیگری است. امکان بروزرسانی وجود ندارد");
                                            errorCount++;
                                            continue;
                                        }
                                        isUpdate = true;
                                    }
                                    else
                                    {
                                        // ID provided but not found in DB - treat as new record
                                        // This allows importing records with IDs that don't exist yet
                                        _logger.LogWarning($"Product ID {existingId} not found in DB, will create new record");
                                    }
                                }

                                // If not found by Id, try to find by Name + HSCode
                                if (productDto == null && !string.IsNullOrEmpty(hsCode))
                                {
                                    // Get all products for this SEO
                                    var productManagePage = await seoService.GetProductManagePage(seoId, token);
                                    var existingProducts = productManagePage?.SelectedProduct;

                                    // Search for existing product with same name and HSCode
                                    var matchedProduct = existingProducts?.FirstOrDefault(p =>
                                        (p.ProductTitle?.Equals(productName, StringComparison.OrdinalIgnoreCase) ?? false)
                                        && (p.HSCode?.Equals(hsCode, StringComparison.OrdinalIgnoreCase) ?? false)
                                    );

                                    if (matchedProduct != null)
                                    {
                                        productDto = await seoService.GetSeoProductById(matchedProduct.Id, token);
                                        if (productDto != null)
                                        {
                                            isUpdate = true;
                                        }
                                    }
                                }

                                // If still not found, create new
                                if (productDto == null)
                                {
                                    productDto = new AddProductToSeoDto();
                                    productDto.Id = Guid.Empty;
                                    productDto.SeoId = seoId;
                                    isUpdate = false;
                                }

                                // Update/Set values
                                productDto.SeoId = seoId;
                                productDto.Count = count < 1 ? 1 : count;
                                productDto.WeightGross = weightGross;
                                productDto.WeightNet = weightNet;
                                productDto.Volume = volume;
                                // Note: Length, Width, Height are not included in Excel export/import

                                // For text fields, only update if value is provided (preserve existing on empty for updates)
                                if (!string.IsNullOrEmpty(description) || !isUpdate)
                                    productDto.Description = description;
                                if (!string.IsNullOrEmpty(nameFa) || !isUpdate)
                                    productDto.NameFa = nameFa;
                                if (!string.IsNullOrEmpty(nameEn) || !isUpdate)
                                    productDto.NameEn = nameEn;
                                if (!string.IsNullOrEmpty(modelName) || !isUpdate)
                                    productDto.ModelName = modelName;
                                if (!string.IsNullOrEmpty(vin) || !isUpdate)
                                    productDto.VehicleIdentificationNumber = vin;
                                if (!string.IsNullOrEmpty(containerNumber) || !isUpdate)
                                    productDto.ContainerNumber = containerNumber;
                                if (containerSize > 0 || !isUpdate)
                                    productDto.ContainerSize = containerSize;
                                if (!string.IsNullOrEmpty(containerSealNumber) || !isUpdate)
                                    productDto.ContainerSealNumber = containerSealNumber;
                                if (!string.IsNullOrEmpty(containerType) || !isUpdate)
                                    productDto.ContainerType = containerType;
                                if (!string.IsNullOrEmpty(containerMoveType) || !isUpdate)
                                    productDto.ContainerMoveType = containerMoveType;
                                if (!string.IsNullOrEmpty(hsCode) || !isUpdate)
                                    productDto.HSCode = hsCode;

                                // Only update boolean fields if Excel cell has a value
                                // This prevents overwriting existing values with defaults when re-importing
                                if (!string.IsNullOrEmpty(isDangerousText))
                                    productDto.IsDangerous = ParseBooleanValue(isDangerousText);
                                if (!string.IsNullOrEmpty(isBigVolumeText))
                                    productDto.IsBigVolume = ParseBooleanValue(isBigVolumeText);
                                if (!string.IsNullOrEmpty(isUnusualText))
                                    productDto.IsUnusual = ParseBooleanValue(isUnusualText);
                                if (!string.IsNullOrEmpty(isRawMaterialsText))
                                    productDto.IsRawMaterials = ParseBooleanValue(isRawMaterialsText);
                                if (!string.IsNullOrEmpty(isNeedsSpecialEquipmentText))
                                    productDto.IsNeedsSpecialEquipment = ParseBooleanValue(isNeedsSpecialEquipmentText);
                                if (!string.IsNullOrEmpty(isSelfTransportedText))
                                    productDto.IsSelfTransported = ParseBooleanValue(isSelfTransportedText);

                                // New fields: ProductionYear and Color
                                if (!string.IsNullOrEmpty(modelName) || !isUpdate)
                                    productDto.ModelName = modelName;
                                if (!string.IsNullOrEmpty(productionYear) || !isUpdate)
                                    productDto.ProductionYear = productionYear;
                                if (!string.IsNullOrEmpty(color) || !isUpdate)
                                    productDto.Color = color;
                                if (!string.IsNullOrEmpty(manufacturer) || !isUpdate)
                                    productDto.Manufacturer = manufacturer;

                                // Note: ProductId and HSCodeId lookup is not implemented
                                // Users should manually set these in the UI after import
                                // or the system will use default values

                                // Calculate weight per product
                                if (productDto.WeightGross > 0 && productDto.Count > 0)
                                {
                                    productDto.WeightOneProduct = productDto.WeightGross / productDto.Count;
                                }

                                productDto.ProductCondition = (ProductConditionType)productConditionType;
                                productDto.EngineSerial = engineSerial;
                                productDto.Length = length; //طول
                                productDto.Width = width; //عرض
                                productDto.Height = height; //ارتفاع

                                if (!string.IsNullOrEmpty(isNeedsSpecialEquipment))
                                    productDto.IsNeedsSpecialEquipment = ParseBooleanValue(isNeedsSpecialEquipment);

                                // Save product
                                await seoService.AddOrUpdateProductToSeo(productDto, token);

                                if (isUpdate)
                                {
                                    updatedCount++;
                                    _logger.LogInformation($"Updated product '{productName}' (ID: {productDto.Id}) in SEO {seoId}");
                                }
                                else
                                {
                                    insertedCount++;
                                    _logger.LogInformation($"Inserted new product '{productName}' (ID: {productDto.Id}) in SEO {seoId}");
                                }
                            }
                            catch (Exception rowEx)
                            {
                                errors.Add($"سطر {row}: {rowEx.Message}");
                                errorCount++;
                                _logger.LogError($"Error processing row {row}: {rowEx.Message}");
                            }
                        }

                        string resultMessage = $"عملیات با موفقیت انجام شد. ";
                        resultMessage += $"ثبت جدید: {insertedCount} رکورد، ";
                        resultMessage += $"بروزرسانی: {updatedCount} رکورد";

                        if (errorCount > 0)
                        {
                            resultMessage += $"، خطا: {errorCount} رکورد";
                        }

                        _logger.LogInformation(
                            $"Import completed for SEO {seoId}. Inserted: {insertedCount}, Updated: {updatedCount}, Errors: {errorCount}"
                        );

                        return Json(
                            new
                            {
                                success = true,
                                message = resultMessage,
                                inserted = insertedCount,
                                updated = updatedCount,
                                errors = errorCount,
                                errorDetails = errors.Take(10).ToList(), // Return first 10 errors
                            }
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"ImportProductsFromExcel error: {ex.Message}");
                return Json(new { success = false, message = $"خطا در بارگذاری فایل Excel: {ex.Message}" });
            }
        }

        public async Task<IActionResult> UpdateDatabase(CancellationToken token = default)
        {
            await seoService.UpdateDatabase(token);

            return Ok();
        }

        [HttpPost]
        public async Task<JsonResult> GetSpecialGearTariff(AddProductToSeoDto addProductToSeo, CancellationToken token = default)
        {
            var result = await seoService.GetSpecialGearTariffPrice(addProductToSeo, token);

            return Json(result);
        }

        #endregion



        #region  مدیریت کالا درون کانتینر

        [HttpPost]
        public async Task<IActionResult> SetFirstContainerProductDataDto(
            SeoContainerProductFirstDataDto firstDataDto,
            CancellationToken token = default
        )
        {
            try
            {
                await seoService.SetFirstContainerProductDataDto(firstDataDto, token);

                return RedirectToAction(nameof(ContainerProductManage), new { seoProductId = firstDataDto.SeoProductId });
            }
            catch (Exception ex)
            {
                _logger.LogError("SetFirstContainerProductDataDto: " + ex.Message + "----" + ex.GetBaseException().Message);
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> ContainerProductManage(Guid seoProductId, CancellationToken token = default)
        {
            var find = await seoService.GetContainerProductManagePage(seoProductId, token);
            return View(find);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProductToSeoProduct(
            AddContainerProductToSeoProductDto addProductToSeoProduct,
            CancellationToken token = default
        )
        {
            await seoService.AddOrUpdateProductToSeoProduct(addProductToSeoProduct, token);

            return RedirectToAction(nameof(ContainerProductManage), new { seoProductId = addProductToSeoProduct.SeoProductId });
        }

        public async Task<IActionResult> DeleteProductFromSeoProduct(long seoContainerProductId, CancellationToken token = default)
        {
            var seoProductId = await seoService.DeleteProductFromSeoProduct(seoContainerProductId, token);

            return RedirectToAction(nameof(ContainerProductManage), new { seoProductId });
        }

        [HttpPost]
        public async Task<IActionResult> GetSeoContainerProductById(long seoContainerProductId, CancellationToken token = default)
        {
            var find = await seoService.GetSeoContainerProductById(seoContainerProductId, token);
            if (find == null)
                return RedirectToAction(nameof(Index));

            return Json(find);
        }

        #endregion


        #region قبض انبار
        public async Task<IActionResult> SeoWarehouseReceipt(Guid seoId, CancellationToken token = default)
        {
            var data = await SeoWarehouseReceiptService.GetPage(seoId, token);

            return View(data);
        }

        public async Task<IActionResult> GetSeoProducts(Guid seoId, CancellationToken token = default)
        {
            var data = await SeoWarehouseReceiptService.GetSeoProducts(seoId, token);

            return Json(data);
        }

        public async Task<IActionResult> GetAllProducts(CancellationToken token = default)
        {
            var data = await SeoWarehouseReceiptService.GetAllProducts(token);

            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProductToWarehouseReceipt(
            CreateSeoWarehouseReceiptProductDto addWarehouseReceiptProduct,
            CancellationToken token = default
        )
        {
            var products = await SeoWarehouseReceiptService.AddOrUpdateProductToWarehouseReceipt(addWarehouseReceiptProduct, token);
            return PartialView("_SeoWarehouseReceiptListProduct", products);
        }

        [HttpPost]
        public async Task<IActionResult> AddSelectedTaliToWarehouseReceipt(
            AddSelectedTaliToWarehouseReceiptDto model,
            CancellationToken token = default
        )
        {
            try
            {
                var products = await SeoWarehouseReceiptService.AddSelectedTaliToWarehouseReceipt(model, token);

                return PartialView("_SeoWarehouseReceiptListProduct", products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddSelectedTaliToWarehouseReceipt error: {ex.Message}");
                return Json(new { success = false, message = "خطا در افزودن تالی‌ها به قبض انبار" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddMultiProductToWarehouseReceipt(
            AddMultiProductToWarehouseReceiptDto model,
            CancellationToken token = default
        )
        {
            try
            {
                var products = await SeoWarehouseReceiptService.AddMultiProductToWarehouseReceipt(model, token);

                return PartialView("_SeoWarehouseReceiptListProduct", products);
            }
            catch (Exception ex)
            {
                _logger.LogError($"AddMultiProductToWarehouseReceipt error: {ex.Message}");
                return Json(new { success = false, message = "خطا در افزودن کالاها به قبض انبار" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWarehouseReceiptProduct(Guid seoWarehouseReceiptProductId, CancellationToken token = default)
        {
            var products = await SeoWarehouseReceiptService.DeleteProduct(seoWarehouseReceiptProductId, token);
            return PartialView("_SeoWarehouseReceiptListProduct", products);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateWarehouseReceipt(Guid seoId, CancellationToken token = default)
        {
            try
            {
                await SeoWarehouseReceiptService.SaveReportFile(seoId, token);
                return RedirectToAction(nameof(SeoWarehouseReceipt), new { seoId });
            }
            catch (Exception ex)
            {
                TempData["msg"] = new ResponseMessage(ResponseMessageType.Error, ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> GetFileWarehouseReceiptById(Guid seoWarehouseReceiptId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await SeoWarehouseReceiptService.ReadFile(seoWarehouseReceiptId, token);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", $"{fileDto.FileInfo.FileTitle}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> GetPreviewWarehouseReceipt(Guid seoWarehouseReceiptId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await SeoWarehouseReceiptReportService.GetFile(seoWarehouseReceiptId, token, isPreview: true);

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

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> BaskolData(CancellationToken token = default, int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await seoService.GetBaskolData(pagenumaber, token);

            return View(result);
        }

        public async Task<IActionResult> GetSeoSummary(GetSeoSummaryFilter filter, CancellationToken token = default)
        {
            var pageData = await SeoPublicService.GetSeoSummary(filter, token);

            return View(pageData);
        }

        /// <summary>
        /// Parses boolean value from string, supporting both English (true/false) and Persian (بله/خیر) formats
        /// Empty or null values will return false
        /// </summary>
        private bool ParseBooleanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            value = value.Trim().ToLower();

            // Check English values
            if (value == "true" || value == "1" || value == "yes")
                return true;

            // Check Persian values
            if (value == "بله" || value == "بلی" || value == "آری" || value == "yes fa")
                return true;

            // Everything else (including "false", "خیر", "no", etc.) returns false
            return false;
        }

        /// <summary>
        /// Gets text value from Excel cell, preserving leading zeros for specific columns
        /// </summary>
        private string GetExcelCellText(ExcelRange cell)
        {
            if (cell == null)
                return "";

            // Use Text property to preserve any leading zeros
            return cell.Text?.Trim() ?? "";
        }
    }
}
