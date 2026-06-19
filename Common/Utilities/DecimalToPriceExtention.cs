using System;

namespace Common.Utilities
{
    public static class ToPriceExtention
    {
        // ---------------------- Format Constants ----------------------
        private const string FormatInteger = "#,##0"; // بدون اعشار
        private const string FormatDecimal3 = "#,##0.###"; // تا ۳ رقم اعشار
        private static readonly string f = "#,##;-##,###;0";

        //private static readonly string pos = "  تومان";
        // ---------------------- decimal ----------------------
        public static string ToPrice(this decimal value, PriceType priceType = PriceType.NoType)
        {
            string formatted = (value == Math.Floor(value)) ? FormatInteger : FormatDecimal3;

            return value.ToString(formatted) + priceType.GetTitel();
        }

        public static string ToPrice(this decimal? value, PriceType priceType = PriceType.NoType)
        {
            return value == null ? $"0{priceType.GetTitel()}" : value.Value.ToPrice(priceType);
        }

        // ---------------------- double ----------------------
        public static string ToPrice(this double value, PriceType priceType = PriceType.NoType)
        {
            string formatted = (value == Math.Floor(value)) ? FormatInteger : FormatDecimal3;

            return value.ToString(formatted) + priceType.GetTitel();
        }

        public static string ToPrice(this double? value, PriceType priceType = PriceType.NoType)
        {
            return value == null ? $"0{priceType.GetTitel()}" : value.Value.ToPrice(priceType);
        }

        // ---------------------- float ----------------------
        public static string ToPrice(this float value, PriceType priceType = PriceType.NoType)
        {
            string formatted = (value == MathF.Floor(value)) ? FormatInteger : FormatDecimal3;

            return value.ToString(formatted) + priceType.GetTitel();
        }

        public static string ToPrice(this float? value, PriceType priceType = PriceType.NoType)
        {
            return value == null ? $"0{priceType.GetTitel()}" : value.Value.ToPrice(priceType);
        }

        public static string ToPrice(this long value, PriceType priceType = PriceType.NoType) => value.ToString(f) + priceType.GetTitel();

        public static string ToPrice(this long? value, PriceType priceType = PriceType.NoType)
        {
            return value == null ? $"0{priceType.GetTitel()}" : value?.ToString(f) + priceType.GetTitel();
        }

        public static string ToPrice(this int value, PriceType priceType = PriceType.NoType) => value.ToString(f) + priceType.GetTitel();

        public static string ToPrice(this int? value, PriceType priceType = PriceType.NoType)
        {
            return value == null ? $"0{priceType.GetTitel()}" : value?.ToString(f) + priceType.GetTitel();
        }

        private static string GetTitel(this PriceType priceType)
        {
            switch (priceType)
            {
                case PriceType.NoType:
                    return "";
                case PriceType.Riyal:
                    return "  ريال";
                case PriceType.Toman:
                    return "  تومان";
                default:
                    return "  تومان";
            }
        }

        public enum PriceType
        {
            NoType,
            Riyal,
            Toman,
        }
        //public static string ToPrice(this decimal value, CurrencyType currencyType)
        //{
        //    switch (currencyType)
        //    {
        //        case CurrencyType.Dollar:
        //            return $"{CurrencyType.Dollar.ToDisplay()}{value.ToString("#,##.##;##,###-;0")}";
        //        case CurrencyType.Rial:
        //            return $"{value.ToString("#,##.##;##,###-;0")} {CurrencyType.Rial.ToDisplay()}";
        //        default:
        //            return $"{CurrencyType.Dollar.ToDisplay()}{value.ToString("#,##.##;##,###-;0")}";
        //    }
        //}

        //    private static CultureInfo GetCultureInfo(CurrencyType currencyType)
        //{
        //    switch (currencyType)
        //    {
        //        case  CurrencyType.Dollar:
        //            return new  CultureInfo("en-US");
        //        case  CurrencyType.Rial:
        //            return new CultureInfo("ar-SA");
        //        default:
        //            return new CultureInfo("en-US");
        //    }
        //}
    }
}
