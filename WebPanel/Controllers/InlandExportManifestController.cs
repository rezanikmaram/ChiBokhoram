using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Exceptions;
using DNTPersianUtils.Core;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.DTOs.Web.Hirman.InlandExportManifest.InlandExportManifestCounterDetail;
using Entities.DTOs.Web.Hirman.InlandExportManifest.WarehouseReceipt;
using Entities.Entities.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    //مجوز ها و بارنامه های از راه خشکی
    [Authorize(Roles = "admin, adminOnePort")]
    public class InlandExportManifestController : Controller
    {
        public IInlandExportManifestService InlandExportManifestService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }
        public IInlandExportManifestTruckService ExportTruckService { get; }

        public InlandExportManifestController(
            IInlandExportManifestService inlandExportManifestService,
            ICurrentUserService<UserPanelInfoDto> currentUserService,
            IInlandExportManifestTruckService exportTruckService
        )
        {
            InlandExportManifestService = inlandExportManifestService;
            CurrentUserService = currentUserService;
            ExportTruckService = exportTruckService;
        }

        [Authorize(Roles = "admin, adminOnePort")]
        public async Task<IActionResult> Index(InlandExportManifestFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterSpecialIncome(filter);

            var data = await InlandExportManifestService.GetPage(filter, token);

            return View(data);
        }

        private async Task ConfigFilterSpecialIncome(InlandExportManifestFilterDto filter)
        {
            var user = await CurrentUserService.GetCurrentUser();
            var now = DateTimeOffset.Now.ToShortPersianDateString();
            var dayStart = DateTimeOffset.Now.AddDays(-30).ToShortPersianDateString();

            filter ??= new InlandExportManifestFilterDto { Page = 1 };
            filter.FilterDate ??= new FilterDateDto { FromDate = dayStart, ToDate = now };
            filter.PortId = user.PortId;
        }

        [HttpPost]
        public async Task<IActionResult> CreateInlandExportManifest(CreateInlandExportManifestDto createDto, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                var result = await InlandExportManifestService.CreateInlandExportManifest(createDto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                    return View(createDto);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Details(Guid id, CancellationToken token = default)
        {
            var data = await InlandExportManifestService.GetDetail(id, token);

            ViewBag.IsEmpty = Services.Helper.ModelHelper.CheckEmpty(data.EditDto);

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EditInlandExport(InlandExportEditDto editDto, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                var result = await InlandExportManifestService.EditInlandExport(editDto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                }
                return RedirectToAction(nameof(Details), new { id = editDto.Id });
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> ManageWarehouse(
            Guid inlandExportManifestId,
            InlandExportManifestWareHouseFilterDto filterDto,
            CancellationToken token = default
        )
        {
            if (filterDto == null)
            {
                filterDto = new InlandExportManifestWareHouseFilterDto { InlandExportManifestId = inlandExportManifestId };
            }
            else
            {
                filterDto.InlandExportManifestId = inlandExportManifestId;
            }

            var data = await InlandExportManifestService.GetWarehouseManagerPage(filterDto, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SetWareHouseForInlandExportManifest(
            Guid inlandExportManifestId,
            string ids,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await InlandExportManifestService.SetWareHouseForInlandExportManifest(inlandExportManifestId, ids, token);
            }
            return RedirectToAction(nameof(ManageWarehouse), new { inlandExportManifestId = inlandExportManifestId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveWareHouseFromInlandExportManifest(
            Guid inlandExportManifestId,
            string ids,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await InlandExportManifestService.RemoveWareHouseFromInlandExportManifest(inlandExportManifestId, ids, token);
            }
            return RedirectToAction(nameof(ManageWarehouse), new { inlandExportManifestId = inlandExportManifestId });
        }

        [HttpPost]
        public async Task<IActionResult> SetDefaultWareHouse(Guid id, Guid inlandExportManifestId, CancellationToken token = default)
        {
            await InlandExportManifestService.SetDefaultWareHouse(id, inlandExportManifestId, token);
            return RedirectToAction(nameof(ManageWarehouse), new { inlandExportManifestId = inlandExportManifestId });
        }

        #region "مدیریت کامیون ها"


        public async Task<IActionResult> TruckManage(TruckExportManageFilterDto filterDto, CancellationToken token = default)
        {
            var dto = await ExportTruckService.ManageTruck(filterDto, token);
            if (dto?.Info == null)
                return RedirectToAction(nameof(Index));
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetTruckForExport(string ids, Guid exportId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await ExportTruckService.SetTruckForExport(ids, exportId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { exportId = exportId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTruckFromExport(string ids, Guid exportId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await ExportTruckService.RemoveTruckFromExport(ids, exportId, token);
            }
            return RedirectToAction(nameof(TruckManage), new { exportId = exportId });
        }

        public async Task<IActionResult> ActivationTruck(Guid truckId, Guid exportId, CancellationToken token = default)
        {
            await ExportTruckService.ActivationTruck(exportId, truckId, token);
            return RedirectToAction(nameof(TruckManage), new { exportId = exportId });
        }
        #endregion

        #region مدیریت  کاربران موبایل صادرات
        public async Task<IActionResult> ManageUser(ExportMobileFilterDto filterDto, CancellationToken token = default)
        {
            if (filterDto == null || filterDto.ExportId == Guid.Empty)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "بدون مشخصات مجوز وارد صفحه شده اید"));
                return RedirectToAction(nameof(Index));
            }

            var dto = await InlandExportManifestService.ManageExportUser(filterDto, token);

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> SetUserForExport(string ids, Guid exportId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await InlandExportManifestService.SetUserForExport(ids, exportId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { exportId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveUserFromExport(string ids, Guid exportId, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await InlandExportManifestService.RemoveUserFromExport(ids, exportId, token);
            }
            return RedirectToAction(nameof(ManageUser), new { exportId });
        }

        #endregion

        #region "مدیریت تجهیزات"
        public async Task<IActionResult> EquipmentManage(
            InlandExportManifestEquipmentFilterDto filterDto,
            int returnPage,
            CancellationToken token = default
        )
        {
            var result = await InlandExportManifestService.ManageInlandExportManifestEquipment(filterDto, token);
            ViewBag.returnPage = returnPage;

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> SetEquipmentForInlandExportManifest(
            string ids,
            Guid inlandExportManifestId,
            int returnPage,
            bool usedInWaterfrontSide = false,
            bool usedInPortSide = false,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await InlandExportManifestService.SetEquipmentForInlandExportManifest(ids, inlandExportManifestId, usedInWaterfrontSide, usedInPortSide, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { inlandExportManifestId = inlandExportManifestId, returnPage = returnPage });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveEquipmentFormInlandExportManifest(
            string ids,
            Guid inlandExportManifestId,
            int returnPage,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                TempData["msg"] = await InlandExportManifestService.RemoveEquipmentFromInlandExportManifest(ids, inlandExportManifestId, token);
            }
            return RedirectToAction(nameof(EquipmentManage), new { inlandExportManifestId = inlandExportManifestId, returnPage = returnPage });
        }

        public async Task<IActionResult> ActivationEquipment(int equipmentId, Guid inlandExportManifestId, CancellationToken token = default)
        {
            await InlandExportManifestService.ActivationEquipment(inlandExportManifestId, equipmentId, token);
            return RedirectToAction(nameof(EquipmentManage), new { inlandExportManifestId = inlandExportManifestId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateEquipmentSide(Guid inlandExportManifestId, int equipmentId, string side, bool isChecked, CancellationToken token = default)
        {
            await InlandExportManifestService.UpdateEquipmentSide(inlandExportManifestId, equipmentId, side, isChecked, token);
            return Ok();
        }
        #endregion

        #region Counter - بارشماری
        public async Task<IActionResult> CompactCounter(ExportCounterCompactFilterDto filter, int returnPage = 0, CancellationToken token = default)
        {
            var data = await InlandExportManifestService.GetCompactCounter(filter, token);
            ViewBag.returnPage = returnPage;

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> GetListTaliOneProduct(Guid? seoProductId, CancellationToken token = default)
        {
            var find = await InlandExportManifestService.GetListTaliOneProduct(seoProductId, token);

            return PartialView("_TaliListWithWarehouse", find);
        }

        [HttpGet]
        public async Task<IActionResult> GetTaliGearList(Guid inlandExportManifestId, Guid? seoProductId, CancellationToken token = default)
        {
            var page = await InlandExportManifestService.GetTaliGearsList(inlandExportManifestId, seoProductId, token);
            return PartialView("_TaliGearList", page);
        }

        public async Task<IActionResult> CounterDetail(InlandExportManifestCounterDetailFilterDto filter, int returnPage = 0, CancellationToken token = default)
        {
            var data = await InlandExportManifestService.GetCounterDetail(filter, token);
            
            ViewBag.returnPage = returnPage;

            return View(data);
        }
        #endregion

        #region قبض انبار
        public async Task<IActionResult> ExportWarehouseReceipt(Guid exportId, CancellationToken token = default)
        {
            try
            {
                var data = await InlandExportManifestService.GetExportWarehouseReceiptPage(exportId, token);

                return View(data);
            }
            catch (Exception ex)
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, ex.Message));
                return RedirectToAction(nameof(Details), new { Id = exportId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProductToWarehouseReceipt(
            CreateSeoWarehouseReceiptProductDto addWarehouseReceiptProduct,
            CancellationToken token = default
        )
        {
            var products = await InlandExportManifestService.AddOrUpdateProductToWarehouseReceipt(addWarehouseReceiptProduct, token);
            return PartialView("_ExportWarehouseReceiptListProduct", products);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteWarehouseReceiptProduct(Guid seoWarehouseReceiptProductId, CancellationToken token = default)
        {
            var products = await InlandExportManifestService.DeleteWarehouseReceiptProduct(seoWarehouseReceiptProductId, token);
            return PartialView("_ExportWarehouseReceiptListProduct", products);
        }

        [HttpPost]
        public async Task<IActionResult> AddSelectedTaliToWarehouseReceipt(
            AddSelectedTaliToWarehouseReceiptExportDto model,
            CancellationToken token = default
        )
        {
            try
            {
                var products = await InlandExportManifestService.AddSelectedTaliToWarehouseReceipt(model, token);

                return PartialView("_ExportWarehouseReceiptListProduct", products);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در افزودن تالی‌ها به قبض انبار" });
            }
        }

        //[HttpPost]
        //public async Task<IActionResult> GenerateWarehouseReceipt(Guid exportId, CancellationToken token = default)
        //{
        //	try
        //	{
        //		await InlandExportManifestService.SaveReportFile(exportId, token);
        //		return RedirectToAction(nameof(ExportWarehouseReceipt), new { exportId });
        //	}
        //	catch (Exception ex)
        //	{

        //		TempData["msg"] = new ResponseMessage(ResponseMessageType.Error, ex.Message);
        //		throw;
        //	}

        //}
        #endregion
    }
}
