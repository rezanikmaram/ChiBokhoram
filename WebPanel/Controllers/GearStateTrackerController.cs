using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Common.enumeration;
using Common.enumeration.Log;
using Common.Exceptions;
using Entities.DTOs.Public;
using Entities.DTOs.Public.LogBook;
using Entities.DTOs.Web;
using Entities.DTOs.Web.Hirman;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Services.PublicService;
using Services.WebService.Abstract;

namespace WebPanel.Controllers
{
    //دستگاه های FPN
    [Authorize(Roles = "admin, adminOnePort")]
    public class GearStateTrackerController : Controller
    {
        private readonly IGearService _gearService;
        private readonly IGearStateTrackerService _gearStateTrackerService;
        private readonly IGearStateService _gearStateService;
        private readonly IGearStateTrackerReportService _gearStateTrackerReportService;

        public GearStateTrackerController(
            IGearService gearService,
            IGearStateTrackerService gearStateTrackerService,
            IGearStateService gearStateService,
            IGearStateTrackerReportService gearStateTrackerReportService
        )
        {
            _gearService = gearService;
            _gearStateTrackerService = gearStateTrackerService;
            _gearStateService = gearStateService;
            _gearStateTrackerReportService = gearStateTrackerReportService;
        }

        #region GearStateTracker
        public async Task<IActionResult> Index(int previousMonth = 0, CancellationToken token = default)
        {
            var currentDate = DateTime.Now;
            var calculateDate = currentDate.AddMonths(-previousMonth);

            var dateItems = await PopulateAvailablePersianDate(calculateDate, token);
            var states = await _gearStateService.GetGearStateSelectList();
            PersianCalendar persianCalendar = new PersianCalendar();
            var model = new LogBookCalendarDto()
            {
                CurrentMonthTitle = GetMonthTitle(persianCalendar.GetMonth(calculateDate)),
                CurrentYear = persianCalendar.GetYear(calculateDate).ToString(),
                DateItems = dateItems,
                DaysOfWeeks = GetDaysOfWeeks(),
                PreviousMonth = previousMonth,
            };
            ViewBag.States = states;
            return View(model);
        }

        public async Task<IActionResult> LoadGearStateTracker(DateTime targetDate, CancellationToken token = default)
        {
            var items = await _gearStateTrackerService.GetAllGearWithCurrentlyStatus(targetDate, token);
            var states = await _gearStateService.GetGearStateSelectList();
            var defaultItem = new SelectListItem() { Text = "انتخاب وضعیت", Value = "" };
            var finallStateList = new List<SelectListItem>();
            finallStateList.Add(defaultItem);
            finallStateList.AddRange(states);
            var model = new LoadGearStateTrackerDto()
            {
                Gears = items,
                States = finallStateList,
                TargetDate = targetDate,
            };
            return PartialView("_LoadGearStateTracker", model);
        }

        public async Task UpdateGearStateTracker(UpdateGearStateTrackerDto dto, CancellationToken token = default)
        {
            await _gearStateTrackerService.UpdateGearStateTracker(dto, token);
        }

        private async Task<List<LogBookPersianCalendarDto>> PopulateAvailablePersianDate(DateTimeOffset passDate, CancellationToken token = default)
        {
            PersianCalendar persianCalendar = new PersianCalendar();

            // گرفتن تاریخ امروز
            DateTime currentPassDate = passDate.LocalDateTime;

            // استخراج سال، ماه و روز از تاریخ امروز به صورت شمسی
            var PersianYear = persianCalendar.GetYear(currentPassDate);
            var PersianMonth = persianCalendar.GetMonth(currentPassDate);
            var CurrentPersianDay = persianCalendar.GetDayOfMonth(currentPassDate);

            var DaysInMonth = persianCalendar.GetDaysInMonth(PersianYear, PersianMonth, 0);

            // بدست آوردن روز اول ماه جاری
            DateTime firstDayOfMonth = persianCalendar.ToDateTime(PersianYear, PersianMonth, 1, 0, 0, 0, 0);

            // بدست آوردن روز آخر ماه جاری
            DateTime lastDayOfMonth = persianCalendar.ToDateTime(PersianYear, PersianMonth, DaysInMonth, 0, 0, 0, 0);

            var stateTrackers = await _gearStateTrackerService.GetGearStateTrackerFilteredByDate(firstDayOfMonth, lastDayOfMonth, token);

            var gearCount = await _gearService.GetGearCount(true, token);

            var dateItems = new List<LogBookPersianCalendarDto>();

            var iterateDate = firstDayOfMonth;
            while (iterateDate <= lastDayOfMonth)
            {
                var stateTrackerCount = stateTrackers.FirstOrDefault(x => x.TargetDate == iterateDate);
                var finishedStateTrackerCount = stateTrackerCount != null ? stateTrackerCount.TotalState : 0;
                var alreadyInProcessCount = gearCount - finishedStateTrackerCount;

                dateItems.Add(
                    new LogBookPersianCalendarDto()
                    {
                        GregorianDate = iterateDate,
                        PersianDay = persianCalendar.GetDayOfMonth(iterateDate),
                        PersianMonth = persianCalendar.GetMonth(iterateDate),
                        PersianYear = persianCalendar.GetYear(iterateDate),
                        DayOfWeek = GetPersianDayOfWeek(iterateDate),
                        FinishedStateTrackerCount = finishedStateTrackerCount,
                        AlreadyInProcessCount = alreadyInProcessCount,
                    }
                );
                iterateDate = iterateDate.AddDays(1);
            }

            return dateItems;
        }

