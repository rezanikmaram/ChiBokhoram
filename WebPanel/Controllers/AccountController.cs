using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration.Log;
using Common.Exceptions;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Entities.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IPortService portService;
        private readonly IAccountPanelService accountPanelService;
        private readonly IContractService contractService;
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory;
        private readonly IOptionsSnapshot<SiteSettings> settings;

        private const string ContractSelectionSessionKey = "_ContractSelectionUserId";

        public AccountController(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IPortService portService,
            IAccountPanelService accountPanelService,
            IContractService contractService,
            IUserClaimsPrincipalFactory<ApplicationUser> claimsPrincipalFactory,
            IOptionsSnapshot<SiteSettings> settings
        )
        {
            RoleManager = roleManager;
            UserManager = userManager;
            SignInManager = signInManager;
            this.portService = portService;
            this.accountPanelService = accountPanelService;
            this.contractService = contractService;
            this.claimsPrincipalFactory = claimsPrincipalFactory;
            this.settings = settings;
        }

        public RoleManager<ApplicationRole> RoleManager { get; }
        public UserManager<ApplicationUser> UserManager { get; }
        public SignInManager<ApplicationUser> SignInManager { get; }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Login(UserLoginDto dto, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var user = await UserManager.FindByNameAsync(dto.UserName);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "نام کاربری یا کلمه عبور شما صحیح  نمی باشد");
                return View(dto);
            }

            var signInResult = await SignInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!signInResult.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "نام کاربری یا کلمه عبور شما صحیح  نمی باشد");
                return View(dto);
            }

            if (user.IsActive == false)
            {
                ModelState.AddModelError(string.Empty, "حساب شما غیر فعال شده است");
                return View(dto);
            }

            var contracts = await contractService.GetContractsForPortalUser(user.Id);
            var contractList = contracts ?? new List<PortalUserContractSelectionDto>();

            if (contractList.Count > 1)
            {
                HttpContext.Session.SetInt32(ContractSelectionSessionKey, user.Id);
                var selectionModel = new SelectContractViewModel
                {
                    UserId = user.Id,
                    FullName = ($"{user.FirstName} {user.LastName}").Trim(),
                    UserName = user.UserName,
                    Contracts = contractList,
                    ReturnUrl = returnUrl,
                };
                return View("SelectContract", selectionModel);
            }

            var selectedContract = contractList.Count == 1 ? contractList.First() : null;

            await SignInWithContractAsync(user, selectedContract);
            HttpContext.Session.Remove(ContractSelectionSessionKey);

            return await CompleteLoginAsync(user, returnUrl);
        }

        private IActionResult RedirectBasedOnRoles(IList<string> userRoles)
        {
            if (userRoles.Contains("DashboardAdmin"))
                return RedirectToAction("Index", "Dashboard");

            if (userRoles.Contains("incomeReports"))
                return RedirectToAction("SpecialIncome", "AllocationOfEquipment");

            return null;
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SelectContract(SelectContractViewModel model, CancellationToken token = default)
        {
            var sessionUserId = HttpContext.Session.GetInt32(ContractSelectionSessionKey);
            if (sessionUserId == null || sessionUserId.Value != model.UserId)
            {
                HttpContext.Session.Remove(ContractSelectionSessionKey);
                ModelState.AddModelError(string.Empty, "نشست شما منقضی شده است، مجدداً وارد شوید");
                return RedirectToAction(nameof(Login));
            }

            var user = await UserManager.FindByIdAsync(model.UserId.ToString());
            if (user == null)
            {
                HttpContext.Session.Remove(ContractSelectionSessionKey);
                ModelState.AddModelError(string.Empty, "کاربر یافت نشد");
                return RedirectToAction(nameof(Login));
            }

            if (user.IsActive == false)
            {
                HttpContext.Session.Remove(ContractSelectionSessionKey);
                ModelState.AddModelError(string.Empty, "حساب شما غیر فعال شده است");
                return View("Login", new UserLoginDto { UserName = user.UserName });
            }

            var contracts = await contractService.GetContractsForPortalUser(user.Id, token);
            model.Contracts = contracts ?? new List<PortalUserContractSelectionDto>();
            model.FullName = ($"{user.FirstName} {user.LastName}").Trim();
            model.UserName = user.UserName;

            if (!model.Contracts.Any())
            {
                HttpContext.Session.Remove(ContractSelectionSessionKey);
                await SignInWithContractAsync(user, null);
                return await CompleteLoginAsync(user, model.ReturnUrl);
            }

            if (!ModelState.IsValid)
            {
                return View("SelectContract", model);
            }

            var selectedId = model.SelectedContractId ?? Guid.Empty;
            var selectedContract = await contractService.GetPortalUserContractDetails(selectedId, user.Id, token);
            if (selectedContract == null)
            {
                ModelState.AddModelError("SelectedContractId", "قرارداد انتخاب شده معتبر نیست");
                return View("SelectContract", model);
            }

            await SignInWithContractAsync(user, selectedContract);
            HttpContext.Session.Remove(ContractSelectionSessionKey);

            return await CompleteLoginAsync(user, model.ReturnUrl);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> PanelUsers(CancellationToken token = default)
        {
            var users = await accountPanelService.GetPanelUsers(token);

            return View(users);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ManageUserContracts(int userId, AssignPanelUserContractsFilterDto filter, CancellationToken token = default)
        {
            var vm = await accountPanelService.GetPanelUserContracts(userId, filter, token);
            return View("PanelUserContracts", vm);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> AssignContractsToPanelUser(AssignContractsToPanelUserDto dto, CancellationToken token = default)
        {
            if (dto != null && dto.UserId > 0 && !string.IsNullOrWhiteSpace(dto.Ids))
            {
                await accountPanelService.AssignContractsToPanelUser(dto, token);
            }
            return RedirectToAction(nameof(ManageUserContracts), new { userId = dto.UserId });
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> RemoveContractsFromPanelUser(RemoveContractsFromPanelUserDto dto, CancellationToken token = default)
        {
            if (dto != null && dto.UserId > 0 && !string.IsNullOrWhiteSpace(dto.Ids))
            {
                await accountPanelService.RemoveContractsFromPanelUser(dto, token);
            }
            return RedirectToAction(nameof(ManageUserContracts), new { userId = dto.UserId });
        }

        private async Task SignInWithContractAsync(ApplicationUser user, PortalUserContractSelectionDto contractInfo)
        {
            var principal = await claimsPrincipalFactory.CreateAsync(user);
            if (principal?.Identity is ClaimsIdentity identity)
            {
                RemoveClaimIfExists(identity, "Contract");

                if (contractInfo != null)
                {
                    var contractPayload = new SelectedContractInfoDto
                    {
                        Id = contractInfo.ContractId,
                        ContractNumber = contractInfo.ContractNumber,
                        Title = contractInfo.Title,
                        CompanyName = contractInfo.CompanyName,
                        FinallVolume = contractInfo.FinallVolume,
                        FinallWeight = contractInfo.FinallWeight,
                        FinallVoyage = contractInfo.FinallVoyage,
                        FinallContainer = contractInfo.FinallContainer,
                        CompanySharePercentage = contractInfo.CompanySharePercentage,
                        EmployerSharePercentage = contractInfo.EmployerSharePercentage,
                        BeneficiaryPhoneNumber = contractInfo.BeneficiaryPhoneNumber,
                        WeightCalculationMethodAnbardary = contractInfo.WeightCalculationMethodAnbardary,
                        WeightCalculationMethodLoading = contractInfo.WeightCalculationMethodLoading,
                        WeightCalculationMethodDefault = contractInfo.WeightCalculationMethodDefault,
                        AccountNumbers = contractInfo.AccountNumbers ?? new List<ContractAccountNumberDto>(),
                    };

                    var payload = JsonConvert.SerializeObject(contractPayload);
                    identity.AddClaim(new Claim("Contract", payload));
                }
            }

            var authProperties = new AuthenticationProperties { IsPersistent = false, AllowRefresh = true };

            await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal, authProperties);
        }

        private static void RemoveClaimIfExists(ClaimsIdentity identity, string claimType)
        {
            var existingClaims = identity.FindAll(claimType).ToList();
            foreach (var claim in existingClaims)
            {
                identity.RemoveClaim(claim);
            }
        }

        private async Task<IActionResult> CompleteLoginAsync(ApplicationUser user, string returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (settings.Value.OrganizationInfo.Type == Common.enumeration.EnumOrganizationInfo.HedayatPGP)
                return RedirectToAction(
                    "DashboardEquipmentStatus",
                    "EquipmentLog",
                    new DashboardEquipmentFilterDto { Type = EquipmentLogType.Vessel }
                );

            var userRoles = await UserManager.GetRolesAsync(user);
            var roleRedirect = RedirectBasedOnRoles(userRoles);
            if (roleRedirect != null)
                return roleRedirect;

            return RedirectToAction("Index", "Voyage");
        }

        [AllowAnonymous]
        public async Task<IActionResult> CreateDefaultUser(CancellationToken token)
        {
            //RegisterUserDto dto = new RegisterUserDto();
            //var rolse = await RoleManager.Roles.ToListAsync(token);
            //dto.Roles = rolse;
            //ViewBag.category = EnumExtensions.GetEnumSelectList<UserCategory>(1);
            //await RoleManager.CreateAsync(new ApplicationRole() { Name = "admin", Title = "مدیر کل سامانه", NormalizedName = "admin" });
            try
            {
                ApplicationUser user = new ApplicationUser
                {
                    FirstName = "مدیر",
                    IsActive = true,
                    Email = "",
                    LastName = "سامانه",
                    UserName = "Admin",
                    PortId = Guid.Parse("bd541953-3fe7-ed11-a03a-e39549b01a8c"),
                };
                await UserManager.CreateAsync(user, "Admin@1402#");
                await UserManager.AddToRoleAsync(user, "admin");

                //return View(dto);
                return null;
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        [AllowAnonymous]
        public async Task<IActionResult> CreateDefaultRoles(CancellationToken token)
        {
            //await RoleManager.CreateAsync(
            //    new ApplicationRole()
            //    {
            //        Name = "admin",
            //        Title = "مدیر کل سامانه",
            //        NormalizedName = "admin",
            //    }
            //);
            //await RoleManager.CreateAsync(
            //    new ApplicationRole()
            //    {
            //        Name = "adminOnePort",
            //        Title = "مدیر بندر",
            //        NormalizedName = "adminOnePort",
            //    }
            //);
            //await RoleManager.CreateAsync(
            //    new ApplicationRole()
            //    {
            //        Name = "AllocationEquToEnd",
            //        Title = "عملیات تخصیص تجهیز",
            //        NormalizedName = "allocationEquToEnd",
            //    }
            //);

            //await RoleManager.CreateAsync(
            //    new ApplicationRole()
            //    {
            //        Name = "DashboardAdmin",
            //        Title = "داشبورد مدیریت",
            //        NormalizedName = "DashboardAdmin",
            //    }
            //);
            //await RoleManager.CreateAsync(
            //    new ApplicationRole()
            //    {
            //        Name = "incomeReports",
            //        Title = "گزارشات درآمد (مالی)",
            //        NormalizedName = "incomeReports",
            //    }
            //);

            await RoleManager.CreateAsync(
                new ApplicationRole()
                {
                    Name = "FinanceUser",
                    Title = "امور مالی",
                    NormalizedName = "FinanceUser",
                }
            );

            return Ok("رول ها ایجاد شدن");
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreatePortalUser(CreatePanelUserDto createUser)
        {
            bool currentUser = User?.Identity?.GetUserName() == createUser?.UserName;

            if (createUser.Id > 0)
            {
                ModelState.Remove("createUser.Password");
                ModelState.Remove("createUser.UserName");
            }
            ModelState.Remove("createUser.Id");
            if (ModelState.IsValid && createUser.SelectedRole.Length > 0)
            {
                var result = await accountPanelService.CreateUser(createUser);
                if (string.IsNullOrEmpty(result) && currentUser)
                    await Logout();

                TempData["msg"] = result;
            }
            else
            {
                string allErrors = string.Join(
                    Environment.NewLine,
                    ModelState
                        .Where(x => x.Value.Errors.Any())
                        .Select(x => $"{x.Key}: {string.Join(Environment.NewLine, x.Value.Errors.Select(e => e.ErrorMessage))}")
                );

                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, allErrors));
            }

            return RedirectToAction(nameof(PanelUsers));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int userId, CancellationToken token)
        {
            TempData["msg"] = await accountPanelService.DeleteUser(userId);
            return RedirectToAction(nameof(PanelUsers));
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ActivationUser(int userId, CancellationToken token)
        {
            TempData["msg"] = await accountPanelService.ActivationUser(userId);

            return RedirectToAction(nameof(PanelUsers));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetPortalUserById(int userId, CancellationToken token = default)
        {
            var find = await accountPanelService.GetPortalUserById(userId);
            if (find == null)
                return RedirectToAction(nameof(PanelUsers));

            return PartialView("_CreatePortalUser", find);
        }

        #region Validation

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ValidatePassword(CreatePanelUserDto createUser, CancellationToken token = default)
        {
            var result = await accountPanelService.ValidatePassword(createUser.Password);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ValidatePassword2(string newPass)
        {
            var result = await accountPanelService.ValidatePassword(newPass);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ValidateUser(CreatePanelUserDto createUser, CancellationToken token = default)
        {
            var result = await accountPanelService.ValidateUser(createUser);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }
        #endregion


        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await SignInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userFind = await UserManager.GetUserAsync(User);

            var result = await UserManager.ChangePasswordAsync(userFind, dto.OldPass, dto.NewPass);
            if (result.Succeeded)
            {
                await SignInManager.SignOutAsync();
                return RedirectToAction(nameof(Login));
            }
            if (result.Errors != null)
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, result.Errors?.First().Description));
            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            TempData["msg"] = await accountPanelService.ResetPassword(dto);
            return RedirectToAction(nameof(PanelUsers));
        }
    }
}
