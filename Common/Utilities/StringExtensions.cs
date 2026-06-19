using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Exceptions;
using DNTPersianUtils.Core;
using Newtonsoft.Json.Linq;
using PersianDate.Standard;

namespace Common.Utilities
{
    public static class StringExtensions
    {
        public static bool HasValue(this string value, bool ignoreWhiteSpace = true)
        {
            return ignoreWhiteSpace ? !string.IsNullOrWhiteSpace(value) : !string.IsNullOrEmpty(value);
        }

        public static int ToInt(this string value)
        {
            return Convert.ToInt32(value);
        }

        public static decimal ToDecimal(this string value)
        {
            return Convert.ToDecimal(value);
        }

        public static string ToNumeric(this int value)
        {
            return value.ToString("N0"); //"123,456"
        }

        public static string ToNumeric(this decimal value)
        {
            return value.ToString("N0");
        }

        public static string ToCurrency(this int value)
        {
            //fa-IR => current culture currency symbol => ریال
            //123456 => "123,123ریال"
            return value.ToString("C0");
        }

        public static string ToCurrency(this decimal value)
        {
            return value.ToString("C0");
        }

        public static string En2Fa(this string str)
        {
            return str.Replace("0", "۰")
                .Replace("1", "۱")
                .Replace("2", "۲")
                .Replace("3", "۳")
                .Replace("4", "۴")
                .Replace("5", "۵")
                .Replace("6", "۶")
                .Replace("7", "۷")
                .Replace("8", "۸")
                .Replace("9", "۹");
        }

        public static string Fa2En(this string str)
        {
            return str.Replace("۰", "0")
                .Replace("۱", "1")
                .Replace("۲", "2")
                .Replace("۳", "3")
                .Replace("۴", "4")
                .Replace("۵", "5")
                .Replace("۶", "6")
                .Replace("۷", "7")
                .Replace("۸", "8")
                .Replace("۹", "9")
                //iphone numeric
                .Replace("٠", "0")
                .Replace("١", "1")
                .Replace("٢", "2")
                .Replace("٣", "3")
                .Replace("٤", "4")
                .Replace("٥", "5")
                .Replace("٦", "6")
                .Replace("٧", "7")
                .Replace("٨", "8")
                .Replace("٩", "9");
        }

        public static string FixPersianChars(this string str)
        {
            return str.Replace("ﮎ", "ک")
                .Replace("ﮏ", "ک")
                .Replace("ﮐ", "ک")
                .Replace("ﮑ", "ک")
                .Replace("ك", "ک")
                .Replace("ي", "ی")
                .Replace(" ", " ")
                .Replace("‌", " ")
                .Replace("ھ", "ه"); //.Replace("ئ", "ی");
        }

        public static string CleanString(this string str)
        {
            return str.Trim().FixPersianChars().Fa2En().NullIfEmpty();
        }

        public static string NullIfEmpty(this string str)
        {
            return str?.Length == 0 ? null : str;
        }

        public static DateTimeOffset ToDateTime(this string date)
        {
            DateTime utcTime1 = date.ToGregorianDateTime().Value;
            utcTime1 = DateTime.SpecifyKind(utcTime1, DateTimeKind.Utc);
            DateTimeOffset utcTime2 = utcTime1;
            return utcTime2;
        }

        public static int GetHoure(this string h)
        {
            return int.Parse(h.Split(':')[0]);
        }

        public static int GetMinute(this string h)
        {
            return int.Parse(h.Split(':')[1]);
        }

        public static string ToPersianDate(this DateTime date)
        {
            return date.ToFa();
        }

        public static string ToPersianDate2(this DateTimeOffset date)
        {
            return date.ToShortPersianDateString();
        }

        public static string ToPersianDateTime(this DateTime date)
        {
            return date.ToFa("YYYY/MM/dd hh:mm");
        }

        public static string GetInnerHtmltext(this string data)
        {
            string decode = System.Web.HttpUtility.HtmlDecode(data);
            Regex objRegExp = new Regex("<(.|\n)+?>");
            string replace = objRegExp.Replace(decode, "");
            return replace.Trim("\t\r\n ".ToCharArray());
        }

