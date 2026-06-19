using Common;
using Common.Utilities;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class TransportationController : Controller
    {

        public TransportationController(ICompanyService companyService, ITransportationService transportationService)
        {
            CompanyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            TransportationService = transportationService;
        }

        public ICompanyService CompanyService { get; }
        public ITransportationService TransportationService { get; }

      
        public async Task<IActionResult> CreateTruck(Guid? companyId, Guid truckId, CancellationToken token = default)
        {
            ViewBag.Companies = await CompanyService.GetCompanySelectList(token);

            CreateTruckDto dto = new CreateTruckDto();
            if (truckId.GuidHasValue())
            {
                dto = await TransportationService.GetTrukById(truckId, token);
                return View(dto);
            }
            dto.CompanyId = companyId;

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTruck(CreateTruckDto dto, CancellationToken token = default)
        {
            if (dto.CarPlatesType == Common.enumeration.CarPlatesType.Number)
            {
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesAreaCode));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesLeft));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesMiddle));
                ModelState.Remove(nameof(CreateTruckDto.CarPlatesRight));
            }
            else
                ModelState.Remove(nameof(CreateTruckDto.CarPlates));

            if (ModelState.IsValid)
            {
                var result = await TransportationService.AddOrUpdate(dto, token);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    TempData["msg"] = result;
                    //TempData["dto"] = JsonConvert.SerializeObject(dto);
                }
            }
            else
                return View(dto);

            if (dto.CompanyId == null)
                return RedirectToAction(nameof(AllTruks));
            return RedirectToAction(nameof(Truks), new { companyId = dto.CompanyId });
        }

        public async Task<IActionResult> DeleteTruck(Guid truckId, Guid? companyId, CancellationToken token = default)
        {
            var result = await TransportationService.DeleteTruck(truckId, token);
            if (!result)
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "خطایی در حذف رخ داده است."));


            if (companyId == null)
                return RedirectToAction(nameof(AllTruks));
            return RedirectToAction(nameof(Truks), new { companyId = companyId });
        }

        public async Task<IActionResult> Truks(Guid companyId, int? p = null, CancellationToken token = default)
        {
            ViewBag.company = await CompanyService.GetCompanyNameById(companyId, token);
            int pagenumaber = p ?? 1;
            var result = await TransportationService.GetTruck(companyId, pagenumaber, token);
            ViewBag.companyId = companyId;
            return View(result);
        }

        public async Task<IActionResult> AllTruks(Guid? companyId, int? p = null, CancellationToken token = default)
        {
            var result = await TransportationService.GetTruck(companyId, p ?? 1, token);
            ViewBag.companyId = companyId;
            return View(result);
        }

        public async Task<IActionResult> ActivationTruck(Guid truckId, Guid? companyId, CancellationToken token = default)
        {
            await TransportationService.ActivationTruck(truckId, token);

            if (companyId == null)
                return RedirectToAction(nameof(AllTruks), new { companyId = companyId });

            return RedirectToAction(nameof(Truks), new { companyId = companyId });
        }

        public async Task<IActionResult> Drivers(Guid companyId, int? p = null, CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var result = await TransportationService.GetDriver(companyId, pagenumaber, token);
            ViewBag.companyId = companyId;
            return View(result);
        }

        public async Task<IActionResult> CreateDriver(Guid? companyId, Guid driverId, CancellationToken token = default)
        {
            CreateDriverDto dto = new CreateDriverDto();
            if (driverId.GuidHasValue())
            {
                dto = await TransportationService.GetDriverById(driverId, token);
                return View(dto);
            }
            dto.CompanyId = companyId.Value;
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDriver(CreateDriverDto dto, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                await TransportationService.AddOrUpdate(dto, token);
                return RedirectToAction(nameof(Drivers), new { companyId = dto.CompanyId });
            }
            return View(dto);
        }

        public async Task<IActionResult> GetTruckListAsJson(Guid? companyId, CancellationToken token = default)
        {
            var result = await TransportationService.GetTrucksDropDownDto(companyId, token);
            return Json(result);
        }

        #region Truck Failure
        public async Task<IActionResult> TruckFailure(Guid? seoId = null,
            int? p = null,
            CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var data = await TransportationService.GetTruckFailurePage(new TruckFailureFilterDto { SeoId = seoId },
                pagenumaber, token);
            ViewBag.seoId = seoId;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> TruckFailure(TruckFailureFilterDto filter = null,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var data = await TransportationService.GetTruckFailurePage(filter, pagenumaber, token);
            return View(data);
        }
        #endregion

    }
}
