using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    // گروه های کانتینر
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContainerGroupController : Controller
    {
        private IContainerGroupService _containerGroupService { get; set; }

        public ContainerGroupController(IContainerGroupService containerGroupService)
        {
            _containerGroupService = containerGroupService;
        }

        #region ContainerGroup
        public async Task<IActionResult> Index(GetContainerGroupFilterDto filter, CancellationToken token = default)
        {
            var result = await _containerGroupService.GetContainerGroups(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateContainerGroup(int containerGroupId, CancellationToken token = default)
        {
            if (containerGroupId > 0)
            {
                var dto = await _containerGroupService.GetContainerGroupById(containerGroupId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainerGroup(CreateContainerGroupDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _containerGroupService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteContainerGroup(int containerGroupId, CancellationToken token = default)
        {
            TempData["msg"] = await _containerGroupService.DeleteContainerGroup(containerGroupId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetContainerGroupById(int containerGroupId, CancellationToken token = default)
        {
            var find = await _containerGroupService.GetContainerGroupById(containerGroupId, token);
            return PartialView("_CreateContainerGroup", find);
        }

        #endregion
    }
}
