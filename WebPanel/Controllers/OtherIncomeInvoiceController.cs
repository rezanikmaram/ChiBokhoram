using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports")]
    public class OtherIncomeInvoiceController : Controller
    {
        public IOtherIncomeInvoiceService OtherIncomeInvoiceService { get; }
        public IOtherIncomeInvoiceReportService OtherIncomeInvoiceReportService { get; }
        public IOtherInvoiceFileService OtherInvoiceFileService { get; }
        public IOtherIncomeInvoiceDocFileService OtherIncomeInvoiceDocFileService { get; }
        public IOtherInvoicePublicService OtherInvoicePublicService { get; }

        public OtherIncomeInvoiceController(
            IOtherIncomeInvoiceService otherIncomeInvoiceService,
            IOtherIncomeInvoiceReportService otherIncomeInvoiceReportService,
            IOtherInvoiceFileService otherInvoiceFileService,
            IOtherIncomeInvoiceDocFileService otherIncomeInvoiceDocFileService,
            IOtherInvoicePublicService otherInvoicePublicService
        )
        {
            OtherIncomeInvoiceService = otherIncomeInvoiceService;
            OtherIncomeInvoiceReportService = otherIncomeInvoiceReportService;
            OtherInvoiceFileService = otherInvoiceFileService;
            OtherIncomeInvoiceDocFileService = otherIncomeInvoiceDocFileService;
            OtherInvoicePublicService = otherInvoicePublicService;
        }

        #region   سایر درامد
        public async Task<IActionResult> Index(int? p, OtherIncomeInvoiceFilterDto filter, CancellationToken token = default)
        {
            var data = await OtherIncomeInvoiceService.GetList(p ?? 1, filter, token);

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOtherIncomeInvoice(CreateOtherIncomeInvoiceDto createDto, CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await OtherIncomeInvoiceService.AddOrUpdate(createDto, token);
                return RedirectToAction(nameof(Index));
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetIncomeInvoiceById(Guid invoiceId, CancellationToken token = default)
        {
            var find = await OtherIncomeInvoiceService.GetById(invoiceId, token);
            if (find == null)
                throw new Exception("موردی یافت نشد");

            return Json(find);
        }

        public async Task<IActionResult> DeleteInvoice(Guid invoiceId, CancellationToken token = default)
        {
            await OtherIncomeInvoiceService.DeleteInvoice(invoiceId, token);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid invoiceId, CancellationToken token = default)
        {
            var data = await OtherIncomeInvoiceService.Details(invoiceId, token);

            return View(data);
        }
        #endregion


        [HttpPost]
        public async Task<IActionResult> GetInvoiceServiceById(Guid invoiceServiceId, CancellationToken token = default)
        {
            var find = await OtherIncomeInvoiceService.GetInvoiceServiceById(invoiceServiceId, token);
            if (find == null)
            {
                find = new OtherInvoiceServiceDto();
            }

            return Json(find);
        }

        #region PreFactor

        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> PreFactor(Guid invoiceId, CancellationToken token = default)
        {
            var data = await OtherIncomeInvoiceService.GetPreFactorPage(invoiceId, token);
            return View(data);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> EndPreFactor(Guid invoiceId, CancellationToken token = default)
        {
            await OtherIncomeInvoiceService.EndPreFactor(invoiceId, token);

            return RedirectToAction(nameof(PreFactor), new { invoiceId });
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> CreatePreService(CreateOtherInvoiceServiceDto createDto, CancellationToken token = default)
        {
            if (createDto == null)
                return RedirectToAction(nameof(Index));

            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("Id");

            createDto.Type = AllocationEquServiceType.PreFactor;
            var result = new OtherInvoiceServicePageDto { OtherIncomeInvoiceId = createDto.OtherIncomeInvoiceId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await OtherIncomeInvoiceService.AddOrEditService(createDto, token);
            }

            return PartialView("_PreFactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> DeletePreService(Guid invoiceServiceId, CancellationToken token = default)
        {
            var result = await OtherIncomeInvoiceService.DeletePreService(invoiceServiceId, token);

            return PartialView("_PreFactorListService", result);
        }

        //preview factor
        public async Task<IActionResult> GetPreviewFactor(OtherIncomeInvoicePreviewReportFilterDto filter, CancellationToken token = default)
        {
            try
            {
                var fileDto = await OtherIncomeInvoiceReportService.GetPreviewFactor(filter, token);

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

        public async Task<IActionResult> GetFile(Guid invoiceId, OtherIncomeInvoiceFileType type, CancellationToken token = default)
        {
            try
            {
                var fileDto = await OtherInvoiceFileService.ReadFile(invoiceId, type, token);

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

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> BackToPendingState(Guid invoiceId, CancellationToken token = default)
        {
            await OtherIncomeInvoiceService.BackToPendingState(invoiceId, token);

            return RedirectToAction(nameof(PreFactor), new { invoiceId });
        }
        #endregion



        #region Factor
        public async Task<IActionResult> Factor(Guid invoiceId, CancellationToken token = default)
        {
            var data = await OtherIncomeInvoiceService.GetFactorPage(invoiceId, token);
            return View(data);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> CreateFactorService(CreateOtherInvoiceServiceDto createDto, CancellationToken token = default)
        {
            if (createDto == null)
                return RedirectToAction(nameof(Index));

            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("Id");

            createDto.Type = AllocationEquServiceType.Factor;
            var result = new OtherInvoiceServicePageDto { OtherIncomeInvoiceId = createDto.OtherIncomeInvoiceId, CanChangeService = true };

            if (ModelState.IsValid)
            {
                result = await OtherIncomeInvoiceService.AddOrEditService(createDto, token);
            }

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> DeleteFactorService(Guid invoiceServiceId, CancellationToken token = default)
        {
            var result = await OtherIncomeInvoiceService.DeleteFactorService(invoiceServiceId, token);

            return PartialView("_FactorListService", result);
        }

        [HttpPost]
        public async Task<IActionResult> EndFactor(Guid invoiceId, CancellationToken token = default)
        {
            await OtherIncomeInvoiceService.EndFactor(invoiceId, token);

            return RedirectToAction(nameof(Factor), new { invoiceId });
        }

        [HttpPost]
        public async Task<IActionResult> BackToPreFactorState(Guid invoiceId, CancellationToken token = default)
        {
            await OtherIncomeInvoiceService.BackToPreFactorState(invoiceId, token);

            return RedirectToAction(nameof(Factor), new { invoiceId });
        }
        #endregion


        #region File Manager
        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> DocFiles(Guid invoiceId, int reternPage = 0, CancellationToken token = default)
        {
            var result = await OtherIncomeInvoiceDocFileService.GetDocFilePage(invoiceId, token);
            ViewBag.reternPage = reternPage;
            return View(result);
        }

        public async Task<IActionResult> DeleteFileById(Guid docFileId, CancellationToken token = default)
        {
            try
            {
                var invoiceId = await OtherInvoiceFileService.DeleteFileById(docFileId, token);

                return RedirectToAction(nameof(DocFiles), new { invoiceId });
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(
            OtherIncomeInvoiceSaveFileDto fileDto,
            int? goToPageCreate = null,
            CancellationToken token = default
        )
        {
            try
            {
                if (fileDto == null || fileDto?.File == null || fileDto?.File?.Length == 0)
                    return RedirectToAction(nameof(Index));

                var result = await OtherInvoiceFileService.SaveFile(fileDto, token);

                if (result == null)
                    return RedirectToAction(nameof(Index));

                await OtherInvoicePublicService.UpdateRemainderPrice(fileDto.OtherIncomeInvoiceId, token);

                if (goToPageCreate == null)
                    return RedirectToAction(nameof(PreFactor), new { invoiceId = fileDto.OtherIncomeInvoiceId });
                else
                    return RedirectToAction(nameof(DocFiles), new { invoiceId = fileDto.OtherIncomeInvoiceId });
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
                var fileDto = await OtherInvoiceFileService.ReadFileById(docFileId, token);

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
        #endregion
    }
}
