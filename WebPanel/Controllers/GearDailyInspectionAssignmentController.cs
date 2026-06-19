using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearDailyInspectionAssignmentController : Controller
    {
        private readonly IGearDailyInspectionAssignmentService _service;

        public GearDailyInspectionAssignmentController(IGearDailyInspectionAssignmentService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Manage(int gearId, string search, int? p, CancellationToken token)
        {
            var filter = new GearDailyInspectionAssignmentFilterDto { Search = search, p = p };
            var vm = await _service.GetPage(gearId, filter, token);
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Assign(AssignGearDailyInspectionDto dto, CancellationToken token)
        {
            if (dto == null) return BadRequest();
            if (dto.CheckListId == Guid.Empty) return RedirectToAction(nameof(Manage), new { gearId = dto.GearId });
            await _service.Assign(dto, token);
            return RedirectToAction(nameof(Manage), new { gearId = dto.GearId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(RemoveGearDailyInspectionDto dto, CancellationToken token)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _service.Remove(dto, token);
            }
            return RedirectToAction(nameof(Manage), new { gearId = dto.GearId });
        }

        // AJAX Actions for Modal
        [HttpGet]
        public async Task<IActionResult> ManageChecklistsPartial(int gearId, string search, CancellationToken token)
        {
            var filter = new GearDailyInspectionAssignmentFilterDto { Search = search };
            var vm = await _service.GetPage(gearId, filter, token);
            return PartialView("_ManageChecklists", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignAjax(AssignGearDailyInspectionDto dto, CancellationToken token)
        {
            if (dto == null || dto.CheckListId == Guid.Empty)
            {
                return Json(new { success = false, message = "لطفاً یک چک لیست انتخاب کنید" });
            }

            try
            {
                await _service.Assign(dto, token);
                return Json(new { success = true, message = "چک لیست با موفقیت اضافه شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAjax(RemoveGearDailyInspectionDto dto, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(dto?.Ids))
            {
                return Json(new { success = false, message = "لطفاً حداقل یک چک لیست انتخاب کنید" });
            }

            try
            {
                await _service.Remove(dto, token);
                return Json(new { success = true, message = "چک لیست(ها) با موفقیت حذف شد" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
