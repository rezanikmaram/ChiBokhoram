using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    //دستگاه های FPN
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearController : Controller
    {
        private IGearService _gearService { get; set; }
        private ICompanyService _companyService { get; set; }
        private IGearStateService _gearStateService { get; set; }
        private IGearTypeService _gearTypeService { get; set; }
        private IPortService _portService { get; set; }
        private IContractService _contractService { get; set; }

        public GearController(
            IGearService gearService,
            ICompanyService companyService,
            IGearStateService gearStateService,
            IGearTypeService gearTypeService,
            IPortService portService,
            IContractService contractService
        )
        {
            _gearService = gearService;
            _companyService = companyService;
            _gearStateService = gearStateService;
            _gearTypeService = gearTypeService;
            _portService = portService;
            _contractService = contractService;
        }

        #region Gear
        public async Task<IActionResult> Index(int p = 1, CancellationToken token = default)
        {
            ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
            ViewBag.Companies = await _companyService.GetCompanySelectList();
            ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
            ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);

            var filter = new GetGearFilterDto() { p = p };

            var result = await _gearService.GetGears(filter, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Index(GetGearFilterDto filter, CancellationToken token = default)
        {
            ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
            ViewBag.Companies = await _companyService.GetCompanySelectList();
            ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
            ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);

            var result = await _gearService.GetGears(filter, token);
            return View(result);
        }

        public async Task<IActionResult> CreateGear(int gearId, CancellationToken token = default)
        {
            ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
            ViewBag.Companies = await _companyService.GetCompanySelectList();
            ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
            ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);
            if (gearId > 0)
            {
                var dto = await _gearService.GetGearById(gearId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateGear(CreateGearDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            ModelState.Remove("PortId");
            if (ModelState.IsValid)
            {
                var result = await _gearService.AddOrUpdate(dto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                    ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
                    ViewBag.Companies = await _companyService.GetCompanySelectList();
                    ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
                    ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);
                    return View(dto);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
                ViewBag.Companies = await _companyService.GetCompanySelectList();
                ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
                ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);
                return View(dto);
            }
        }

        public async Task<IActionResult> DeleteGear(int gearId, CancellationToken token = default)
        {
            TempData["msg"] = await _gearService.DeleteGear(gearId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetGearById(int gearId, CancellationToken token = default)
        {
            ViewBag.GearTypes = await _gearTypeService.GetGearTypeSelectList();
            ViewBag.Companies = await _companyService.GetCompanySelectList();
            ViewBag.GearStates = await _gearStateService.GetGearStateSelectList();
            ViewBag.Contracts = await _contractService.GetContractSelectListForCurrentPort(token);
            
            if (gearId <= 0)
            {
                return PartialView("_CreateGear", new CreateGearDto());
            }
            
            var find = await _gearService.GetGearById(gearId, token);
            return PartialView("_CreateGear", find ?? new CreateGearDto());
        }

        [HttpPost]
        public async Task<IActionResult> GetGearTable(GetGearFilterDto filter, CancellationToken token = default)
        {
            var result = await _gearService.GetGears(filter, token);
            return PartialView("_GearTable", result);
        }

        [HttpPost]
        public async Task<IActionResult> SaveGearAjax(CreateGearDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            ModelState.Remove("PortId");
            
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "لطفاً تمام فیلدهای الزامی را پر کنید" });
            }

            var result = await _gearService.AddOrUpdate(dto, token);
            
            if (result != null)
            {
                return Json(new { success = false, message = result });
            }

            return Json(new { success = true, message = dto.Id > 0 ? "تجهیز با موفقیت ویرایش شد" : "تجهیز با موفقیت ثبت شد" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteGearAjax(int gearId, CancellationToken token = default)
        {
            var result = await _gearService.DeleteGear(gearId, token);
            
            if (!string.IsNullOrEmpty(result))
            {
                return Json(new { success = false, message = result });
            }

            return Json(new { success = true, message = "تجهیز با موفقیت حذف شد" });
        }

        public async Task<IActionResult> GearUseInAllocationOfEqu(GearUseInAllocationOfEquFilterDto filter, CancellationToken token = default)
        {
            var data = await _gearService.GearUseInAllocationOfEqu(filter, token);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SetGeaForUseInAllocationOfEqu(string ids, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _gearService.SetGeaForUseInAllocationOfEqu(ids, token);
            }
            return RedirectToAction(nameof(GearUseInAllocationOfEqu));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveGearUseInAllocationOfEqu(string ids, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                await _gearService.RemoveGearUseInAllocationOfEqu(ids, token);
            }
            return RedirectToAction(nameof(GearUseInAllocationOfEqu));
        }

        #endregion
    }
}
