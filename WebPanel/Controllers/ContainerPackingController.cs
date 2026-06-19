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
    // انواع بسته بندی کانتینر
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContainerPackingTypeController : Controller
    {
        private IContainerPackingTypeService _containerPackingTypeService { get; set; }

        public ContainerPackingTypeController(IContainerPackingTypeService containerPackingTypeService)
        {
            _containerPackingTypeService = containerPackingTypeService;
        }

        #region ContainerPackingType
        public async Task<IActionResult> Index(GetContainerPackingTypeFilterDto filter, CancellationToken token = default)
        {
            var result = await _containerPackingTypeService.GetContainerPackingTypes(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateContainerPackingType(int containerPackingTypeId, CancellationToken token = default)
        {
            if (containerPackingTypeId > 0)
            {
                var dto = await _containerPackingTypeService.GetContainerPackingTypeById(containerPackingTypeId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainerPackingType(CreateContainerPackingTypeDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await _containerPackingTypeService.AddOrUpdate(dto, token);
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

        public async Task<IActionResult> DeleteContainerPackingType(int containerPackingTypeId, CancellationToken token = default)
        {
            TempData["msg"] = await _containerPackingTypeService.DeleteContainerPackingType(containerPackingTypeId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetContainerPackingTypeById(int containerPackingTypeId, CancellationToken token = default)
        {
            var find = await _containerPackingTypeService.GetContainerPackingTypeById(containerPackingTypeId, token);
            return PartialView("_CreateContainerPackingType", find);
        }

        #endregion
    }
}
