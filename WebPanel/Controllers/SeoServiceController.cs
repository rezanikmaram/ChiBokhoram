using Entities.DTOs.Web.Hirman;
using Entities.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class SeoServiceController : Controller
    {
        public SeoServiceController(
            ISeoBarshomarService seoBarshomarService,
            ISeoWarehouseService seoWarehouseService,
            ISeoCompletedService seoCompletedService,
            ISeoService seoService,
            UserManager<ApplicationUser> userManager)
        {
            SeoBarshomarService = seoBarshomarService;
            SeoWarehouseService = seoWarehouseService;
            SeoCompletedService = seoCompletedService;
            SeoService = seoService;
            UserManager = userManager;
        }

        public ISeoBarshomarService SeoBarshomarService { get; }
        public ISeoWarehouseService SeoWarehouseService { get; }
        public ISeoCompletedService SeoCompletedService { get; }
        public ISeoService SeoService { get; }
        public UserManager<ApplicationUser> UserManager { get; }

        public async Task<IActionResult> Counter(
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoBarshomarService.GetSeoCounterUnloadService(new SeoServiceFilterDto { SeoId = seoId }, pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Counter(
            SeoServiceFilterDto filterDto,
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoBarshomarService.GetSeoCounterUnloadService(filterDto, pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        public async Task<IActionResult> CounterOneTime(
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoBarshomarService.GetSeoCounterUnloadOneTimeService(new SeoServiceFilterDto { SeoId = seoId },
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> CounterOneTime(
            SeoServiceFilterDto filterDto,
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoBarshomarService.GetSeoCounterUnloadOneTimeService(filterDto,
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        public async Task<IActionResult> Warehouse(
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoWarehouseService.GetSeoWarehouseUnloadService(new SeoServiceFilterDto { SeoId = seoId },
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Warehouse(
             SeoServiceFilterDto filterDto,
            Guid seoId,
            CancellationToken token = default,
            int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoWarehouseService.GetSeoWarehouseUnloadService(filterDto,
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            return View(result);
        }

        public async Task<IActionResult> Completed(
           Guid seoId,
           CancellationToken token = default,
           int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoCompletedService.GetSeoCompletedService(new SeoCompletedServiceFilterDto { SeoId = seoId },
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            ViewBag.seoState = await SeoService.GetSeoState(seoId, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Completed(
            SeoCompletedServiceFilterDto filterDto,
           Guid seoId,
           CancellationToken token = default,
           int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await SeoCompletedService.GetSeoCompletedService(filterDto,
                pagenumaber, token);
            ViewBag.seoId = seoId;
            ViewBag.seo = await SeoService.GetCreateSeoDto(seoId, token);
            ViewBag.seocode = await SeoService.GetSeoCode(seoId, token);
            ViewBag.seotype = await SeoService.GetSeoTransportType(seoId, token);
            ViewBag.seoState = await SeoService.GetSeoState(seoId, token);
            return View(result);
        }

        public async Task<IActionResult> StartToComplet(
           Guid? seoId,
           CancellationToken token = default)
        {
            if (seoId == null)
            {
                return RedirectToAction(nameof(Completed));
            }

            var userFind = await UserManager.GetUserAsync(User);
            await SeoCompletedService.StartServiceToCompleted(userFind.Id, seoId.Value, token);
            return RedirectToAction(nameof(Completed), new { seoId });
        }


        public async Task<IActionResult> FindOldService(
           Guid? seoId,
           CancellationToken token = default)
        {
            if (seoId == null)
            {
                return RedirectToAction(nameof(Completed));
            }

            var userFind = await UserManager.GetUserAsync(User);
            await SeoCompletedService.FindOldServiceAndOptimization(userFind.Id, seoId.Value, token);
            return RedirectToAction(nameof(Completed), new { seoId });
        }
    }
}
