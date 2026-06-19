using Entities.DTOs.Web;
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
    [Authorize(Roles = "admin")]
    public class PortController : Controller
    {
        private readonly IPortService portService;

        public SignInManager<ApplicationUser> SignInManager { get; }

        public PortController(IPortService portService,
            SignInManager<ApplicationUser> signInManager
            )
        {
            this.portService = portService;
            SignInManager = signInManager;
        }
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> CreatePorts(CreatePortDto dto,
            CancellationToken token = default)
        {
            if (dto.Id == Guid.Empty) ModelState.Remove("Id");
            if (ModelState.IsValid)
                await portService.AddOrUpdatePort(dto, token);

            return RedirectToAction(nameof(Ports));
        }

        public async Task<IActionResult> Ports(CancellationToken token = default)
        {
            var result = await portService.GetPorts(token);
            return View(result);
        }


        [HttpPost]
        public async Task<IActionResult> GetCreatePortsPageById(Guid portsId,
        CancellationToken token = default)
        {
            var find = await portService.GetPortDto(portsId, token);

            return PartialView("_CreatePort", find);
        }

        public async Task<IActionResult> ActivationPort(Guid portId, CancellationToken token = default)
        {
            var result = await portService.ActivationPort(portId);
             await SignInManager.SignOutAsync();

            return RedirectToAction(nameof(Ports));
        }
    }
}
