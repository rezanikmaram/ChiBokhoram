using DNTPersianUtils.Core;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Common.Utilities
{
    public static class TimeTools
    {
        public static string GetTimeTitle(this DateTimeOffset? date)
        {
            if (date == null) return "لحظاتی پیش";
            return date.Value.GetTimeTitle();
        }

        public static string GetTimeTitle(this DateTimeOffset date)
        {
            var def = DateTimeOffset.Now - date;
            if (def.Days >= 1) return $"{def.Days} روز پیش";
            if (def.Hours >= 1) return $"{def.Hours} ساعت پیش";
            if (def.Minutes >= 3) return $"{def.Minutes} دقیقه قبل";
            if (def.Minutes >= 0 && def.Minutes <= 2) return "لحظاتی پیش";

            return "لحظاتی پیش";
        }

        public static string GetDayOfWeekFa(this DateTimeOffset date)
        {
            var day = new PersianCalendar().GetDayOfWeek(date.DateTime);
            switch (day)
            {
                case DayOfWeek.Sunday:
                    return "یک شنبه";
                case DayOfWeek.Monday:
                    return "دو شنبه";
                case DayOfWeek.Tuesday:
                    return "سه شنبه";
                case DayOfWeek.Wednesday:
                    return "چهار شنبه";
                case DayOfWeek.Thursday:
                    return "پنج شنبه";
                case DayOfWeek.Friday:
                    return "جمعه";
                case DayOfWeek.Saturday:
                    return "شنبه";
                default:
                    return "";
            }
        }

        public static byte GetDayOfMonthNumberFa(this DateTimeOffset date)
        {
            return (byte)new PersianCalendar().GetDayOfMonth(date.DateTime);
        }

        public static string GetMonthNameFa(this DateTimeOffset date)
        {
            int persianMonth = new PersianCalendar().GetMonth(date.DateTime);

            switch (persianMonth)
            {
                case 1:
                    return "فروردین";
                case 2:
                    return "اردیبهشت";
                case 3:
                    return "خرداد";
                case 4:
                    return "تیر";
                case 5:
                    return "مرداد";
                case 6:
                    return "شهریور";
                case 7:
                    return "مهر";
                case 8:
                    return "آبان";
                case 9:
                    return "آذر";
                case 10:
                    return "دی";
                case 11:
                    return "بهمن";
                case 12:
                    return "اسفند";
                default:
                    return "";
            }
        }

        public static string GetMonthNameFa(this int persianMonth)
        {
            switch (persianMonth)
            {
                case 1:
                    return "فروردین";
                case 2:
                    return "اردیبهشت";
                case 3:
                    return "خرداد";
                case 4:
                    return "تیر";
                case 5:
                    return "مرداد";
                case 6:
                    return "شهریور";
                case 7:
                    return "مهر";
                case 8:
                    return "آبان";
                case 9:
                    return "آذر";
                case 10:
                    return "دی";
                case 11:
                    return "بهمن";
                case 12:
                    return "اسفند";
                default:
                    return "";
            }
        }

        public static List<KeyValuePair<int, string>> GetPersianMonthKeyValue()
        {
            return new List<KeyValuePair<int, string>> {
                new KeyValuePair<int, string> (1 , "فروردین"),
                new KeyValuePair<int, string> (2 , "اردیبهشت"),
                new KeyValuePair<int, string> (3 , "خرداد"),

                new KeyValuePair<int, string> (4 , "تیر"),
                new KeyValuePair<int, string> (5 , "مرداد"),
                new KeyValuePair<int, string> (6 , "شهریور"),

                new KeyValuePair<int, string> (7 , "مهر"),
                new KeyValuePair<int, string> (8 , "آبان"),
                new KeyValuePair<int, string> (9 , "آذر"),

                new KeyValuePair<int, string> (10 , "دی"),
                new KeyValuePair<int, string> (11 , "بهمن"),
                new KeyValuePair<int, string> (12 , "اسفند"),
            };
        }

        public static SelectList GetPersianMonth()
        {
            return new SelectList(GetPersianMonthKeyValue(), "Key", "Value");
        }

        public static List<KeyValuePair<int, string>> GetYearsKeyValue(int minYear, int? maxYear = null)
        {
            var result = new List<KeyValuePair<int, string>>();
            maxYear ??= DateTimeOffset.Now.ToPersianYearMonthDay().Year;
            for (int year = minYear; year <= maxYear; year++)
            {
                result.Add(new KeyValuePair<int, string>(year, $"{year}"));
            }

            return result;
        }

        public static SelectList GetYears(int minYear, int? maxYear = null)
        {
            return new SelectList(GetYearsKeyValue(minYear, maxYear), "Key", "Value");
        }

        #region فصل ها
        public static List<KeyValuePair<int, string>> GetSeasonKeyValue()
        {
            return new List<KeyValuePair<int, string>> {
                new KeyValuePair<int, string> (1 , "بهار"),
                new KeyValuePair<int, string> (2 , "تابستان"),

                new KeyValuePair<int, string> (3 , "پاییز"),
                new KeyValuePair<int, string> (4 , "زمستان")
            };
        }

        public static SelectList GetSeason()
        {
            return new SelectList(GetSeasonKeyValue(), "Key", "Value");
        }

        public static string GetSeasonName(this int season)
        {
            switch (season)
            {
                case 1:
                    return "بهار";    // فصل 1: بهار
                case 2:
                    return "تابستان"; // فصل 2: تابستان
                case 3:
                    return "پاییز";   // فصل 3: پاییز
                case 4:
                    return "زمستان";  // فصل 4: زمستان
                default:
                    return "فصل نامعتبر"; // در صورتی که شماره فصل معتبر نباشد
            }
        }

        /// <summary>
        /// دریافت شماره فصل جاری
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentSeason()
        {
            var nowFa = DateTimeOffset.Now.ToPersianYearMonthDay();
            var season = (nowFa.Month - 1) / 3 + 1;

            return season;
        }

        public static Tuple<string, string> GetStartEndFromSeason(int years, int season)
        {
            var pc = new PersianCalendar();
            switch (season)
            {
                case 1:
                    return Tuple.Create($"{years}/01/01", $"{years}/03/31");

                case 2:
                    return Tuple.Create($"{years}/03/01", $"{years}/06/31");

                case 3:
                    return Tuple.Create($"{years}/07/01", $"{years}/09/30");

                case 4:
                    var days = pc.GetDaysInMonth(years, 12, PersianCalendar.PersianEra);
                    return Tuple.Create($"{years}/10/01", $"{years}/12/{days}");

                default:
                    return Tuple.Create($"{years}/01/01", $"{years}/03/31");
            }
        }

        #endregion



        public static string GetElapsedTime(this TimeSpan timeSpan)
        {
            var elapsed = "";
            if (Math.Floor(timeSpan.TotalDays) > 0)
                elapsed = $"{Convert.ToInt32(timeSpan.TotalDays):#,#} روز";

            if (timeSpan.Hours > 0)
            {
                if (elapsed?.Length > 0)
                    elapsed += " و ";
                elapsed += $"{timeSpan.Hours} ساعت";
            }


            if (timeSpan.Minutes > 0)
            {
                if (elapsed?.Length > 0)
                    elapsed += " و ";
                elapsed += $"{timeSpan.Minutes} دقیقه";
            }


            if (string.IsNullOrEmpty(elapsed))
                elapsed = "کمتر از یک دقیقه";
            return elapsed;
        }

    }
}
