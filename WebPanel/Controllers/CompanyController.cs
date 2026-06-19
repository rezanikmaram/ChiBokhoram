using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Common.Utilities;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, AllocationEquToEnd")]
    public class CompanyController : Controller
    {
        private readonly IPersonelService personelService;
        private readonly IExcelToolsService excelToolsService;

        public CompanyController(
            ICompanyService companyService,
            IPortService portService,
            IPersonelService personelService,
            IExcelToolsService excelToolsService
        )
        {
            CompanyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
            PortService = portService ?? throw new ArgumentNullException(nameof(portService));
            this.personelService = personelService;
            this.excelToolsService = excelToolsService ?? throw new ArgumentNullException(nameof(excelToolsService));
        }

        public ICompanyService CompanyService { get; }
        public IPortService PortService { get; }

        public async Task<IActionResult> Index(CompanyFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await CompanyService.GetCompanyPageList(p ?? 1, filter, token);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(CompanyFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await CompanyService.GetCompanyPageList(p ?? 1, filter, token);
            return PartialView("_CompanyTable", result);
        }

        public async Task<IActionResult> Details(Guid companyId, CancellationToken token = default)
        {
            if (companyId == Guid.Empty)
                return RedirectToAction("AllTruks", "Transportation");

            var dto = await CompanyService.GetCompanyById(companyId, token);
            if (dto == null)
                return RedirectToAction(nameof(Index));

            return View(dto);
        }

        #region افزودن شرکت

        [HttpPost]
        public async Task<IActionResult> AddCompany(CreateCompanyDto addModel, CancellationToken token = default)
        {
            // اضافه کردن یا بروزرسانی شرکت
            var resultDto = await CompanyService.AddOrUpdate(addModel, token);

            return Json(resultDto.NewModelId.Value); // فقط شناسه شرکت برگرده
        }

        public IActionResult GetAddCompanyPage(CancellationToken token = default)
        {
            return PartialView("_AddCompany", new CreateCompanyDto { IsActive = true });
        }

        public async Task<IActionResult> GetCompanyNameById(Guid companyId, CancellationToken token = default)
        {
            var company = await CompanyService.GetCompanyById(companyId, token);

            return Json(
                new { id = company.Id, text = company.MeliCode == null ? company.CompanyName : $"{company.CompanyName} ({company.MeliCode})" }
            );
        }

        #endregion


        public async Task<IActionResult> CreateCompany(Guid companyId, CancellationToken token = default)
        {
            var activeport = await PortService.GetActivePortId(token);
            //var drop = await PortService.GetPortDropDown(token);
            //ViewBag.Port = new SelectList(drop, "Id", "Title");
            if (companyId.GuidHasValue())
            {
                var dto = await CompanyService.GetCompanyById(companyId, token);
                return View(dto);
            }

            return View(new CreateCompanyDto { IsActive = true });
        }

        [HttpGet]
        public async Task<IActionResult> CreateCompanyModal(Guid? companyId, CancellationToken token = default)
        {
            if (companyId.HasValue && companyId.Value.GuidHasValue())
            {
                var dto = await CompanyService.GetCompanyById(companyId.Value, token);
                return PartialView("_CompanyForm", dto);
            }

            return PartialView("_CompanyForm", new CreateCompanyDto { IsActive = true });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCompany(CreateCompanyDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty)
                ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = (await CompanyService.AddOrUpdate(dto, token)).Error;
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(dto);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveCompany(CreateCompanyDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty)
                ModelState.Remove("Id");

            if (!ModelState.IsValid)
            {
                return PartialView("_CompanyForm", dto);
            }

            var msg = (await CompanyService.AddOrUpdate(dto, token)).Error;
            return Json(new { success = true, message = msg });
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueEconomicCode(CreateCompanyDto createCompanyDto, CancellationToken token = default)
        {
            var result = await CompanyService.UniqueEconomicCodeValidation(createCompanyDto.EconomicCode, createCompanyDto.Id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueMeliCode(CreateCompanyDto createCompanyDto, CancellationToken token = default)
        {
            var result = await CompanyService.UniqueMeliCodeValidation(createCompanyDto.MeliCode, createCompanyDto.Id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        public async Task<IActionResult> DeleteCompany(Guid companyId, CancellationToken token = default)
        {
            TempData["msg"] = await CompanyService.DeleteById(companyId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetLoadCunterUser(Guid companyId, CancellationToken token = default)
        {
            var result = await personelService.GetLoadCounterUsre(companyId, token);
            ViewBag.company = await CompanyService.GetCompanyNameById(companyId, token);
            ViewBag.companyId = companyId;
            return View(result);
        }

        public async Task<IActionResult> ExportToExcel(CompanyFilterDto filter = null, CancellationToken token = default)
        {
            try
            {
                var companies = await CompanyService.GetAllCompaniesForExcel(filter, token);

                var excelStream = await excelToolsService.GetCompanyExcelFile(companies);

                if (excelStream == null)
                {
                    return BadRequest("خطا در تولید فایل اکسل");
                }

                return File(
                    excelStream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"Companies_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
                );
            }
            catch (Exception ex)
            {
                return BadRequest($"خطا در تولید فایل اکسل: {ex.Message}");
            }
        }
    }
}
