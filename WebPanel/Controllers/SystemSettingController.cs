using Common.enumeration.Hirman;
using Entities.DTOs.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.WebService.Abstract;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WebPanel.Controllers
{
    /// <summary>
    /// کنترلر مدیریت تنظیمات سامانه
    /// </summary>
    [Authorize(Roles = "admin")]
    public class SystemSettingController : Controller
    {
        private readonly ISystemSettingService _systemSettingService;

        public SystemSettingController(ISystemSettingService systemSettingService)
        {
            _systemSettingService = systemSettingService;
        }

        /// <summary>
        /// صفحه لیست تنظیمات
        /// </summary>
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var settings = await _systemSettingService.GetAllSettingsAsync(cancellationToken);
            ViewBag.Definitions = _systemSettingService.GetAllDefinitions()
                .OrderBy(d => (int)d.Subsystem)
                .ThenBy(d => d.DisplayName)
                .ToList();
            return View(settings.OrderBy(s => (int)s.Subsystem).ToList());
        }

        /// <summary>
        /// دریافت اطلاعات یک تنظیم برای ویرایش
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetSetting(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Json(new { success = false, message = "کلید تنظیم مشخص نشده است" });
            }

            var setting = await _systemSettingService.GetSettingAsync(key, cancellationToken);
            if (setting == null)
            {
                // اگر تنظیم در دیتابیس وجود ندارد، تعریف آن را برگردان
                if (System.Enum.TryParse<SystemSettingKey>(key, out var settingKey))
                {
                    setting = new SystemSettingViewDto
                    {
                        Key = key,
                        DisplayName = settingKey.GetDisplayName(),
                        Value = string.Empty,
                        ValueType = settingKey.GetValueType(),
                        Description = settingKey.GetDescription(),
                        IsMultiline = settingKey.IsMultiline(),
                        PredefinedValues = settingKey.GetPredefinedValues()
                    };
                }
            }

            return Json(new { success = true, data = setting });
        }

        /// <summary>
        /// ذخیره تنظیم
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SaveSetting([FromBody] CreateSystemSettingDto dto, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "اطلاعات وارد شده نامعتبر است" });
            }

            try
            {
                var result = await _systemSettingService.SaveSettingAsync(dto.Key, dto.Value, cancellationToken);
                if (result)
                {
                    return Json(new { success = true, message = "تنظیم با موفقیت ذخیره شد" });
                }
                return Json(new { success = false, message = "خطا در ذخیره تنظیم" });
            }
            catch (System.ArgumentException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// حذف تنظیم
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteSetting(string key, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                return Json(new { success = false, message = "کلید تنظیم مشخص نشده است" });
            }

            var result = await _systemSettingService.DeleteSettingAsync(key, cancellationToken);
            if (result)
            {
                return Json(new { success = true, message = "تنظیم با موفقیت حذف شد" });
            }
            return Json(new { success = false, message = "تنظیم یافت نشد" });
        }

        /// <summary>
        /// دریافت لیست کلیدهای موجود
        /// </summary>
        [HttpGet]
        public IActionResult GetSettingKeys()
        {
            var definitions = _systemSettingService.GetAllDefinitions();
            return Json(new { success = true, data = definitions });
        }

        /// <summary>
        /// اعتبارسنجی مقدار تنظیم
        /// </summary>
        [HttpPost]
        public IActionResult ValidateSettingValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key) || !System.Enum.TryParse<SystemSettingKey>(key, out var settingKey))
            {
                return Json(new { success = false, message = "کلید تنظیم نامعتبر است" });
            }

            var isValid = settingKey.ValidateValue(value, out var errorMessage);
            return Json(new { success = isValid, message = errorMessage });
        }
    }
}
