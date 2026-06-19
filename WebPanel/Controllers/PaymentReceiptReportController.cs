using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort, incomeReports")]
    public class PaymentReceiptReportController : Controller
    {
        private readonly IPaymentReceiptReportService _reportService;

        public PaymentReceiptReportController(IPaymentReceiptReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(PaymentReceiptReportWebFilterDto filter, CancellationToken token = default)
        {
            var result = await _reportService.GetPage(filter, token);
            return View(result);
        }
    }
}
