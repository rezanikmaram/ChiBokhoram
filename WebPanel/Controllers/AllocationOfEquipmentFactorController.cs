using Common.enumeration;
using Common.Exceptions;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class AllocationOfEquipmentFactorController : Controller
    {
        public IAllocationEquFactorService AllocationEquipmentService { get; }
        public IAllocationEquReportService AllocationEquReportService { get; }
        public IAllocationEquFileService AllocationEquFileService { get; }
        public IAllocationEquChangeStateService AllocationEquChangeStateService { get; }

        public AllocationOfEquipmentFactorController(
            IAllocationEquFactorService allocationEquipmentService,
            IAllocationEquReportService allocationEquReportService,
            IAllocationEquFileService allocationEquFileService,
            IAllocationEquChangeStateService allocationEquChangeStateService)
        {
            AllocationEquipmentService = allocationEquipmentService;
            AllocationEquReportService = allocationEquReportService;
            AllocationEquFileService = allocationEquFileService;
            AllocationEquChangeStateService = allocationEquChangeStateService;
        }


        

        public async Task<IActionResult> Factor(Guid allocationEquId,
            CancellationToken token = default)
        {
            var data = await AllocationEquipmentService.GetById(allocationEquId, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> EndAllocationFactor(Guid allocationEquId,
           CancellationToken token = default)
        {
            await AllocationEquipmentService.EndAllocationFactor(allocationEquId, token);

            return RedirectToAction(nameof(Factor), new { allocationEquId });
        }

        [HttpPost]
        public async Task<IActionResult> GoBackToCompletingEndState(Guid allocationEquId,
           CancellationToken token = default)
        {
            await AllocationEquipmentService.GoBackToCompletingEndState(allocationEquId, token);

            return RedirectToAction(nameof(Factor), new { allocationEquId });
        }


        [HttpPost]
        public async Task<IActionResult> CreateEquService(CreateAllocationOfEquServiceDto createDto,
           CancellationToken token = default)
        {
            if (createDto == null) return RedirectToAction("ListAllocationOfEqu", "AllocationOfEquipment");

            if (createDto?.Id == Guid.Empty) ModelState.Remove("Id");
            var result = new AllocationOfEquServicePageDto
            {
                AllocationOfEquId = createDto.AllocationOfEquipmentId,
                CanChangeService = true, 
            };

            if (ModelState.IsValid)
            {
                result = await AllocationEquipmentService.AddOrEditEquService(createDto, token);

                return PartialView("_ListEqupmentService", result);
            }


            return BadRequest();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEquService(Guid equServiceId,
            CancellationToken token = default)
        {
            var result = await AllocationEquipmentService.DeleteEquService(equServiceId, token);

            return PartialView("_ListEqupmentService", result);
        }


        [HttpPost]
        public async Task<IActionResult> GetAllocationEquipmentServiceById(Guid allocationOfEquServiceId,
            CancellationToken token = default)
        {
            var find = await AllocationEquipmentService
                .GetAllocationOfEquServiceById(allocationOfEquServiceId, token);
            if (find == null)
                find = new AllocationOfEquServiceDto();

            return Json(find);
        }

         

         

        #region Allocation Equipment File
        
        public async Task<IActionResult> GetFile(
            Guid allocationOfEquId,
            AllocationEquFileType type,
            CancellationToken token = default)
        {
            try
            {
                var fileDto = await AllocationEquFileService
                    .ReadFile(allocationOfEquId,
                         type,
                         token);

                if (fileDto == null) return Ok();

                return File(fileDto.FileStream, "application/pdf", $"{fileDto.FileInfo.FileTitle}{fileDto.FileInfo.FileExtention}");
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }

        }
        #endregion


        #region تایید پرداخت در مرحله فاکتور
        [HttpPost]
        public async Task<IActionResult> AcceptPaymentFactor(
            AllocationOfEquAcceptPaymentDto AcceptPaymentDto,
            CancellationToken token = default)
        {
            await AllocationEquChangeStateService
                .AcceptPaymentPreFactor(AcceptPaymentDto, null, token);
            return RedirectToAction(nameof(Factor),
                new { allocationEquId = AcceptPaymentDto.AllocationEquId });
        }

        #endregion
    }
}
