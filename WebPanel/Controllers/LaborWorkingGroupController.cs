using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.CustomAttribute;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class LaborWorkingGroupController : Controller
    {
        private ILaborWorkingGroupService LaborWorkingGroupService { get; set; }

        public LaborWorkingGroupController(ILaborWorkingGroupService laborWorkingGroupService)
        {
            LaborWorkingGroupService = laborWorkingGroupService;
        }

        public async Task<IActionResult> Index(CancellationToken token = default)
        {
            var data = await LaborWorkingGroupService.GetLaborWorkingGroup(token);
            var dto = new LaborWorkingGroupPageDto { LaborWorkingGroups = data, CreateLaborWorkingGroupDto = new CreateLaborWorkingGroupDto() };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLaborWorkingGroup(
            CreateLaborWorkingGroupDto createLaborWorkingGroupDto,
            CancellationToken token = default
        )
        {
            if (createLaborWorkingGroupDto?.Id == 0)
                ModelState.Remove("createLaborWorkingGroupDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await LaborWorkingGroupService.AddOrUpdate(createLaborWorkingGroupDto, token);
                return RedirectToAction(nameof(Index));
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetLaborWorkingGroupById(int laborWorkingGroupId, CancellationToken token = default)
        {
            var find = await LaborWorkingGroupService.GetLaborWorkingGroupById(laborWorkingGroupId);
            if (find == null)
                return RedirectToAction(nameof(Index));

            return Json(find);
        }

        public async Task<IActionResult> ActivateLaborWorkingGroup(int laborWorkingGroupId, CancellationToken token = default)
        {
            await LaborWorkingGroupService.ActivateLaborWorkingGroup(laborWorkingGroupId, token);
            return RedirectToAction(nameof(Index));
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyName(CreateLaborWorkingGroupDto createLaborWorkingGroupDto)
        {
            if (string.IsNullOrWhiteSpace(createLaborWorkingGroupDto.Name))
                return Json("نام گروه کاری نمی‌تواند خالی باشد");

            if (createLaborWorkingGroupDto.Name.Length > 100)
                return Json("نام گروه کاری نمی‌تواند بیشتر از 100 کاراکتر باشد");

            return Json(true);
        }
    }
}
