using Common.Utilities;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class EquipmentController : Controller
    {
        public EquipmentController(IEquipmentService equipmentService, ICompanyService companyService)
        {
            EquipmentService = equipmentService;
            CompanyService = companyService;
        }

        public IEquipmentService EquipmentService { get; }
        public ICompanyService CompanyService { get; }

        public async Task<IActionResult> EquipmentCategories(CancellationToken token = default)
        {
            var cat = await EquipmentService.GetEquipmentCategories(token);
            var res = new EquipmentCategoryViewModel { EquipmentCategories = cat, EquipmentCategoryDto = new EquipmentCategoryDto() };
            return View(res);
        }

        [HttpPost]
        public async Task<IActionResult> GetEquipmentCategoryById(Guid equipmentCategoryId, CancellationToken token = default)
        {
            var find = await EquipmentService.GetEquipmentCategoryById(equipmentCategoryId, token);
            return PartialView("_CreateEquipmentCategory", find);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEquipmentCategory(EquipmentCategoryDto dto, CancellationToken token = default)
        {
            await EquipmentService.AddOrUpdate(dto, token);
            return RedirectToAction(nameof(EquipmentCategories));
        }


        public async Task<IActionResult> DeleteEquipmentCategory(Guid equipmentCategoryId, CancellationToken token = default)
        {
            TempData["msg"] = await EquipmentService.DeleteEquipmentCategory(equipmentCategoryId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(EquipmentCategories));
        }

         
        public async Task<IActionResult> EquipmentCompany(Guid companyId, CancellationToken token = default)
        {
            ViewBag.CompanyId = companyId;
            ViewBag.CompanyName = await CompanyService.GetCompanyNameById(companyId, token);
            var result = await EquipmentService.GetEquipments(companyId, token);
            return View(result);
        }


        public async Task<IActionResult> CreateEquipment(Guid companyId, Guid equipmentId, CancellationToken token = default)
        {
            if (!companyId.GuidHasValue())
            {
                return RedirectToAction(nameof(EquipmentCompany));
            }

            var drop = await EquipmentService.GetEquipmentCategoryDropDown(token);
            ViewBag.Categories = new SelectList(drop, "Id", "Title");

            var create = new CreateEquipmentDto { CompanyId = companyId };
            if (equipmentId.GuidHasValue())
            {
                create = await EquipmentService.GetEquipmentById(equipmentId);
            }

            return View(create);
        }

        [HttpPost]
        public async Task<IActionResult> CreateEquipment(CreateEquipmentDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty) ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                await EquipmentService.AddOrUpdate(dto, token);
            }

            //var create = new CreateEquipmentDto { CompanyId = dto.CompanyId };
            return RedirectToAction(nameof(EquipmentCompany), new { companyId = dto.CompanyId });
        }

        public async Task<IActionResult> GetEquipmentListAsJson(Guid? companyId, CancellationToken token = default)
        {
            var result = await EquipmentService.GetEquipmentsDropDownDto(companyId, token);
            return Json(result);
        }

        #region Equipment Failure
        public async Task<IActionResult> Failure(
            Guid? seoId = null,
            int? p = null,
            CancellationToken token = default)
        {
            int pagenumaber = p ?? 1;
            var data = await EquipmentService.GetEquipmentFailurePage(new EquipmentFailureFilterDto { SeoId = seoId },
                pagenumaber, token);
            ViewBag.seoId = seoId;
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Failure(EquipmentFailureFilterDto filter = null,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var data = await EquipmentService.GetEquipmentFailurePage(filter, pagenumaber, token);
            return View(data);
        }
        #endregion

    }
}
