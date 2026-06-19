using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Entities.DTOs;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ProductController : Controller
    {
        private readonly ICheckListService _checkListService;
        private readonly IGeneralCargoProductService generalCargoProductService;

        public ProductController(
            IProductService productService,
            ICheckListService checkListService,
            IGeneralCargoProductService generalCargoProductService,
            ICargoHandlingTariffService cargoHandlingTariffService
        )
        {
            ProductService = productService;
            _checkListService = checkListService;
            this.generalCargoProductService = generalCargoProductService;
            CargoHandlingTariffService = cargoHandlingTariffService;
        }

        public IProductService ProductService { get; }
        public ICargoHandlingTariffService CargoHandlingTariffService { get; }

        public async Task<IActionResult> Index(ProductFilterDto filter, CancellationToken token = default)
        {
            var products = await ProductService.GetProductCategory(filter, token);
            return View(products);
        }

        public async Task<IActionResult> Products(ProductCargoHandlingFilterDto filter, CancellationToken token = default)
        {
            var products = await ProductService.GetProductCargoHandlingFlat(filter, token);
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductCategory(CreateProductCategoryDto dto, CancellationToken token = default)
        {
            await ProductService.AddOrUpdateCategory(dto, token);
            if (dto?.ReturnPage == 100)
                return RedirectToAction(nameof(Products));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetProductCategoryById(Guid categoryId, CancellationToken token = default)
        {
            var find = await ProductService.GetProductCategoryById(categoryId, token);
            return PartialView("_CreateProductCategory", find);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(CreateProductDto createDto, CancellationToken token = default)
        {
            await ProductService.AddOrUpdateProduct(createDto, token);
            if (createDto?.ReturnPage == 100)
                return RedirectToAction(nameof(Products));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(CreateProductDto addModel, CancellationToken token = default)
        {
            var id = await ProductService.AddOrUpdateProduct(addModel, token);
            return Json(id);
        }

        public async Task<IActionResult> GetAddProductPage(CancellationToken token = default)
        {
            var data = await ProductService.GetAddProductPage(token);
            return PartialView("_AddProduct", data);
        }

        public async Task<IActionResult> GetProductAndCategoryNameById(Guid productId, CancellationToken token = default)
        {
            var product = await ProductService.GetProductAndCategoryNameById(productId, token);

            return Json(new { id = product.Id, text = product.ProductName });
        }

        [HttpPost]
        public async Task<IActionResult> GetProductById(Guid productId, int? returnPage = null, CancellationToken token = default)
        {
            var find = await ProductService.GetProductById(productId, token);
            find.ReturnPage = returnPage;
            var categories = await ProductService.GetProductCategoryDropDown(token);

            var result = new CreateProductPageDto
            {
                CreateDto = find,
                Categories = categories.ToSelectList(),
                CheckLists = await this._checkListService.GetCheckListSelecList(token),
                GeneralCargoProducts = await CargoHandlingTariffService.GetBaseCargoHandlingForProduct(token),
            };

            return PartialView("_CreateProduct", result);
        }

        [HttpPost]
        public async Task<IActionResult> GetNewProduct(Guid categoryId, int? returnPage = null, CancellationToken token = default)
        {
            CreateProductCategoryDto find = new CreateProductCategoryDto { ReturnPage = returnPage };

            if (categoryId != Guid.Empty)
                find = await ProductService.GetProductCategoryById(categoryId, token);

            var categories = await ProductService.GetProductCategoryDropDown(token);

            var result = new CreateProductPageDto
            {
                CreateDto = new CreateProductDto
                {
                    ProductCategoryId = categoryId,
                    ProductCategoryTrafficType = find?.TrafficType ?? Common.enumeration.TrafficType.Bulk,
                    ExpireDayCount = find?.ExpireDayCount ?? 60,
                    ReturnPage = returnPage,
                },
                Categories = categories.ToSelectList(),
                CheckLists = await this._checkListService.GetCheckListSelecList(token),
                GeneralCargoProducts = await CargoHandlingTariffService.GetBaseCargoHandlingForProduct(token),
            };

            return PartialView("_CreateProduct", result);
        }

        public async Task<IActionResult> DeleteProduct(Guid productId, int? returnPage = null, CancellationToken token = default)
        {
            TempData["msg"] = await ProductService.DeleteProduct(productId, token);
            TempData.Keep("msg");

            if (returnPage == 100)
                return RedirectToAction(nameof(Products));

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteProductCategory(Guid productCategoryId, CancellationToken token = default)
        {
            TempData["msg"] = await ProductService.DeleteProductCategory(productCategoryId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        #region HS Code
        public async Task<IActionResult> HSCode(int? p, ProductHSCodeFilterDto filter, CancellationToken token = default)
        {
            var data = await ProductService.GetHSCodeList(p ?? 1, filter, token);

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHSCode(CraeteProductHSCode createDto, CancellationToken token = default)
        {
            if (createDto?.Id == Guid.Empty)
                ModelState.Remove("createDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await ProductService.AddOrUpdateHSCode(createDto, token);
                return RedirectToAction(nameof(HSCode));
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(HSCode));
        }

        [HttpPost]
        public async Task<IActionResult> GetHSCodeById(Guid hsCodeId, CancellationToken token = default)
        {
            var find = await ProductService.GetHSCodeById(hsCodeId, token);
            if (find == null)
                throw new Exception("موردی یافت نشد");

            return Json(find);
        }

        public async Task<IActionResult> DeleteHscode(Guid HscodeId, CancellationToken token = default)
        {
            TempData["msg"] = await ProductService.DeleteHscode(HscodeId, token);
            return RedirectToAction(nameof(HSCode));
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueHsCodeNumber(CraeteProductHSCode createDto, CancellationToken token = default)
        {
            var result = await ProductService.UniqueHSCodeNumberValidation(createDto.Number, createDto.Id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }
        #endregion
    }
}
