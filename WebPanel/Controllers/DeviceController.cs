using Common.CustomAttribute;
using Common.enumeration;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    //دستگاه های FPN
    [Authorize(Roles = "admin, adminOnePort")]
    public class DeviceController : Controller
    {
        public DeviceController(IDeviceService deviceService)
        {
            DeviceService = deviceService;
        }

        public IDeviceService DeviceService { get; }


        #region Device
        public async Task<IActionResult> Index(CancellationToken token = default, int? p = null)
        {
            int pagenumaber = p ?? 1;
            var result = await DeviceService.GetDevices(pagenumaber, token);
            return View(result);
        }
        public async Task<IActionResult> CreateDevice(int deviceId, CancellationToken token = default)
        {
            if (deviceId > 0)
            {
                var dto = await DeviceService.GetDeviceById(deviceId, token);
                return View(dto);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateDevice(CreateDeviceDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                var result = await DeviceService.AddOrUpdate(dto, token);
                if (result != null)
                {
                    TempData["msg"] = result;
                    return View(dto);
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(dto);
            }
        }

        public async Task<IActionResult> DeleteDevice(int deviceId, CancellationToken token = default)
        {
            TempData["msg"] = await DeviceService.DeleteDevice(deviceId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GetDeviceById(int deviceId, CancellationToken token = default)
        {
            var find = await DeviceService.GetDeviceById(deviceId, token);
            return PartialView("_CreateDevice", find);
        }

        public async Task<IActionResult> ActivationDevice(int deviceId, CancellationToken token = default)
        {
            await DeviceService.ActivationDevice(deviceId, token);
            return RedirectToAction(nameof(Index));
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueDeviceSn(string sn, int id, CancellationToken token = default)
        {
            var result = await DeviceService.UniqueDeviceSnValidation(sn, id, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }
        #endregion


        #region FpnDevice Log
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> FpnDeviceLog(CancellationToken token = default,
            int? p = null)
        {
            var list = await DeviceService.GetFpnDeviceLog(p ?? 1, token);
            return View(list);
        }
        #endregion


        #region Device radio

        public async Task<IActionResult> DeviceRadios(int deviceId, CancellationToken token = default, int? p = null)
        {
            if (deviceId <= 0) return RedirectToAction(nameof(Index));

            var device = await DeviceService.GetDeviceById(deviceId, token);
            if (device == null) return RedirectToAction(nameof(Index));

            ViewBag.DeviceName = device.Name;
            var result = await DeviceService.GetDeviceRadios(deviceId, token);

            return View(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateDeviceRadio(CreateDeviceRadioDto dto, CancellationToken token = default)
        {
            ModelState.Remove("Id");
            if (ModelState.IsValid)
            {
                TempData["msg"] = await DeviceService.AddOrUpdateRadio(dto, token);

                return RedirectToAction(nameof(DeviceRadios), new { deviceId = dto.DeviceId });
            }
            else
            {
                return RedirectToAction(nameof(DeviceRadios), new { deviceId = dto.DeviceId });
            }
        }

        public async Task<IActionResult> RemoveDeviceRadio(int deviceRadioId, int deviceId, CancellationToken token = default)
        {
            TempData["msg"] = await DeviceService.DeleteDeviceRadio(deviceRadioId, token);
            TempData.Keep("msg");
            return RedirectToAction(nameof(DeviceRadios), new { deviceId = deviceId });
        }

        public async Task<IActionResult> ActivationDeviceRadio(int deviceRadioId, int deviceId, CancellationToken token = default)
        {
            await DeviceService.ActivationDeviceRadio(deviceRadioId, token);
            return RedirectToAction(nameof(DeviceRadios), new { deviceId });
        }

        public async Task<IActionResult> GetDeviceRadioById(int deviceRadioId, CancellationToken token = default)
        {
            var find = await DeviceService.GetDeviceRadioById(deviceRadioId, token);
            return PartialView("_CreateRadio", find);
        }

        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyIpAddress(string ip)
        {
            var result = new IPAddressAttribute().MyIpIsValid(ip);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        //اعتبارسنجی در سمت کلاینت
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> UniqueCodeNameForFpn(string CodeNameForFpn, int deviceId,
            int id, FpnDirectionType direction, CancellationToken token = default)
        {
            var result = await DeviceService.UniqueCodeNameForFpnValidation(CodeNameForFpn, deviceId, id, direction, token);
            if (string.IsNullOrWhiteSpace(result))
                return Json(true);
            else
                return Json(result);
        }

        #endregion

    }
}
