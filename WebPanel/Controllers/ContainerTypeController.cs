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
    // انواع کانتینر
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContainerTypeController : Controller
    {
        private IContainerTypeService _containerTypeService { get; set; }
        private IContainerGroupService _containerGroupService { get; set; }

        public ContainerTypeController(IContainerTypeService containerTypeService, IContainerGroupService containerGroupService)
        {
            _containerTypeService = containerTypeService;
            _containerGroupService = containerGroupService;
        }

        #region ContainerType
        public async Task<IActionResult> Index(GetContainerTypeFilterDto filter, CancellationToken token = default)
        {
            ViewBag.ContainerGroups = await this._containerGroupService.GetContainerGroupSelectList();
            var result = await _containerTypeService.GetContainerTypes(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateContainerType(int containerTypeId, CancellationToken token = default)
        {
            ViewBag.ContainerGroups = await this._containerGroupService.GetContainerGroupSelectList();
            if (containerTypeId > 0)
            {
                var dto = await _containerTypeService.GetContainerTypeById(containerTypeId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainerType(CreateContainerTypeDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            ViewBag.ContainerGroups = await this._containerGroupService.GetContainerGroupSelectList();
            if (ModelState.IsValid)
            {
                var result = await _containerTypeService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteContainerType(int containerTypeId, CancellationToken token = default)
        {
            TempData["msg"] = await _containerTypeService.DeleteContainerType(containerTypeId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetContainerTypeById(int containerTypeId, CancellationToken token = default)
        {
            ViewBag.ContainerGroups = await this._containerGroupService.GetContainerGroupSelectList();
            var find = await _containerTypeService.GetContainerTypeById(containerTypeId, token);
            return PartialView("_CreateContainerType", find);
        }

        #endregion
    }
}
