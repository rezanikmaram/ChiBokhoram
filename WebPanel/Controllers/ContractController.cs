using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Utilities;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.WebService.Abstract;
using Newtonsoft.Json;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly ICompanyService _companyService;

        public ContractController(IContractService contractService, ICompanyService companyService)
        {
            _contractService = contractService ?? throw new ArgumentNullException(nameof(contractService));
            _companyService = companyService ?? throw new ArgumentNullException(nameof(companyService));
        }

        private async Task PopulateCompanySelectLists(CancellationToken token)
        {
            ViewBag.Company = await _contractService.GetCompanySelectListForCurrentPort(token);
            ViewBag.AccountCompany = await _contractService.GetPortCompanySelectList(token);
            ViewBag.PortOperatorCompanies = await _companyService.GetCompanySelectList(token);
        }

        public async Task<IActionResult> Index(ContractFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            ViewBag.Company = await _contractService.GetCompanySelectListForCurrentPort(token);
            var result = await _contractService.GetContractPageList(p ?? 1, filter, token);
            return View(result);
        }

        public async Task<IActionResult> Options(Guid contractId, CancellationToken token = default)
        {
            if (!contractId.GuidHasValue())
                return RedirectToAction(nameof(Index));

            var model = await _contractService.GetContractOptions(contractId, token);
            ViewBag.CompanySelectList = await _contractService.GetCompanySelectListForCurrentPort(token);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOptions(UpdateContractOptionsDto dto, CancellationToken token = default)
        {
            if (dto == null || !dto.ContractId.GuidHasValue())
            {
                TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "قرارداد نامعتبر است"));
                TempData.Keep("msg");
                return RedirectToAction(nameof(Index));
            }

            TempData["msg"] = await _contractService.UpdateContractOptions(dto, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Options), new { contractId = dto.ContractId });
        }

        public async Task<IActionResult> CreateContract(Guid contractId, CancellationToken token = default)
        {
            await PopulateCompanySelectLists(token);
            if (contractId.GuidHasValue())
            {
                var dto = await _contractService.GetContractById(contractId, token);
                return View(dto);
            }

            return View(new CreateContractDto());
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract(CreateContractDto dto, CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty)
                ModelState.Remove("Id");

            if (ModelState.IsValid)
            {
                TempData["msg"] = await _contractService.AddOrUpdate(dto, token);
                return RedirectToAction(nameof(Index));
            }

            await PopulateCompanySelectLists(token);
            return View(dto);
        }

        public async Task<IActionResult> DeleteContract(Guid contractId, CancellationToken token = default)
        {
            TempData["msg"] = await _contractService.DeleteContract(contractId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetPortalUsers(Guid contractId, AssignContractPortalUsersFilterDto filter, CancellationToken token = default)
        {
            var vm = await _contractService.GetAssignedPortalUsers(contractId, filter, token);
            return View("AssignedContractPortalUsers", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPortalUsersToContract(AssignPortalUsersToContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.AssignPortalUsersToContract(dto, token);
            }
            return RedirectToAction(nameof(GetPortalUsers), new { contractId = dto.ContractId });
        }

        [HttpPost]
        public async Task<IActionResult> RemovePortalUsersFromContract(RemovePortalUsersFromContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.RemovePortalUsersFromContract(dto, token);
            }
            return RedirectToAction(nameof(GetPortalUsers), new { contractId = dto.ContractId });
        }

        #region Assign Gears
        public async Task<IActionResult> GetAssignedGears(Guid contractId, AssignContractGearsFilterDto filter, CancellationToken token = default)
        {
            var vm = await _contractService.GetAssignedGears(contractId, filter, token);
            return View("AssignedContractGears", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignGearsToContract(AssignGearsToContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.AssignGearsToContract(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedGears), new { contractId = dto.ContractId });
        }

        [HttpPost]
        public async Task<IActionResult> RemoveGearsFromContract(RemoveGearsFromContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.RemoveGearsFromContract(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedGears), new { contractId = dto.ContractId });
        }
        #endregion

        #region Assign Persons
        public async Task<IActionResult> GetAssignedPersons(Guid contractId, AssignContractPersonsFilterDto filter, CancellationToken token = default)
        {
            var vm = await _contractService.GetAssignedPersons(contractId, filter, token);
            return View("AssignedContractPersons", vm);
        }

        [HttpPost]
        public async Task<IActionResult> AssignPersonsToContract(AssignPersonsToContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.AssignPersonsToContract(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedPersons), new { contractId = dto.ContractId });
        }

        [HttpPost]
        public async Task<IActionResult> RemovePersonsFromContract(RemovePersonsFromContractDto dto, CancellationToken token = default)
        {
            if (!string.IsNullOrWhiteSpace(dto?.Ids))
            {
                await _contractService.RemovePersonsFromContract(dto, token);
            }
            return RedirectToAction(nameof(GetAssignedPersons), new { contractId = dto.ContractId });
        }
        #endregion
    }
}
