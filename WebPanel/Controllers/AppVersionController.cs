using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.enumeration.Hirman;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Helper;
using Services.PublicService;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    [Authorize(Roles = "admin, AppVersion")]
    public class AppVersionController : Controller
    {
        public AppVersionController(IAppVersionWebService appVersionWebService, ISettingService settingService)
        {
            AppVersionWebService = appVersionWebService;
            SettingService = settingService;
        }

        public IAppVersionWebService AppVersionWebService { get; }
        public ISettingService SettingService { get; }

        public async Task<IActionResult> ManageApp(CancellationToken token = default)
        {
            var result = await AppVersionWebService.GetApps(token);
            return View(result);
        }

        // Handle requests up to configured max size (default: 50 MB)
        // This value is controlled by SystemSettingKey.MaxUploadFileSizeMB setting
        // To change: Update the setting in تنظیمات سامانه page
        [RequestSizeLimit(52428800)]
        public async Task<IActionResult> UpdateNewApp(AppVersionFileDto appFile, CancellationToken token)
        {
            if (appFile == null || appFile?.File == null)
                return Json(new { success = false, message = "فایلی انتخاب نشده است" });

            var result = await AppVersionWebService.UploadNewApp(appFile, token);
            TempData["msg"] = result;
            return Json(new { success = true, message = result });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAppVersionInfo(AppVersionInfoDto appVersion, CancellationToken token)
        {
            await AppVersionWebService.SaveAppVersionInfo(appVersion, token);
            return RedirectToAction(nameof(ManageApp));
        }
    }
}
