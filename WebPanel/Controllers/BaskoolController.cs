using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Entities.DTOs.Public;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class BaskoolController : Controller
    {
        public BaskoolController(IBaskoolService baskoolService, ICompanyService companyService, ITruckInfoSokhtService truckInfoSokhtService)
        {
            BaskoolService = baskoolService;
            CompanyService = companyService;
            TruckInfoSokhtService = truckInfoSokhtService;
        }

        public IBaskoolService BaskoolService { get; }
        public ICompanyService CompanyService { get; }
        public ITruckInfoSokhtService TruckInfoSokhtService { get; }

        public async Task<IActionResult> Index(CancellationToken token = default)
        {
            var data = await BaskoolService.GetBaskool(token);
            var dto = new BaskoolPageDto
            {
                Baskools = data,
                CreateBaskoolDto = new CreateBaskoolDto(),
                Companies = await CompanyService.GetCompanySelectList(token),
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBaskool(CreateBaskoolDto createBaskoolDto, CancellationToken token = default)
        {
            if (createBaskoolDto?.Id == 0)
                ModelState.Remove("createBaskoolDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await BaskoolService.AddOrUpdate(createBaskoolDto, token);
                return RedirectToAction(nameof(Index));
            }

            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetBaskoolById(int baskoolId, CancellationToken token = default)
        {
            var find = await BaskoolService.GetBaskoolById(baskoolId);
            if (find == null)
                return RedirectToAction(nameof(Index));

            return Json(find);
        }

        public async Task<IActionResult> ActivationBaskool(int baskoolId, CancellationToken token = default)
        {
            await BaskoolService.ActivationBaskool(baskoolId, token);
            //TempData["portName"] = result;
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteBaskool(int baskoolId, CancellationToken token = default)
        {
            TempData["msg"] = await BaskoolService.DeleteBaskool(baskoolId, token);
            return RedirectToAction(nameof(Index));
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueDeviceFpnCode(CreateBaskoolDto createBaskoolDto, CancellationToken token = default)
        {
            var result = await BaskoolService.UniqueBaskoolFpnCodeValidation(createBaskoolDto.CodeNameForFpn, createBaskoolDto.Id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        #region Weight
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> WeightList(CancellationToken token = default, int? p = null)
        {
            var list = await BaskoolService.GetBaskoolWeightList(p ?? 1, token);
            return View(list);
        }

        public async Task<IActionResult> UpdateWeighFreeBaskool(CancellationToken token = default)
        {
            await BaskoolService.UpdateWeighFreeBaskool(token);
            return RedirectToAction(nameof(WeightList));
        }
        #endregion


        #region WeightByUser

        public async Task<IActionResult> WeightByUserList(BaskolWeightByUserFilterDto filter, int? p = null, CancellationToken token = default)
        {
            var list = await BaskoolService.GetBaskoolWeightByUserList(p ?? 1, filter, token);
            return View(list);
        }
        #endregion

        #region اطلاعات کامیون ها از سرویس خارجی
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TrukInfoList(CancellationToken token = default, int? p = null)
        {
            var list = await TruckInfoSokhtService.GetTruckInfoList(p ?? 1, token);
            return View(list);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> TruckInfoInOut(TruckInfoInOutFilterDto filter, CancellationToken token = default, int? p = null)
        {
            var list = await TruckInfoSokhtService.GetTruckInfoInOut(filter, p ?? 1, token);
            return View(list);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DetectNoTruck(CancellationToken token = default)
        {
            await TruckInfoSokhtService.DetectAllNoTruck(token);
            return RedirectToAction(nameof(TrukInfoList));
        }
        #endregion
    }
}