        public static Boolean GuidHasValue(this Guid guid)
        {
            if (guid != null && guid != Guid.Parse("00000000-0000-0000-0000-000000000000"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string ToText(this int digit)
        {
            string txt = digit.ToString();
            int length = txt.Length;

            string[] a1 = new string[10] { "-", "یک", "دو", "سه", "چهار", "پنح", "شش", "هفت", "هشت", "نه" };
            string[] a2 = new string[10] { "ده", "یازده", "دوازده", "سیزده", "چهارده", "پانزده", "شانزده", "هفده", "هجده", "نوزده" };
            string[] a3 = new string[10] { "-", "ده", "بیست", "سی", "چهل", "پنجاه", "شصت", "هفتاد", "هشتاد", "نود" };
            string[] a4 = new string[10] { "-", "یک صد", "دویست", "سیصد", "چهارصد", "پانصد", "ششصد", "هفصد", "هشصد", "نهصد" };

            string result = "";
            bool isDahegan = false;

            for (int i = 0; i < length; i++)
            {
                string character = txt[i].ToString();
                switch (length - i)
                {
                    case 7: //میلیون
                        if (character != "0")
                        {
                            result += a1[Convert.ToInt32(character)] + " میلیون و ";
                        }
                        else
                        {
                            result = result.TrimEnd('و', ' ');
                        }
                        break;
                    case 6: //صدهزار
                        if (character != "0")
                        {
                            result += a4[Convert.ToInt32(character)] + " و ";
                        }
                        else
                        {
                            result = result.TrimEnd('و', ' ');
                        }
                        break;
                    case 5: //ده هزار
                        if (character == "1")
                        {
                            isDahegan = true;
                        }
                        else if (character != "0")
                        {
                            result += a3[Convert.ToInt32(character)] + " و ";
                        }
                        break;
                    case 4: //هزار
                        if (isDahegan == true)
                        {
                            result += a2[Convert.ToInt32(character)] + " هزار و ";
                            isDahegan = false;
                        }
                        else
                        {
                            if (character != "0")
                            {
                                result += a1[Convert.ToInt32(character)] + " هزار و ";
                            }
                            else
                            {
                                result = result.TrimEnd('و', ' ');
                            }
                        }
                        break;
                    case 3: //صد
                        if (character != "0")
                        {
                            result += a4[Convert.ToInt32(character)] + " و ";
                        }
                        break;
                    case 2: //ده
                        if (character == "1")
                        {
                            isDahegan = true;
                        }
                        else if (character != "0")
                        {
                            result += a3[Convert.ToInt32(character)] + " و ";
                        }
                        break;
                    case 1: //یک
                        if (isDahegan == true)
                        {
                            result += a2[Convert.ToInt32(character)];
                            isDahegan = false;
                        }
                        else
                        {
                            if (character != "0")
                                result += a1[Convert.ToInt32(character)];
                            else
                                result = result.TrimEnd('و', ' ');
                        }
                        break;
                }
            }
            return result;
        }

        /// <summary>
        /// dateStr = "1398/05/31"
        /// </summary>
        /// <param name="dateStr"></param>
        /// <returns></returns>
        public static DateTimeOffset ToDate(this string dateStr)
        {
            //var date = dateStr.Substring(0, 4) + "/" + dateStr.Substring(4, 2) + "/" +
            //                 dateStr.Substring(6, 2);

            return dateStr.ToEn();
        }

        public static DateTimeOffset? ToDateNulable(this string dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
                return null;

            return dateStr.ToEn();
        }

        public static DateTimeOffset ToMiladiDate(this string date)
        {
            return date != null ? date.ToDateTime() : DateTimeOffset.UtcNow;
        }

        public static EnumFileType GetExtentionType(this string ext)
        {
            var img = new string[] { ".jpg", ".jpeg", ".png", ".tiff", ".gif", ".bmp" };
            var vid = new string[]
            {
                ".mkv",
                ".flv",
                ".vob",
                ".ogg",
                ".avi",
                ".mov",
                ".qt",
                ".wmv",
                ".mp4",
                ".mpg",
                ".mpeg",
                ".3gp",
                ".dvx",
                ".ogv",
                ".ts",
            };
            var sound = new string[] { ".aac", ".m4a", ".mp3", ".mpc", ".ogg", ".wav", ".wma" };

            ext = ext.ToLower();

            if (img.Contains(ext))
                return EnumFileType.Image;
            if (vid.Contains(ext))
                return EnumFileType.Video;
            if (sound.Contains(ext))
                return EnumFileType.Audio;

            return EnumFileType.Unknown;
        }

        public static DateTimeOffset? ToGregorianDateTimeOffset2(this string dateTimePersion)
        {
            try
            {
                var dateAndTime = dateTimePersion.Split(" ");
                var time = dateAndTime[1].Split(":");
                if (int.Parse(time[0]) == 24)
                    dateTimePersion = $"{dateAndTime[0]} 00:{time[1]}:{time[2]}";
            }
            catch { }

            return dateTimePersion.ToGregorianDateTimeOffset();
        }

        public static string ExtractPropertyFromJson(string json, string propertyName)
        {
            JObject jsonObject = JObject.Parse(json);

            JToken token = jsonObject[propertyName];
            if (token != null)
            {
                return token.ToString();
            }
            else
            {
                throw new Exception($"Property '{propertyName}' not found in the JSON object.");
            }
        }

        public static JObject ImportPropertyToJson(string json, string propertyName, object newValue)
        {
            JObject jsonObject = JObject.Parse(json);

            jsonObject[propertyName].Replace(newValue as JToken);

            return jsonObject;
        }

        public static string GetMessageForSms(this string[] template, string code)
        {
            for (int i = 0; i < template.Length; i++)
            {
                if (template[i].Contains("???"))
                {
                    template[i] = template[i].Replace("???", code);
                }
            }

            return $"{string.Join(Environment.NewLine, template)}";
        }

        public static string GetValidFileName(this string fileName, char replaceValidChar = '_')
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, replaceValidChar);
            }

            return fileName;
        }
    }
}
