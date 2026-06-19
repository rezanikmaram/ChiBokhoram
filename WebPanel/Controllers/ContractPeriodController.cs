using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Entities.DTOs.Web.Hirman.Tariff.ContractPeriod;
using Entities.enumeration;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, FinanceUser")]
    public class ContractPeriodController : Controller
    {
        private readonly IContractPeriodService contractPeriodService;

        public ContractPeriodController(IContractPeriodService contractPeriodService)
        {
            this.contractPeriodService = contractPeriodService;
        }

        public async Task<IActionResult> List(ContractPeriodFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var data = await contractPeriodService.GetPageContractPeriod(p ?? 1, filter, token);
            return View(data);
        }

        public async Task<IActionResult> Create(CreateContractPeriodDto create, CancellationToken token = default)
        {
            TempData["msg"] = await contractPeriodService.CreateContractPeriod(create, token);
            return RedirectToAction(nameof(List));
        }

        public async Task<IActionResult> Delete(Guid contractPeriodId, CancellationToken token = default)
        {
            await contractPeriodService.DeleteContractPeriod(contractPeriodId, token);
            return RedirectToAction(nameof(List));
        }

        [HttpGet]
        public async Task<IActionResult> GetDeleteInfo(Guid contractPeriodId, CancellationToken token)
        {
            var info = await contractPeriodService.GetDeleteInfo(contractPeriodId, token);
            return PartialView("_DeleteContractPeriod", info);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(Guid contractPeriodId, CancellationToken token = default)
        {
            await contractPeriodService.DeleteContractPeriod(contractPeriodId, token);
            return RedirectToAction(nameof(List));
        }

        [HttpPost]
        public async Task<IActionResult> GetById(Guid contractPeriodId, CancellationToken token = default)
        {
            var period = await contractPeriodService.GetContractPeriodById(contractPeriodId, token);
            return PartialView("_CreateContractPeriod", period);
        }

        [HttpGet]
        public async Task<IActionResult> GetCopyPeriod(Guid originalContractPeriodId, CancellationToken token)
        {
            var originalPeriod = await contractPeriodService.GetContractPeriodById(originalContractPeriodId, token);
            var dto = new CopyContractPeriodDto
            {
                OriginalContractPeriodId = originalContractPeriodId,
                OriginalContractPeriodTitle = originalPeriod?.Title,
                TableSettings = Enum
                    .GetValues(typeof(ContractPeriodCopyableTables))
                    .Cast<ContractPeriodCopyableTables>()
                    .Select(t => new CopyContractPeriodTableSettingDto
                    {
                        Table = t,
                        Selected = true,
                        Percentage = 0
                    })
                    .ToList()
            };
            return PartialView("_CopyContractPeriod", dto);
        }

        [HttpPost]
        public async Task<IActionResult> Copy(CopyContractPeriodDto dto, CancellationToken token)
        {
            var result = await contractPeriodService.CopyContractPeriod(dto, token);
            if (result != null)
            {
                var msg = JsonConvert.DeserializeObject<ResponseMessage>(result);
                return Ok(new { success = false, message = msg?.Message ?? "خطا در کپی دوره" });
            }
            return Ok(new { success = true });
        }
    }
}

