using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.DTOs.Web.Hirman.CheckList;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Helper;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    // چک لیست
    [Authorize(Roles = "admin, adminOnePort")]
    public class CheckListController : Controller
    {
        private ICheckListService checkListService { get; set; }

        public CheckListController(ICheckListService checkListService)
        {
            this.checkListService = checkListService;
        }

        #region CheckList
        public async Task<IActionResult> Index(int p = 1, CancellationToken token = default)
        {
            var filter = new GetCheckListFilterDto() { p = p };

            var result = await checkListService.GetCheckLists(filter, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Index(GetCheckListFilterDto filter, CancellationToken token = default)
        {
            var result = await checkListService.GetCheckLists(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateCheckList(Guid checkListId, CancellationToken token = default)
        {
            if (!checkListId.CheckEmpty())
            {
                var dto = await checkListService.GetCheckListById(checkListId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckList(CreateCheckListDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await checkListService.AddOrUpdate(dto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                    return View(dto);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> DeleteCheckList(Guid checkListId, CancellationToken token = default)
        {
            TempData["msg"] = await checkListService.DeleteCheckList(checkListId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetCheckListById(Guid checkListId, CancellationToken token = default)
        {
            var find = await checkListService.GetCheckListById(checkListId, token);
            return PartialView("_CreateCheckList", find);
        }

        public async Task<IActionResult> GetAssignedCheckListFactor(
            Guid checkListId,
            AssignedCheckListFilterDto filter,
            CancellationToken token = default
        )
        {
            var item = await checkListService.GetAssignedCheckListFactors(checkListId, filter, token);
            return View("AssignedCheckListFactor", item);
        }

        [HttpPost]
        public async Task<IActionResult> AssignedCheckListFactorToCheckList(
            AssignedCheckListFactorToCheckListDto dto,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(dto.Ids))
            {
                await checkListService.AssignedCheckListFactorToCheckList(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedCheckListFactor), new { checkListId = dto.CheckListId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveAssignedCheckListFactorFromCheckList(
            RemoveAssignedCheckListFactorFromCheckListDto dto,
            CancellationToken token = default
        )
        {
            if (!string.IsNullOrEmpty(dto.Ids))
            {
                await checkListService.RemoveAssignedCheckListFactorFromCheckList(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedCheckListFactor), new { checkListId = dto.CheckListId });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateAssignedCheckListFactorSide(
            Guid checkListId,
            int checkListFactorId,
            string side,
            bool isChecked,
            CancellationToken token = default
        )
        {
            await checkListService.UpdateAssignedCheckListFactorSide(checkListId, checkListFactorId, side, isChecked, token);
            return Json(new { success = true });
        }
        #endregion
    }
}
