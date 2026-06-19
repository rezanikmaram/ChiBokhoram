using Common;
using Common.CustomAttribute;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, adminOnePort")]
    public class SimcardController : Controller
    {
        public SimcardController(ISimcardService simcardService, IDeviceService deviceService)
        {
            SimcardService = simcardService;
            DeviceService = deviceService;
        }

        public ISimcardService SimcardService { get; }
        public IDeviceService DeviceService { get; }

        public async Task<IActionResult> Index(SimcardFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await SimcardService.GetSimcardPageList(p ?? 1, filter, token);
            result.CreateSimcardDto = new CreateSimcardDto();
            result.Devices = await DeviceService.GetDeviceSelectList(token);

            return View(result);
        }

        [HttpGet]
        public async Task<IActionResult> Table(SimcardFilterDto filter = null, CancellationToken token = default, int? p = null)
        {
            var result = await SimcardService.GetSimcardPageList(p ?? 1, filter, token);
            return PartialView("_SimcardTable", result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSimcard(CreateSimcardDto createSimcardDto, CancellationToken token = default)
        {
            if (createSimcardDto?.Id == Guid.Empty) ModelState.Remove("createSimcardDto.Id");
            bool isAjax = Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest";
            
            if (ModelState.IsValid)
            {
                var result = await SimcardService.AddOrUpdate(createSimcardDto, token);
                
                if (isAjax)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        return Json(new { success = false, message = result });
                    }
                    return Json(new { success = true, message = "عملیات با موفقیت انجام شد" });
                }
                
                if (!string.IsNullOrEmpty(result))
                {
                    TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, result));
                }
                return RedirectToAction(nameof(Index));
            }

            if (isAjax)
            {
                return Json(new { success = false, message = "ثبت ناموفق، اطلاعات را درست وارد کنید" });
            }
            
            TempData["msg"] = JsonConvert.SerializeObject(new ResponseMessage(ResponseMessageType.Error, "ثبت ناموفق، اطلاعات را درست وارد کنید"));
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> GetSimcardById(Guid simcardId, CancellationToken token = default)
        {
            var find = await SimcardService.GetSimcardById(simcardId);
            if (find == null) return RedirectToAction(nameof(Index));

            return Json(find);
        }


        [HttpPost]
        public async Task<IActionResult> ActivationSimcard(Guid simcardId, CancellationToken token = default)
        {
            await SimcardService.ActivationSimcard(simcardId, token);
            
            if (Request.Headers["X-Requested-With"].ToString() == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetSimcardCount(CancellationToken token = default)
        {
            var count = await SimcardService.GetSimcardCount(token);
            return Json(new { count = count });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSimcard(Guid simcardId, CancellationToken token = default)
        {
            try
            {
                var msg = await SimcardService.DeleteById(simcardId, token);
                var message = JsonConvert.DeserializeObject<ResponseMessage>(msg);
                return Json(new { success = message.Type == ResponseMessageType.Success, message = message.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطا در حذف: " + ex.Message });
            }
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyIpAddress(CreateSimcardDto createSimcardDto)
        {
            var result = new IPAddressAttribute().MyIpIsValid(createSimcardDto.Ip);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }
    }
}