        private PersianDaysOfWeek GetPersianDayOfWeek(DateTimeOffset dateTime)
        {
            PersianCalendar persianCalendar = new PersianCalendar();

            // استخراج تاریخ و زمان محلی از DateTimeOffset
            DateTime localDateTime = dateTime.LocalDateTime;

            // استخراج روز هفته به میلادی
            DayOfWeek dayOfWeek = localDateTime.DayOfWeek;
            PersianDaysOfWeek result = PersianDaysOfWeek.Saturday;
            switch (dayOfWeek)
            {
                case DayOfWeek.Sunday:
                    result = PersianDaysOfWeek.Sunday;
                    break;
                case DayOfWeek.Monday:
                    result = PersianDaysOfWeek.Monday;
                    break;
                case DayOfWeek.Tuesday:
                    result = PersianDaysOfWeek.Tuesday;
                    break;
                case DayOfWeek.Wednesday:
                    result = PersianDaysOfWeek.Wednesday;
                    break;
                case DayOfWeek.Thursday:
                    result = PersianDaysOfWeek.Thursday;
                    break;
                case DayOfWeek.Friday:
                    result = PersianDaysOfWeek.Friday;
                    break;
                case DayOfWeek.Saturday:
                    result = PersianDaysOfWeek.Saturday;
                    break;
                default:
                    break;
            }
            return result;
        }

        private List<DaysOfWeekDto> GetDaysOfWeeks()
        {
            List<DaysOfWeekDto> result = new();
            foreach (var field in typeof(PersianDaysOfWeek).GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();

                if (displayAttribute != null)
                {
                    result.Add(
                        new DaysOfWeekDto { Code = Convert.ToInt32(field.GetValue(typeof(PersianDaysOfWeek))), Title = displayAttribute.Name }
                    );
                }
            }
            return result;
        }

        private string GetMonthTitle(int month)
        {
            string title = "";
            switch (month)
            {
                case 1:
                    title = "فروردین";
                    break;
                case 2:
                    title = "اردیبهشت";
                    break;
                case 3:
                    title = "خرداد";
                    break;
                case 4:
                    title = "تیر";
                    break;
                case 5:
                    title = "مرداد";
                    break;
                case 6:
                    title = "شهریور";
                    break;
                case 7:
                    title = "مهر";
                    break;
                case 8:
                    title = "آبان";
                    break;
                case 9:
                    title = "آذر";
                    break;
                case 10:
                    title = "دی";
                    break;
                case 11:
                    title = "بهمن";
                    break;
                case 12:
                    title = "اسفند";
                    break;
                default:
                    title = "";
                    break;
            }
            return title;
        }

        public async Task UpdateFromPreviousDate(DateTime targetDate, CancellationToken token = default)
        {
            await _gearStateTrackerService.UpdateFromPreviousDate(targetDate, token);
        }

        public async Task<IActionResult> GetGearStateList(GearStateTrackerFilterDto filter, int? p = null, CancellationToken token = default)
        {
            var result = await _gearStateTrackerService.GetGearStateList(filter, p, false, token);
            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> GetGearStateReportList(
            GearStateTrackerFilterDto filter = null,
            ReportType reportType = ReportType.Excel,
            CancellationToken token = default
        )
        {
            try
            {
                var fileDto = await _gearStateTrackerReportService.GetGearStateReportList(filter, token, reportType);

                if (fileDto == null)
                {
                    return Ok();
                }

                return File(fileDto.FileStream, "application/octet-stream", fileDto.FileTitleWithExtention);
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }

        #endregion
    }
}
