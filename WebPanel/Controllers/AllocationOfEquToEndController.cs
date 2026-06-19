using Common.Exceptions;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
    public class AllocationOfEquToEndController : Controller
    {
        public IAllocationEquFileService AllocationEquFileService { get; }
        public IAllocationEquChangeStateService AllocationEquChangeStateService { get; }
        public IAllocationEquToEndService AllocationEquToEndService { get; }

        public AllocationOfEquToEndController(
             IAllocationEquFileService allocationEquFileService,
            IAllocationEquChangeStateService allocationEquChangeStateService,
            IAllocationEquToEndService allocationEquToEndService)
        {
            AllocationEquFileService = allocationEquFileService;
            AllocationEquChangeStateService = allocationEquChangeStateService;
            AllocationEquToEndService = allocationEquToEndService;
        }


        public async Task<IActionResult> ToEnd(Guid allocationEquId,
            CancellationToken token = default)
        {
            var list = await AllocationEquToEndService.GetToEndPage(allocationEquId, token);
            return View(list);
        }

        //[HttpPost]
        //public async Task<IActionResult> SaveBaseInfo(SecondStepSeoDto baseInfo,
        //   CancellationToken token = default)
        //{
        //    if (baseInfo == null || baseInfo?.AllocationEquipmentId == Guid.Empty)
        //        return RedirectToAction("Index", "Seo");

        //    if (ModelState.IsValid)
        //    {
        //        await AllocationEquToEndService.SaveBaseInfo(baseInfo, token);
        //    }



        //    return RedirectToAction(nameof(ToEnd), new { allocationEquId = baseInfo.AllocationEquipmentId });
        //}



        public async Task<IActionResult> AcceptEndOperation(Guid allocationEquId,
            CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AcceptEndOperation(allocationEquId, token);
            if (string.IsNullOrWhiteSpace(result.Error))
                return RedirectToAction("SeoAllocationPage", "Seo", new { seoId = result.SeoId});
            else
            {
                TempData["msg"] = result.Error;
                return RedirectToAction(nameof(ToEnd), new { allocationEquId });
            }
        }


        #region Value Tonnage
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateValueTonnage(CreateAllocationEquValueTonnageDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateValueTonnage(dto, token);

            return PartialView("_ValueTonnageList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteValueTonnage(Guid valueTonnageId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteValueTonnage(valueTonnageId, token);

            return PartialView("_ValueTonnageList", result);
        }
        #endregion


        #region اجاره ساعتی - rental
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateRental(CreateAllocationEquRentalDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateRental(dto, token);

            return PartialView("_RentalList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRental(Guid rentalId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteRental(rentalId, token);

            return PartialView("_RentalList", result);
        }
        #endregion


        #region Count Displacements - تعداد جابجایی تجهیز
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateCountDis(CreateAllocationEquCountDisDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateCountDis(dto, token);

            return PartialView("_CountDisList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCountDis(Guid countDisId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteCountDis(countDisId, token);

            return PartialView("_CountDisList", result);
        }
        #endregion

        #region Moving warehouse door - تعداد جابجایی درب انبار
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateMovingDoor(CreateAllocationEquMovingDoorDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateMovingDoor(dto, token);

            return PartialView("_MovingDoorList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMovingDoor(Guid movingDoorId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteMovingDoor(movingDoorId, token);

            return PartialView("_MovingDoorList", result);
        }
        #endregion


        #region Damaged - خسارت وارد شده به تجهیزات
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateDamaged(CreateAllocationEquDamagedDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateDamaged(dto, token);

            return PartialView("_DamagedList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDamaged(Guid damagedId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteDamaged(damagedId, token);

            return PartialView("_DamagedList", result);
        }
        #endregion


        #region Description - توضیحات
        [HttpPost]
        public async Task<IActionResult> AddOrUpdateDescription(CreateAllocationEquDescriptionDto dto,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.AddOrUpdateDescription(dto, token);

            return PartialView("_DescriptionList", result);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteDescription(Guid descriptionId,
           CancellationToken token = default)
        {
            var result = await AllocationEquToEndService.DeleteDescription(descriptionId, token);

            return PartialView("_DescriptionList", result);
        }
        #endregion

        #region Report excel
        public async Task<IActionResult> GetAllocationEquOperationExcelFile(
           Guid allocationEquId,
           CancellationToken token = default)
        {
            try
            {
                var fileDto = await AllocationEquToEndService
                    .GetAllocationEquOperationExcelFile(allocationEquId, token);

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
