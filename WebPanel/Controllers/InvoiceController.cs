using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.enumeration;
using Common.Exceptions;
using DNTPersianUtils.Core;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using Services.WebService.Concrete;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports")]
    public class InvoiceController : Controller
    {
        public InvoiceController(
            IInvoiceService invoiceService,
            IInvoicePublicService invoicePublicService,
            ICurrentUserService<UserPanelInfoDto> currentUserService
        )
        {
            InvoiceService = invoiceService;
            InvoicePublicService = invoicePublicService;
            CurrentUserService = currentUserService;
        }

        public IInvoiceService InvoiceService { get; }
        public IInvoicePublicService InvoicePublicService { get; }
        public ICurrentUserService<UserPanelInfoDto> CurrentUserService { get; }

        [Authorize(Roles = "admin, adminOnePort, incomeReports")]
        public async Task<IActionResult> Index(InvoiceReportFilterDto filter, CancellationToken token = default)
        {
            await ConfigFilterSpecialIncome(filter);

            var data = await InvoiceService.GetInvoicePage(filter, token);

            return View(data);
        }

        private async Task ConfigFilterSpecialIncome(InvoiceReportFilterDto filter)
        {
            var user = await CurrentUserService.GetCurrentUser();
            var now = DateTimeOffset.Now.ToShortPersianDateString();

            filter ??= new InvoiceReportFilterDto { Page = 1 };
            filter.FilterDate ??= new FilterDateDto { FromDate = now, ToDate = now };
            filter.PortId = user.PortId;
        }

        public async Task<IActionResult> GetIncomeReport(InvoiceReportFilterDto filter = null, CancellationToken token = default)
        {
            await ConfigFilterSpecialIncome(filter);

            var fileDto = await InvoicePublicService.GetIncomeReport(filter, token);

            if (fileDto == null)
                return Ok();

            return File(fileDto.FileStream, "application/vnd.ms-excel", fileDto.FileTitleWithExtention);
        }
    }
}
