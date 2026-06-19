using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Helper;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    // چک لیست
    [Authorize(Roles = "admin, adminOnePort")]
    public class CheckListFactorController : Controller
    {
        private ICheckListFactorService checkListFactorService { get; set; }

        public CheckListFactorController(ICheckListFactorService checkListFactorService)
        {
            this.checkListFactorService = checkListFactorService;
        }

        #region CheckListFactor
        public async Task<IActionResult> Index(int p = 1, CancellationToken token = default)
        {
            var filter = new GetCheckListFactorFilterDto() { p = p };

            var result = await checkListFactorService.GetCheckListFactors(filter, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Index(GetCheckListFactorFilterDto filter, CancellationToken token = default)
        {
            var result = await checkListFactorService.GetCheckListFactors(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateCheckListFactor(int checkListFactorId, CancellationToken token = default)
        {
            if (!checkListFactorId.CheckEmpty())
            {
                var dto = await checkListFactorService.GetCheckListFactorById(checkListFactorId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateCheckListFactor(CreateCheckListFactorDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await checkListFactorService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteCheckListFactor(int checkListFactorId, CancellationToken token = default)
        {
            TempData["msg"] = await checkListFactorService.DeleteCheckListFactor(checkListFactorId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetCheckListFactorById(int checkListFactorId, CancellationToken token = default)
        {
            var find = await checkListFactorService.GetCheckListFactorById(checkListFactorId, token);
            return PartialView("_CreateCheckListFactor", find);
        }

        #endregion
    }
}
