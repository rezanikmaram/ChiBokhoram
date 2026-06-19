using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.DTOs.Web.Hirman.InlandExportManifest.InlandExportManifestCounterDetail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class WarehouseReceiptOutboundController : Controller
    {
        public IWarehouseReceiptOutboundManagerService _warehouseReceiptOutboundManagerService { get; }
        public IWarehouseReceiptOutboundReportService WarehouseReceiptOutboundReportService { get; }

        public WarehouseReceiptOutboundController(
            IWarehouseReceiptOutboundManagerService WarehouseReceiptOutboundManagerService,
            IWarehouseReceiptOutboundReportService warehouseReceiptOutboundReportService
        )
        {
            this._warehouseReceiptOutboundManagerService = WarehouseReceiptOutboundManagerService;
            WarehouseReceiptOutboundReportService = warehouseReceiptOutboundReportService;
        }

        public async Task<IActionResult> List(WarehouseReceiptOutboundFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await _warehouseReceiptOutboundManagerService.GetWarehouseReceiptOutboundManagerPage(filter, p ?? 1, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouseReceiptOutbound(CreateWarehouseReceiptOutboundDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty)
                ModelState.Remove("Id");
            if (ModelState.IsValid)
                await _warehouseReceiptOutboundManagerService.AddOrUpdate(dto, token);

            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> GetWarehouseReceiptOutboundId(Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetWarehouseReceiptOutboundId(warehouseReceiptOutboundId, token);
            return PartialView("_CreateWarehouseReceiptOutbound", model);
        }

        public async Task<IActionResult> GetWarehouseReceiptOutboundProducts(Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            var results = await _warehouseReceiptOutboundManagerService.GetWarehouseReceiptOutboundProducts(warehouseReceiptOutboundId, token);
            return View(results);
        }

        public async Task<IActionResult> GetWarehouseReceiptOutboundProductById(
            Guid warehouseReceiptOutboudnProductId,
            CancellationToken token = default
        )
        {
            var result = await _warehouseReceiptOutboundManagerService.GetWarehouseReceiptOuboundProductById(
                warehouseReceiptOutboudnProductId,
                token
            );
            return PartialView("_ProductFormPartial", result);
        }

        public async Task<IActionResult> Details(Guid wroId, CancellationToken token = default)
        {
            var results = await _warehouseReceiptOutboundManagerService.GetPageInfo(wroId, token);
            return View(results);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouseReceiptOutboundProduct(
            CreateSeoWarehouseReceiptOutboundProductDto createDto,
            CancellationToken token = default
        )
        {
            TempData["msg"] = await _warehouseReceiptOutboundManagerService.CreateOrUpdateWarehouseReceiptOutboundProduct(createDto, token);

            return RedirectToAction(
                nameof(GetWarehouseReceiptOutboundProducts),
                new { warehouseReceiptOutboundId = createDto.SeoWarehouseReceiptOutboundId }
            );
        }

        public async Task<IActionResult> GetSeoWarehouseReceiptByCompanyIdAsJson(Guid companyId, CancellationToken token = default)
        {
            var result = await _warehouseReceiptOutboundManagerService.GetSeoWarehouseReceiptByCompanyId(companyId, token);
            return Json(result);
        }

        #region DocFile
        public async Task<IActionResult> DocFiles(Guid wroId, CancellationToken token = default)
        {
            var model = await this._warehouseReceiptOutboundManagerService.GetDocPage(wroId, token);
            return View(model);
        }

        public async Task<IActionResult> UploadDoc(CreateWarehouseReceiptOutboundDocumentDto CreateDto, CancellationToken token = default)
        {
            await this._warehouseReceiptOutboundManagerService.AddDoc(CreateDto, token);
            return RedirectToAction(nameof(DocFiles), new { wroId = CreateDto.SeoWarehouseReceiptOutboundId });
        }

        [HttpPost]
        public async Task<IActionResult> GetWroDocFileById(long docId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await this._warehouseReceiptOutboundManagerService.GetFileById(docId, token);

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
        public async Task<IActionResult> RemoveWroDocFileById(Guid docId, Guid wroId, CancellationToken token = default)
        {
            await this._warehouseReceiptOutboundManagerService.RemoveDoc(docId, token);
            return RedirectToAction(nameof(DocFiles), new { wroId = wroId });
        }
        #endregion

        #region مدیریت  کاربران جهت خروج کالا
        public async Task<IActionResult> ManageUser(WroMobileFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.WarehouseReceiptOutboundId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            var dto = await _warehouseReceiptOutboundManagerService.ManageUser(filterDto, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserForWro(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _warehouseReceiptOutboundManagerService.SetUserForWro(ids, warehouseReceiptOutboundId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { warehouseReceiptOutboundId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromWro(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _warehouseReceiptOutboundManagerService.RemoveUserFromWro(ids, warehouseReceiptOutboundId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { warehouseReceiptOutboundId });
        }

        #endregion

        #region Truck
        public async Task<IActionResult> Trucks(Guid wroId, CancellationToken token = default)
        {
            if (wroId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            var dto = await _warehouseReceiptOutboundManagerService.GetTrucks(wroId, token);

            return View(dto);
        }

        public async Task<IActionResult> OutboundTalies(Guid wroId, CancellationToken token = default)
        {
            if (wroId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            var dto = await _warehouseReceiptOutboundManagerService.GetOutboundTalies(wroId, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateTruck(CreateWroTruckDto CreateDto, CancellationToken token = default)
        {
            if (CreateDto.CarPlatesType == Common.enumeration.CarPlatesType.Number)
            {
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesAreaCode));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesLeft));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesMiddle));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesRight));
            }
            else
                ModelState.Remove(nameof(CreateTruckDto.CarPlates));

            if (CreateDto.Id == Guid.Empty)
                ModelState.Remove("Id");
            if (ModelState.IsValid)
                await _warehouseReceiptOutboundManagerService.AddOrUpdateTruck(CreateDto, token);

            return RedirectToAction(nameof(Trucks), new { wroId = CreateDto.SeoWarehouseReceiptOutboundId });
        }

        public async Task<IActionResult> GetTruckById(Guid truckId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetTruckById(truckId, token);
            return PartialView("_CreateWarehouseReceiptOutboundTruck", model);
        }

        public async Task<IActionResult> DeleteTruck(Guid truckId, Guid wroId, CancellationToken token = default)
        {
            await _warehouseReceiptOutboundManagerService.DeleteTruck(truckId, token);

            return RedirectToAction(nameof(Trucks), new { wroId });
        }

        #endregion

        #region مدیریت  شناورها جهت خروج کالا
        public async Task<IActionResult> ManageVoyage(WroVoyageFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.WarehouseReceiptOutboundId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            var dto = await _warehouseReceiptOutboundManagerService.ManageVoyage(filterDto, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetVoyageForWro(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _warehouseReceiptOutboundManagerService.SetVoyageForWro(ids, warehouseReceiptOutboundId, token);
            }
            return RedirectToAction(nameof(ManageVoyage), new { warehouseReceiptOutboundId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveVoyageFromWro(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _warehouseReceiptOutboundManagerService.RemoveVoyageFromWro(ids, warehouseReceiptOutboundId, token);
            }
            return RedirectToAction(nameof(ManageVoyage), new { warehouseReceiptOutboundId });
        }

        public async Task<IActionResult> ActivationVoyage(Guid warehouseReceiptOutboundId, Guid voyageId, CancellationToken token = default)
        {
            await _warehouseReceiptOutboundManagerService.ActivationVoyage(warehouseReceiptOutboundId, voyageId, token);
            return RedirectToAction(nameof(ManageVoyage), new { warehouseReceiptOutboundId });
        }
        #endregion

        #region مدیریت تجهیزات
        public async Task<IActionResult> EquipmentManage(WroEquipmentFilterDto filterDto, int returnPage, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.WarehouseReceiptOutboundId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            var result = await _warehouseReceiptOutboundManagerService.ManageWroEquipment(filterDto, token);
            ViewBag.returnPage = returnPage;
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetEquipmentForWro(
            string ids,
            Guid warehouseReceiptOutboundId,
            int returnPage,
            bool usedInWaterfrontSide = false,
            bool usedInPortSide = false,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _warehouseReceiptOutboundManagerService.SetEquipmentForWro(
                    ids,
                    warehouseReceiptOutboundId,
                    usedInWaterfrontSide,
                    usedInPortSide,
                    token
                );
            }
            return RedirectToAction(nameof(EquipmentManage), new { WarehouseReceiptOutboundId = warehouseReceiptOutboundId, returnPage });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEquipmentFromWro(
            string ids,
            Guid warehouseReceiptOutboundId,
            int returnPage,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                TempData["msg"] = await _warehouseReceiptOutboundManagerService.RemoveEquipmentFromWro(ids, warehouseReceiptOutboundId, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { WarehouseReceiptOutboundId = warehouseReceiptOutboundId, returnPage });
        }

        public async Task<IActionResult> ActivationWroEquipment(Guid warehouseReceiptOutboundId, int equipmentId, CancellationToken token = default)
        {
            await _warehouseReceiptOutboundManagerService.ActivationWroEquipment(warehouseReceiptOutboundId, equipmentId, token);
            return RedirectToAction(nameof(EquipmentManage), new { WarehouseReceiptOutboundId = warehouseReceiptOutboundId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEquipmentSide(
            Guid warehouseReceiptOutboundId,
            int equipmentId,
            string side,
            bool isChecked,
            CancellationToken token = default
        )
        {
            await _warehouseReceiptOutboundManagerService.UpdateEquipmentSide(warehouseReceiptOutboundId, equipmentId, side, isChecked, token);
            return Json(new { success = true });
        }
        #endregion

        #region Bijak download
        public async Task<IActionResult> DownloadBijak(Guid outboundTaliId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await WarehouseReceiptOutboundReportService.GetBijakReport(outboundTaliId, token);

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

        public async Task<IActionResult> DownloadListBijak(Guid outboundId, CancellationToken token = default)
        {
            try
            {
                var fileDto = await WarehouseReceiptOutboundReportService.GetBijakListReport(outboundId, token);

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

        #region مجوز های بارگیری
        public async Task<IActionResult> LoadingLicense(Guid wroId, CancellationToken token = default)
        {
            var model = await this._warehouseReceiptOutboundManagerService.GetLoadingLicensePage(wroId, token);
            return View(model);
        }

        public async Task<IActionResult> CreateLoadingLicense(
            CreateWarehouseReceiptOutboundLoadingLicenseDto CreateDto,
            CancellationToken token = default
        )
        {
            await this._warehouseReceiptOutboundManagerService.CreateLoadingLicense(CreateDto, token);
            return RedirectToAction(nameof(LoadingLicense), new { wroId = CreateDto.SeoWarehouseReceiptOutboundId });
        }
        #endregion

        #region تالی های بارگیری به شناور
        public async Task<IActionResult> CompactExportTali(TaliExportCompactFilterDto filter, CancellationToken token = default)
        {
            if (filter == null || filter.WroId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(
                    new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات خروج کالا وارد صفحه شده اید")
                );
                return RedirectToAction(nameof(List));
            }

            if (filter.Page <= 0)
                filter.Page = 1;

            var model = await this._warehouseReceiptOutboundManagerService.GetCompactExportTali(filter, token);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> GetListCounterOneProduct(Guid swrProductId, bool step, int page = 1, int pageSize = 50, CancellationToken token = default)
        {
            var find = await this._warehouseReceiptOutboundManagerService.GetListCounterOneProduct(swrProductId, step, page, pageSize, token);

            if (step)
                return PartialView("_TaliList", find);
            else
                return PartialView("_TaliListWithVoyage", find);
        }

        [HttpPost]
        public async Task<IActionResult> GetCheckListFactorsPartial(Guid counterProductId, CancellationToken token)
        {
            var factors = await _warehouseReceiptOutboundManagerService.GetCounterProductCheckList(counterProductId, token);
            return PartialView("_CheckListFactors", factors);
        }

        [HttpPost]
        public async Task<IActionResult> GetEditHistoryPartial(Guid counterId, CancellationToken token)
        {
            var history = await _warehouseReceiptOutboundManagerService.GetCounterEditHistory(counterId, token);
            return PartialView("_EditHistory", history);
        }
        #endregion

        #region AJAX Endpoints for Modal Support

        [HttpGet]
        public async Task<IActionResult> GetSectionCounts(Guid wroId, CancellationToken token = default)
        {
            // Return empty counts - badges will be updated after each section load
            var counts = new
            {
                documents = 0,
                loadingLicenses = 0,
                products = 0,
                users = 0,
                trucks = 0,
                outboundTalies = 0,
                voyages = 0,
                equipment = 0,
            };
            return Json(new { success = true, counts });
        }

        #region Documents AJAX
        [HttpGet]
        public async Task<IActionResult> GetDocumentsPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetDocPage(wroId, token);
            return PartialView("_DocumentsPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDocumentFormPartial(Guid wroId, CancellationToken token = default)
        {
            var model = new CreateWarehouseReceiptOutboundDocumentDto { SeoWarehouseReceiptOutboundId = wroId };
            return PartialView("_DocumentFormPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> UploadDocAjax(CreateWarehouseReceiptOutboundDocumentDto createDto, CancellationToken token = default)
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.AddDoc(createDto, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWroDocFileByIdAjax(Guid docId, Guid wroId, CancellationToken token = default)
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.RemoveDoc(docId, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Loading Licenses AJAX
        [HttpGet]
        public async Task<IActionResult> GetLoadingLicensesPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetLoadingLicensePage(wroId, token);
            return PartialView("_LoadingLicensesPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetLoadingLicenseFormPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetLoadingLicensePage(wroId, token);
            return PartialView("_LoadingLicenseFormPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLoadingLicenseAjax(
            CreateWarehouseReceiptOutboundLoadingLicenseDto createDto,
            CancellationToken token = default
        )
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.CreateLoadingLicense(createDto, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Products AJAX
        [HttpGet]
        public async Task<IActionResult> GetProductsPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetWarehouseReceiptOutboundProducts(wroId, token);
            return PartialView("_ProductsPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductFormPartial(Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetProductFormByWroId(warehouseReceiptOutboundId, token);
            return PartialView("_ProductFormPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetProductMultiFormPartial(Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetProductMultiFormByWroId(warehouseReceiptOutboundId, token);
            return PartialView("_ProductMultiFormPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouseReceiptOutboundProductAjax(
            CreateSeoWarehouseReceiptOutboundProductDto createDto,
            CancellationToken token = default
        )
        {
            try
            {
                if (!ModelState.IsValid)
                    return Json(new { success = false, message = "مقادیر وارد شده معتبر نیستند" });
                await _warehouseReceiptOutboundManagerService.CreateOrUpdateWarehouseReceiptOutboundProduct(createDto, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouseReceiptOutboundMultiProductAjax(
            SwrOutboundMultiProductSaveDto createDto,
            CancellationToken token = default
        )
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.CreateOrUpdateWarehouseReceiptOutboundMultiProduct(createDto, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductAjax(Guid productId, CancellationToken token = default)
        {
            try
            {
                var result = await _warehouseReceiptOutboundManagerService.DeleteWarehouseReceiptOutboundProduct(productId, token);
                var response = JsonConvert.DeserializeObject<ResponseMessage>(result);
                return Json(new { success = response.Type == ResponseMessageType.Success, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion

        #region Users AJAX
        [HttpGet]
        public async Task<IActionResult> GetUsersPartial(Guid wroId, string search = null, int? userType = null, CancellationToken token = default)
        {
            var filterDto = new WroMobileFilterDto
            {
                WarehouseReceiptOutboundId = wroId,
                Search = search,
                UserMobileType = userType.HasValue ? (UserMobileType?)userType.Value : null,
            };
            var model = await _warehouseReceiptOutboundManagerService.ManageUser(filterDto, token);
            return PartialView("_UsersPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserForWroAjax(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.SetUserForWro(ids, warehouseReceiptOutboundId, token);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromWroAjax(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.RemoveUserFromWro(ids, warehouseReceiptOutboundId, token);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Trucks AJAX
        [HttpGet]
        public async Task<IActionResult> GetTrucksPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetTrucks(wroId, token);
            return PartialView("_TrucksPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> GetTruckFormPartial(Guid wroId, CancellationToken token = default)
        {
            var trucksData = await _warehouseReceiptOutboundManagerService.GetTrucks(wroId, token);
            // Return the CreatePageDto from the trucks page data
            return PartialView("_TruckFormPartial", trucksData.CreatePageDto);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateTruckAjax(CreateWroTruckDto createDto, CancellationToken token = default)
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.AddOrUpdateTruck(createDto, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTruckAjax(Guid truckId, Guid wroId, CancellationToken token = default)
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.DeleteTruck(truckId, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Voyages AJAX
        [HttpGet]
        public async Task<IActionResult> GetVoyagesPartial(Guid wroId, CancellationToken token = default)
        {
            var filterDto = new WroVoyageFilterDto { WarehouseReceiptOutboundId = wroId };
            var model = await _warehouseReceiptOutboundManagerService.ManageVoyage(filterDto, token);
            return PartialView("_VoyagesPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> SetVoyageForWroAjax(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.SetVoyageForWro(ids, warehouseReceiptOutboundId, token);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveVoyageFromWroAjax(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.RemoveVoyageFromWro(ids, warehouseReceiptOutboundId, token);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivationVoyageAjax(Guid warehouseReceiptOutboundId, Guid voyageId, CancellationToken token = default)
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.ActivationVoyage(warehouseReceiptOutboundId, voyageId, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Equipment AJAX
        [HttpGet]
        public async Task<IActionResult> GetEquipmentPartial(
            Guid wroId,
            string search = null,
            int? equipmentType = null,
            CancellationToken token = default
        )
        {
            var filterDto = new WroEquipmentFilterDto
            {
                WarehouseReceiptOutboundId = wroId,
                Search = search,
                EquipmentType = equipmentType.HasValue ? (EquipmentType?)equipmentType.Value : null,
            };
            var model = await _warehouseReceiptOutboundManagerService.ManageWroEquipment(filterDto, token);
            return PartialView("_EquipmentPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> SetEquipmentForWroAjax(
            string ids,
            Guid warehouseReceiptOutboundId,
            bool usedInWaterfrontSide = false,
            bool usedInPortSide = false,
            CancellationToken token = default
        )
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.SetEquipmentForWro(
                        ids,
                        warehouseReceiptOutboundId,
                        usedInWaterfrontSide,
                        usedInPortSide,
                        token
                    );
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEquipmentFromWroAjax(string ids, Guid warehouseReceiptOutboundId, CancellationToken token = default)
        {
            try
            {
                if (!string.IsNullOrEmpty(ids))
                {
                    await _warehouseReceiptOutboundManagerService.RemoveEquipmentFromWro(ids, warehouseReceiptOutboundId, token);
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivationWroEquipmentAjax(
            Guid warehouseReceiptOutboundId,
            int equipmentId,
            CancellationToken token = default
        )
        {
            try
            {
                await _warehouseReceiptOutboundManagerService.ActivationWroEquipment(warehouseReceiptOutboundId, equipmentId, token);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        #region Outbound Talies AJAX
        [HttpGet]
        public async Task<IActionResult> GetOutboundTaliesPartial(Guid wroId, CancellationToken token = default)
        {
            var model = await _warehouseReceiptOutboundManagerService.GetOutboundTalies(wroId, token);
            return PartialView("_OutboundTaliesPartial", model);
        }
        #endregion

        #region Compact Export AJAX
        [HttpGet]
        public async Task<IActionResult> GetCompactExportPartial(Guid wroId, int page = 1, CancellationToken token = default)
        {
            var filter = new TaliExportCompactFilterDto { WroId = wroId, Page = page };
            var model = await _warehouseReceiptOutboundManagerService.GetCompactExportTali(filter, token);
            return PartialView("_CompactExportPartial", model);
        }
        #endregion

        #region Counter Details AJAX
        [HttpGet]
        public async Task<IActionResult> GetCounterDetailPartial(Guid wroId, CancellationToken token = default)
        {
            var filter = new InlandExportManifestCounterDetailFilterDto { SeoWarehouseReceiptOutboundId = wroId };
            var model = await _warehouseReceiptOutboundManagerService.GetCounterDetailByWroId(filter, token);

            return PartialView("_CounterDetailPartial", model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadCounterDetailExcel(Guid wroId, CancellationToken token = default)
        {
            var filter = new InlandExportManifestCounterDetailFilterDto { SeoWarehouseReceiptOutboundId = wroId };
            var stream = await _warehouseReceiptOutboundManagerService.GetCounterDetailExcel(filter, token);
            if (stream == null)
                return Ok();

            var fileName = $"CounterDetail_{wroId}_{DateTimeOffset.Now:yyyyMMddHHmmss}.xlsx";
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GetCounterGears(Guid counterId, CancellationToken token = default)
        {
            var items = await _warehouseReceiptOutboundManagerService.GetCounterGears(counterId, token);
            return PartialView("_GetCounterGears", items);
        }

        [HttpPost]
        public async Task<IActionResult> GetCounterFiles(Guid counterId, CancellationToken token = default)
        {
            var items = await _warehouseReceiptOutboundManagerService.GetCounterFiles(counterId, token);
            return PartialView("_GetCounterFiles", items);
        }

        public async Task<IActionResult> CounterFileDownload(Guid counterProductId, string fileName, CancellationToken token = default)
        {
            try
            {
                var fileDto = await _warehouseReceiptOutboundManagerService.GetCounterFileStream(counterProductId, fileName, token);

                if (fileDto == null)
                    return Ok();

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileInfo.FileTitle);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }
        #endregion

        [HttpGet]
        public async Task<IActionResult> UpdateOutboundMethodTypes(CancellationToken token = default)
        {
            var result = await _warehouseReceiptOutboundManagerService.UpdateAllOutboundMethodTypes(token);
            return Content(result);
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> ChangeCounterValidationStatus(ChangeCounterValidationStatusDto dto, CancellationToken token = default)
        {
            var result = await _warehouseReceiptOutboundManagerService.ChangeCounterValidationStatus(dto, token);
            return Json(new { success = result.Success, message = result.Message });
        }
    }
}
