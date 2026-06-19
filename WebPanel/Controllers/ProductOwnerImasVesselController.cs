using Common.enumeration;
using Entities.DTOs.Public;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.PublicService.Abstract;
using Services.PublicService.Concrete;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class ProductOwnerImasVesselController : Controller
    {
        public ProductOwnerImasVesselController(
            IProductOwnerImasVesselService productOwnerImasVesselService)
        {
            ProductOwnerImasVesselService = productOwnerImasVesselService;
        }

        public IProductOwnerImasVesselService ProductOwnerImasVesselService { get; }

        public async Task<IActionResult> VesselList(ProductOwnerImasVesselFilterDto filter,
            int? p = null, CancellationToken token = default)
        {
            var data = await ProductOwnerImasVesselService.GetList(filter, p ?? 1, token);

            return View(data);
        }

        public async Task<IActionResult> DeleteRequest(Guid imasVesselId, CancellationToken token = default)
        {
            await ProductOwnerImasVesselService.DeleteRequest(imasVesselId, token);

            return RedirectToAction(nameof(VesselList));
        }


        public async Task<IActionResult> AcceptRequest(Guid imasVesselId, CancellationToken token = default)
        {
            await ProductOwnerImasVesselService.AcceptRequest(imasVesselId, token);

            return RedirectToAction(nameof(VesselList));
        }
    }
}
