using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class CommonController : Controller
    {
        private readonly ISeoReciveService seoReciveService;

        public CommonController(ISeoReciveService seoReciveService)
        {
            this.seoReciveService = seoReciveService;
        }


        [HttpPost]
        public async Task<IActionResult> AdminPhone(CreateSeoReciveInfoDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                await seoReciveService.AddOrUpdate(dto, token);
                return RedirectToAction(nameof(AdminPhones));
            }
            else
                return RedirectToAction(nameof(AdminPhones));

        }

        public async Task<IActionResult> AdminPhones(CancellationToken token = default)
        {
            var res = await seoReciveService.GetSeoReciveInfo(token);
            return View(res);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message, CancellationToken token = default)
        {
            if (!string.IsNullOrEmpty(message))
            {
                TempData["msg"] = await seoReciveService.SendSms(message);
                TempData.Keep("msg");
            }

            return RedirectToAction(nameof(AdminPhones));
        }

        public async Task<IActionResult> GetAdminPhoneById(int adminPhoneId, CancellationToken token = default)
        {
            var find = await seoReciveService.GetSeoReceive(adminPhoneId, token);
            return PartialView("_CreateAdminPhone", find);
        }

        public async Task<IActionResult> RemoveAdminPhone(int adminPhoneId, CancellationToken token = default)
        {
            TempData["msg"] = await seoReciveService.RemoveAdminPhone(adminPhoneId, token);
            return RedirectToAction(nameof(AdminPhones));
        }


        public async Task<IActionResult> ActivationAdminPhone(int adminPhoneId, CancellationToken token = default)
        {
            await seoReciveService.ActivationAdminPhone(adminPhoneId, token);
            return RedirectToAction(nameof(AdminPhones));
        }
    }
}
