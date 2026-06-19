using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.WebService.Abstract;
using Services.WebService.Abstract.Personel;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class PersonelController : Controller
    {
        private readonly IPersonelService personelService;
        private readonly ICompanyService companyService;
        private readonly ISeoService seoService;
        private readonly IVoyageService voyageService;
        private readonly IContractService contractService;

        public IPersonelPublicService PersonelPublicService { get; }

        public PersonelController(
            IPersonelService personelService,
            IPersonelPublicService personelPublicService,
            ICompanyService companyService,
            ISeoService seoService,
            IVoyageService voyageService,
            IContractService contractService
        )
        {
            this.personelService = personelService ?? throw new ArgumentNullException(nameof(personelService));
            PersonelPublicService = personelPublicService;
            this.companyService = companyService;
            this.seoService = seoService;
            this.voyageService = voyageService;
            this.contractService = contractService;
        }

        #region Create User partial View
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> PhoneNumberExist(CreatePeronelDto createPeronelDto, CancellationToken token = default)
        {
            var result = await personelService.PhoneNumberExist(createPeronelDto, token);
            if (result)
            {
                var message = "شماره موبایل تکراری است";
                var portName = await personelService.GetPortNameFromPhone(createPeronelDto.Phone, token);
                if (!string.IsNullOrWhiteSpace(portName))
                    message += $" ثبت شده در {portName}";
                return Json(message);
            }
            else
                return Json(true);
        }

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> MeliCodeExist(CreatePeronelDto createPeronelDto, CancellationToken token = default)
        {
            var result = await personelService.MeliCodeExist(createPeronelDto, token);
            if (result)
            {
                var message = "کد ملی تکراری است";
                var portName = await personelService.GetPortNameFromMeliCode(createPeronelDto.MeliCode, token);
                if (!string.IsNullOrWhiteSpace(portName))
                    message += $" ثبت شده در {portName}";
                return Json(message);
            }
            else
                return Json(true);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePersonel(CreatePeronelDto createPeronelDto, CancellationToken token = default)
        {
            if (createPeronelDto?.Id == Guid.Empty)
                ModelState.Remove("createPeronelDto.Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await personelService.AddOrUpdate(createPeronelDto, token);
            }

            return RedirectToAction(nameof(UserList), new { UserType = createPeronelDto.Type });
        }

        [HttpPost]
        public async Task<IActionResult> GetPersonelPageById(Guid personId, CancellationToken token = default)
        {
            var find = await personelService.GetPersonelPageById(personId, token);
            find.Companies = await companyService.GetCompanySelectList(token);
            find.Contracts = await contractService.GetContractSelectListForCurrentPort(token);

            return PartialView("_CreatePerson", find);
        }
        #endregion

        public async Task<IActionResult> UserList(PersonelFilterDto filter, CancellationToken token = default, int? p = null)
        {
            var result = await personelService.GetAllUser(p ?? 1, filter, token);

            result.CreatePeronelPageDto.Companies = await companyService.GetCompanySelectList(token);
            result.CreatePeronelPageDto.Contracts = await contractService.GetContractSelectListForCurrentPort(token);
            return View(result);
        }

        public async Task<IActionResult> ChangePass(Guid personId, CancellationToken token = default)
        {
            var result = await personelService.ChangePass(personId, token);

            return RedirectToAction(nameof(UserList));
        }

        public async Task<IActionResult> GetAllActiveVoyages(Guid personelId, CancellationToken token = default)
        {
            ViewBag.personelId = personelId;
            var data = await voyageService.GetAllActiveVoyages(personelId, token);
            return PartialView(data);
        }

        public async Task<IActionResult> SetActiveVoyage(Guid personelId, Guid voyageId, CancellationToken token = default)
        {
            await PersonelPublicService.SetActiveVoyage(personelId, voyageId, token);
            return RedirectToAction(nameof(UserList));
        }
        //private IActionResult RedirectToPage(UserMobileType result)
        //{
        //    return result switch
        //    {
        //        UserMobileType.BarshomarWaterfront => RedirectToAction(nameof(UserList)),
        //        UserMobileType.BarshomarMohavate => RedirectToAction(nameof(WarehouseKeeperUser)),
        //        UserMobileType.OwnerProduct => RedirectToAction(nameof(ProductOwnerUser)),
        //        UserMobileType.Billing => RedirectToAction(nameof(BillingUser)),
        //        _ => RedirectToAction(nameof(UserList)),
        //    };
        //}
    }
}
