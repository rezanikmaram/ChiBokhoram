using System;
using System.Threading;
using System.Threading.Tasks;
using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.Entities.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class WarehouseReceiptController : Controller
    {
        public WarehouseReceiptController(
            IWarehouseReceiptManagerService warehouseReceiptManagerService,
            ISeoWarehouseReceiptReportService seoWarehouseReceiptReportService,
            ISplitSeoWarehouseReceiptService splitSeoWarehouseReceiptService,
            IMergeSeoWarehouseReceiptService mergeSeoWarehouseReceiptService,
            IChangeOwnershipSeoWarehouseReceiptService changeOwnershipSeoWarehouseReceiptService,
            IStripContainerSeoWarehouseReceiptService stripContainerSeoWarehouseReceiptService,
            ICurrentUserService<UserPanelInfoDto> currentUserService
        )
        {
            WarehouseReceiptManagerService = warehouseReceiptManagerService;
            SeoWarehouseReceiptReportService = seoWarehouseReceiptReportService;
            SplitSeoWarehouseReceiptService = splitSeoWarehouseReceiptService;
            MergeSeoWarehouseReceiptService = mergeSeoWarehouseReceiptService;
            ChangeOwnershipSeoWarehouseReceiptService = changeOwnershipSeoWarehouseReceiptService;
            StripContainerSeoWarehouseReceiptService = stripContainerSeoWarehouseReceiptService;
            CurrentUserService = currentUserService;
        }

        public IWarehouseReceiptManagerService WarehouseReceiptManagerService { get; }
        public ISeoWarehouseReceiptReportService SeoWarehouseReceiptReportService { get; }
        public ISplitSeoWarehouseReceiptService SplitSeoWarehouseReceiptService { get; }
        public IMergeSeoWarehouseReceiptService MergeSeoWarehouseReceiptService { get; }
        public IChangeOwnershipSeoWarehouseReceiptService ChangeOwnershipSeoWarehouseReceiptService { get; }
        public IStripContainerSeoWarehouseReceiptService StripContainerSeoWarehouseReceiptService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }

        public async Task<IActionResult> List(WarehouseReceiptFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await WarehouseReceiptManagerService.GetWarehouseReceiptManagerPage(filter, p ?? 1, token);
            return View(data);
        }

        public async Task<IActionResult> GetswrProductFullInfo(Guid swrProductId, CancellationToken token = default)
        {
            var data = await WarehouseReceiptManagerService.GetswrProductFullInfo(swrProductId, token);
            return PartialView("Partials/_ProductInfo", data);
        }

        public async Task<IActionResult> GetReceiptOutboundsByWarehouseReceiptId(Guid warehouseReceiptId, CancellationToken token = default)
        {
            var data = await WarehouseReceiptManagerService.GetAllWarehouseReceiptOutboundByReceiptId(warehouseReceiptId, token);
            return PartialView(data);
        }

        public async Task<IActionResult> GetProductDepositGroupingByCompany(
            WarehouseReceiptFilterDto filter = null,
            ReportType reportType = ReportType.Excel,
            CancellationToken token = default
        )
        {
            try
            {
                var f = new WarehouseReceiptReportGroupingFilterDto
                {
                    PortId = (await CurrentUserService.GetCurrentUser()).PortId,
                    EntityId = filter?.CompanyId,
                };

                var fileDto = await SeoWarehouseReceiptReportService.GetProductDepositGroupingByCompany(f, token, reportType);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", $"{fileDto.FileTitleWithExtention}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> GetProductDepositGroupingByProductCategory(
            WarehouseReceiptFilterDto filter = null,
            ReportType reportType = ReportType.Excel,
            CancellationToken token = default
        )
        {
            try
            {
                var f = new WarehouseReceiptReportGroupingFilterDto { PortId = (await CurrentUserService.GetCurrentUser()).PortId };

                var fileDto = await SeoWarehouseReceiptReportService.GetProductDepositGroupingByProductCategory(f, token, reportType);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", $"{fileDto.FileTitleWithExtention}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        public async Task<IActionResult> GetProductDepositGroupingByWarehouse(
            WarehouseReceiptFilterDto filter = null,
            ReportType reportType = ReportType.Excel,
            CancellationToken token = default
        )
        {
            try
            {
                var fileDto = await SeoWarehouseReceiptReportService.GetProductDepositGroupingByWarehouse(null, token, reportType);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", $"{fileDto.FileTitleWithExtention}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        #region تفکیک قبض انبار - Split
        public async Task<IActionResult> Split(Guid seoWarehouseReceiptId, Guid? seoId = null, CancellationToken token = default)
        {
            var data = await SplitSeoWarehouseReceiptService.GetPage(seoWarehouseReceiptId, token);
            ViewBag.seoId = seoId;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddUpdateProductWarehouseReceiptSplit(
            AddUpdateProductWarehouseReceiptSplitDto addUpdateProduct,
            Guid? seoId = null,
            CancellationToken token = default
        )
        {
            TempData["msg"] = await SplitSeoWarehouseReceiptService.AddUpdateProductWarehouseReceiptSplit(addUpdateProduct, token);

            return RedirectToAction(nameof(Split), new { seoWarehouseReceiptId = addUpdateProduct.MasterSeoWarehouseReceiptId, seoId });
        }

        public async Task<IActionResult> DeleteProductWarehouseReceiptSplit(
            Guid seoWarehouseReceiptProductId,
            Guid? seoId = null,
            CancellationToken token = default
        )
        {
            var seoWarehouseReceiptId = await SplitSeoWarehouseReceiptService.DeleteProduct(seoWarehouseReceiptProductId, token);

            return RedirectToAction(nameof(Split), new { seoWarehouseReceiptId, seoId });
        }

        [HttpPost]
        public async Task<IActionResult> EndSplit(Guid seoWarehouseReceiptId, Guid? seoId = null, CancellationToken token = default)
        {
            TempData["msg"] = await SplitSeoWarehouseReceiptService.EndSplit(seoWarehouseReceiptId, token);

            if (seoId != null)
                return RedirectToAction("SeoWarehouseReceipt", "Seo", new { seoId });
            else
                return RedirectToAction(nameof(List));
        }

        #endregion

        #region Merge - تجمیع قبض انبارها
        public async Task<IActionResult> Merge(
            Guid seoWarehouseReceiptId,
            Guid? seoId = null,
            Guid? exportId = null,
            CancellationToken token = default
        )
        {
            var data = await MergeSeoWarehouseReceiptService.GetPage(seoWarehouseReceiptId, token);
            ViewBag.seoId = seoId;
            ViewBag.exportId = exportId;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> MergeSeoWarehouseReceipt(
            string ids,
            Guid seoWarehouseReceiptId,
            Guid? seoId = null,
            Guid? exportId = null,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(ids))
            {
                var message = await MergeSeoWarehouseReceiptService.MergeSeoWarehouseReceipt(ids, token);
                TempData["msg"] = message;
                if (!string.IsNullOrWhiteSpace(message))
                    return RedirectToAction(
                        nameof(Merge),
                        new
                        {
                            seoWarehouseReceiptId,
                            seoId,
                            exportId,
                        }
                    );
            }

            if (seoId != null)
                return RedirectToAction("SeoWarehouseReceipt", "Seo", new { seoId });
            else if (exportId != null)
                return RedirectToAction("ExportWarehouseReceipt", "InlandExportManifest", new { exportId });
            else
                return RedirectToAction(nameof(List));
        }
        #endregion

        #region ChangeOwnership - تغییر مالکیت
        public async Task<IActionResult> ChangeOwnership(
            Guid seoWarehouseReceiptId,
            Guid? seoId = null,
            Guid? exportId = null,
            CancellationToken token = default
        )
        {
            var data = await ChangeOwnershipSeoWarehouseReceiptService.GetPage(seoWarehouseReceiptId, token);
            ViewBag.seoId = seoId;
            ViewBag.exportId = exportId;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> NewOwnership(
            WarehouseReceiptNewOwnershipDto newOwnership,
            Guid? seoId = null,
            Guid? exportId = null,
            CancellationToken token = default
        )
        {
            var message = await ChangeOwnershipSeoWarehouseReceiptService.NewOwnership(newOwnership, token);
            TempData["msg"] = message;
            if (!string.IsNullOrWhiteSpace(message))
                return RedirectToAction(
                    nameof(ChangeOwnership),
                    new
                    {
                        seoWarehouseReceiptId = newOwnership.WarehouseReceiptId,
                        seoId,
                        exportId,
                    }
                );

            if (exportId != null)
                return RedirectToAction("ExportWarehouseReceipt", "InlandExportManifest", new { exportId });
            else if (seoId != null)
                return RedirectToAction("SeoWarehouseReceipt", "Seo", new { seoId });
            else
                return RedirectToAction(nameof(List));
        }
        #endregion

        #region استریپ کانتینر
        public async Task<IActionResult> StripContainerList(Guid swrId, Guid? seoId = null, CancellationToken token = default)
        {
            var data = await StripContainerSeoWarehouseReceiptService.GetStripContainerListPage(swrId, token);

            if (
                data.WarehouseReceiptMaster.EntryRouteType == EntryRouteType.EntryFromLand
                && data.WarehouseReceiptMaster.InlandExportManifestId != null
            )
                ViewBag.exportId = data.WarehouseReceiptMaster.InlandExportManifestId;
            ViewBag.seoId = seoId;
            return View(data);
        }

        public async Task<IActionResult> StripContainer(Guid swrProductId, Guid? seoId = null, CancellationToken token = default)
        {
            var data = await StripContainerSeoWarehouseReceiptService.GetStripContainerPage(swrProductId, token);
            ViewBag.seoId = seoId;

            if (data.ProductInfo.InlandExportManifestId != null)
                ViewBag.exportId = data.ProductInfo.InlandExportManifestId;

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateProductFromStripContainer(
            AddContainerProductToSwrDto addProduct,
            Guid? seoId = null,
            CancellationToken token = default
        )
        {
            await StripContainerSeoWarehouseReceiptService.AddOrUpdateProduct(addProduct, token);

            return RedirectToAction(nameof(StripContainer), new { swrProductId = addProduct.SeoWarehouseReceiptProductId, seoId });
        }

        public async Task<IActionResult> DeleteProductFromStripContainer(Guid swrProductId, Guid? seoId = null, CancellationToken token = default)
        {
            var seoWarehouseReceiptId = await StripContainerSeoWarehouseReceiptService.DeleteProduct(swrProductId, token);

            return RedirectToAction(nameof(StripContainer), new { swrProductId = seoWarehouseReceiptId, seoId });
        }

        [HttpPost]
        public async Task<IActionResult> EndStrip(Guid seoWarehouseReceiptId, Guid? seoId = null, CancellationToken token = default)
        {
            TempData["msg"] = await StripContainerSeoWarehouseReceiptService.EndStrip(seoWarehouseReceiptId, token);

            if (seoId != null)
                return RedirectToAction("SeoWarehouseReceipt", "Seo", new { seoId });
            else
                return RedirectToAction(nameof(List));
        }
        #endregion

        #region توضیحات قبض انبار
        [HttpPost]
        public async Task<IActionResult> GetSwrDescription(Guid seoWarehouseReceiptId, CancellationToken token = default)
        {
            var swrDesc = await WarehouseReceiptManagerService.GetSwrDescription(seoWarehouseReceiptId, token);

            return PartialView("Partials/_SwrDescription", swrDesc);
        }

        [HttpPost]
        public async Task<IActionResult> SaveSwrDesciption(SwrDescriptionDto dto, CancellationToken token = default)
        {
            await WarehouseReceiptManagerService.SaveSwrDesciption(dto, token);

            return Ok();
        }

        #endregion
    }
}
