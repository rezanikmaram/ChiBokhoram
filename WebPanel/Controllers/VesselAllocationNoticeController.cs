using System;
using System.Threading;
using System.Threading.Tasks;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin,adminOnePort")]
    public class VesselAllocationNoticeController : Controller
    {
        private readonly IVesselAllocationNoticeService _service;

        public VesselAllocationNoticeController(IVesselAllocationNoticeService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index(VesselAllocationNoticeFilterDto filter, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPageList(filter, cancellationToken);
            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _service.GetById(id, cancellationToken);
            return View(entity);
        }
    }
}
