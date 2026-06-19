using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Utilities;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class WarehouseController : Controller
    {
        private readonly IPersonelService personelService;

        public WarehouseController(
            ICompanyService companyService,
            IProductService productService,
            IWarehouseService warehouseService,
            IPersonelService personelService
        )
        {
            CompanyService = companyService;

            ProductService = productService ?? throw new ArgumentNullException(nameof(productService));
            WarehouseService = warehouseService ?? throw new ArgumentNullException(nameof(warehouseService));
            this.personelService = personelService;
        }

        public ICompanyService CompanyService { get; }
        public IWarehouseService WarehouseService { get; }
        public IProductService ProductService { get; }

        public async Task<IActionResult> EditWarehousePartial(Guid warehouseId, Guid companyId, CancellationToken token = default)
        {
            // Fetch the warehouse data using the service
            var dto = await WarehouseService.GetWarehouseById(warehouseId, token);
            if (dto == null)
                return NotFound();

            // Populate ViewBag dropdowns (same as in CreateWarehouse GET)
            var drop = await ProductService.GetProductCategoryDropDown(token);
            ViewBag.product = new SelectList(drop, "Id", "Title");
            var dropsite = await WarehouseService.GetSiteDropDown(token);
            ViewBag.site = new SelectList(dropsite, "Id", "Title");
            ViewBag.Companies = await CompanyService.GetCompanySelectList(token);

            return PartialView("_CreateWarehouse", dto);
        }

        public async Task<IActionResult> CreateWarehousePartial(Guid? companyId, CancellationToken token = default)
        {
            var drop = await ProductService.GetProductCategoryDropDown(token);
            ViewBag.product = new SelectList(drop, "Id", "Title");
            var dropsite = await WarehouseService.GetSiteDropDown(token);
            ViewBag.site = new SelectList(dropsite, "Id", "Title");
            ViewBag.Companies = await CompanyService.GetCompanySelectList(token);

            var dto = new CreateWarehouseDto { CompanyId = companyId };
            return PartialView("_CreateWarehouse", dto);
        }

        public async Task<IActionResult> Site(CancellationToken token = default)
        {
            var viewModel = new SiteViewModel() { SiteDto = new SiteDto { } };

            viewModel.Sites = await WarehouseService.GetSites(token);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GetSiteDto(Guid siteId, CancellationToken token = default)
        {
            var result = await WarehouseService.GetSiteById(siteId);
            return PartialView("_CreateSite", result);
        }

        public async Task<IActionResult> CreateWarehouse(Guid warehouseId, Guid? companyId, CancellationToken token = default)
        {
            var drop = await ProductService.GetProductCategoryDropDown(token);
            ViewBag.product = new SelectList(drop, "Id", "Title");
            var dropsite = await WarehouseService.GetSiteDropDown(token);
            ViewBag.site = new SelectList(dropsite, "Id", "Title");

            ViewBag.Companies = await CompanyService.GetCompanySelectList(token);

            var dto = new CreateWarehouseDto { CompanyId = companyId };

            if (warehouseId.GuidHasValue())
            {
                dto = await WarehouseService.GetWarehouseById(warehouseId, token);
            }

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouse(CreateWarehouseDto dto, CancellationToken token = default)
        {
            if (ModelState.IsValid)
            {
                var result = await WarehouseService.AddOrUpdate(dto, token);
                if (!string.IsNullOrWhiteSpace(result))
                {
                    TempData["msg"] = result;
                    return View(dto);
                }

                return RedirectToAction(nameof(Warehouses), new { companyId = dto.CompanyId });
            }
            else
            {
                var drop = await ProductService.GetProductCategoryDropDown(token);
                ViewBag.Port = new SelectList(drop, "Id", "Title");
                var dropsite = await WarehouseService.GetSiteDropDown(token);
                ViewBag.site = new SelectList(dropsite, "Id", "Title");
                return View(dto);
            }
        }

        public async Task<IActionResult> Warehouses(WareHouseFilterDto filter, Guid? companyId, CancellationToken token = default)
        {
            filter ??= new WareHouseFilterDto { Page = 1 };
            if (filter.Page <= 0)
                filter.Page = 1;

            if (companyId != null)
                filter.CompanyId = companyId;

            var result = await WarehouseService.GetWarehousePage(filter, token);
            return View(result);
        }

        /// <summary>
        /// انباردارها
        /// </summary>

        public async Task<IActionResult> GetWarehouseKeeper(Guid companyId, CancellationToken token = default)
        {
            var result = await personelService.GetWarehouseKeeper(companyId, token);
            ViewBag.companyId = companyId;
            ViewBag.company = await CompanyService.GetCompanyNameById(companyId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSite(SiteDto dto, CancellationToken token = default)
        {
            await WarehouseService.AddOrUpdateSite(dto, token);
            return RedirectToAction(nameof(Site));
        }

        public async Task<IActionResult> DeleteSite(Guid siteId, CancellationToken token = default)
        {
            TempData["msg"] = await WarehouseService.DeleteSite(siteId, token);
            return RedirectToAction(nameof(Site));
        }

        public async Task<IActionResult> DeleteWarehouse(Guid id, Guid? companyId, CancellationToken token = default)
        {
            TempData["msg"] = await WarehouseService.DeleteWarehouse(id, token);
            return RedirectToAction(nameof(Warehouses), new { companyId });
        }

        #region WarehouseDoor


        public async Task<IActionResult> WarehouseDoor(Guid companyId, Guid warehouseId, CancellationToken token = default)
        {
            ViewBag.CompanyId = companyId;

            var data = await WarehouseService.GetWarehouseDoors(warehouseId, token);
            var dto = new WarehouseDoorPageDto
            {
                DataList = data,
                CreateWarehouseDoorDto = new CreateWarehouseDoorDto() { WarehouseId = warehouseId },
            };

            var findW = await WarehouseService.GetWarehouseById(warehouseId, token);
            if (findW != null)
                ViewBag.WarehouseName = findW.Name;

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWarehouseDoor(CreateWarehouseDoorDto createWarehouseDoorDto, CancellationToken token = default)
        {
            if (createWarehouseDoorDto?.Id == Guid.Empty)
                ModelState.Remove("createWarehouseDoorDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await WarehouseService.AddOrUpdateWarehouseDoor(createWarehouseDoorDto, token);
                return RedirectToAction(
                    nameof(WarehouseDoor),
                    new { warehouseId = createWarehouseDoorDto.WarehouseId, companyId = createWarehouseDoorDto.CompanyId }
                );
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(
                nameof(WarehouseDoor),
                new { warehouseId = createWarehouseDoorDto.WarehouseId, companyId = createWarehouseDoorDto.CompanyId }
            );
        }

        [HttpPost]
        public async Task<IActionResult> GetWarehouseDoorById(Guid warehouseDoorId, Guid warehouseId, CancellationToken token = default)
        {
            var find = await WarehouseService.GetWarehouseDoorById(warehouseDoorId);
            if (find == null)
                return RedirectToAction(nameof(WarehouseDoor), new { warehouseId = warehouseId });

            return Json(find);
        }

        public async Task<IActionResult> ActivationWarehouseDoor(
            Guid warehouseDoorId,
            Guid warehouseId,
            Guid companyId,
            CancellationToken token = default
        )
        {
            await WarehouseService.ActivationWarehouseDoor(warehouseDoorId, token);
            //TempData["portName"] = result;
            return RedirectToAction(nameof(WarehouseDoor), new { warehouseId, companyId });
        }

        public async Task<IActionResult> DeleteWarehouseDoor(
            Guid warehouseDoorId,
            Guid warehouseId,
            Guid companyId,
            CancellationToken token = default
        )
        {
            TempData["msg"] = await WarehouseService.DeleteWarehouseDoor(warehouseDoorId, token);
            return RedirectToAction(nameof(WarehouseDoor), new { warehouseId, companyId });
        }
        #endregion



        #region Segments
        public async Task<IActionResult> Segments(Guid warehouseId, CancellationToken token = default)
        {
            if (warehouseId == Guid.Empty)
                return RedirectToAction(nameof(Warehouses));

            var segments = await WarehouseService.GetSegments(warehouseId, token);

            return View(segments);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdateSegment(CreateWarehouseSegmentDto dto, CancellationToken token = default)
        {
            if (dto?.Id == Guid.Empty)
                ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                await WarehouseService.AddOrUpdateSegment(dto, token);

                return RedirectToAction(nameof(Segments), new { warehouseId = dto.WarehouseId });
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> DeleteWarehouseSegment(Guid warehouseSegmentId, Guid warehouseid, CancellationToken token = default)
        {
            TempData["msg"] = await WarehouseService.DeleteWarehouseSegment(warehouseSegmentId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Segments), new { warehouseId = warehouseid });
        }

        [HttpPost]
        public async Task<IActionResult> GetWarehouseSegmentById(Guid warehouseSegmentId, CancellationToken token = default)
        {
            var find = await WarehouseService.GetWarehouseSegmentById(warehouseSegmentId, token);
            return PartialView("_CreateSegment", find);
        }
        #endregion
    }
}
