using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    //محوطه ها و دستبندی محوطه ها
    [Authorize(Roles = "admin, adminOnePort")]
    public class AreaPointController : Controller
    {
        public AreaPointController(IAreaService  areaService)
        {
            AreaService = areaService;
        }

         public IAreaService AreaService { get; }


        #region AreaCategory
        public async Task<IActionResult> AreaCategories(CancellationToken token = default, int? p = null)
        {
            var result = await AreaService.GetAreaCategory(token);
            return View(result);
        }
        public async Task<IActionResult> CreateAreaCategory(int categoryId, CancellationToken token = default)
        {
            if (categoryId > 0)
            {
                var dto = await AreaService.GetAreaCategoryById(categoryId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAreaCategory(CreateAreaCategoryDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                await AreaService.AddOrUpdateCategory(dto, token);

                return RedirectToAction(nameof(AreaCategories));
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> RemoveAreaCategory(int categoryId, CancellationToken token = default)
        {
            TempData["msg"] = await AreaService.DeleteAreaCategory(categoryId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(AreaCategories));
        }

        [HttpPost]
        public async Task<IActionResult> GetAreaCategoryById(int categoryId, CancellationToken token = default)
        {
            var find = await AreaService.GetAreaCategoryById(categoryId, token);
            return PartialView("_CreateAreaCategory", find);
        }
        #endregion


        #region Area
        public async Task<IActionResult> Areas(int categoryId, CancellationToken token = default, int? p = null)
        {
            if(categoryId <= 0) return RedirectToAction(nameof(AreaCategories));

            var cat = await AreaService.GetAreaCategoryById(categoryId, token);
            if (cat == null) return RedirectToAction(nameof(AreaCategories));

            ViewBag.categoryName = cat.Name;
            var result = await AreaService.GetAreaByCategoryId(categoryId, token);
            ViewBag.CategoryId = categoryId;
            return View(result);
        }
        public async Task<IActionResult> CreateArea(int areaId, CancellationToken token = default)
        {
            if (areaId > 0)
            {
                var dto = await AreaService.GetAreaById(areaId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateArea(CreateAreaDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                await AreaService.AddOrUpdateArea(dto, token);

                return RedirectToAction(nameof(Areas), new { categoryId = dto.AreaCategoryId });
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> RemoveArea(int areaId, int categoryId, CancellationToken token = default)
        {
            TempData["msg"] = await AreaService.DeleteArea(areaId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Areas), new { categoryId = categoryId });
        }

        [HttpPost]
        public async Task<IActionResult> GetAreaById(int areaId, CancellationToken token = default)
        {
            var find = await AreaService.GetAreaById(areaId, token);
            return PartialView("_CreateArea", find);
        }
        #endregion

    }
}
