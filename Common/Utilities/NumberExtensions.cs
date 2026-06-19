namespace Common.Utilities
{
    public static class NumberExtensions
    {
        //const string f = "###.##;###.##;0";//بدون جدا کننده
        const string f = "#,##.##;#,##.##;0";//با جدا کننده
        //const string priceType = "  تومان";
        const string priceType = "";
        public static string ToStringNumber(this decimal number)
        {
            return number.ToString(f) + priceType;
        }
        public static string ToStringNumber(this decimal? number)
        {
            if (number == null) return "0";
            return number.Value.ToString(f) + priceType;
        }
        public static string ToStringNumber(this float number)
        {
            return number.ToString(f) + priceType;
        }
        public static string ToStringNumber(this float? number)
        {
            if (number == null) return "0";
            return number.Value.ToString(f) + priceType;
        }

        public static string ToStringNumber(this double number)
        {
            return number.ToString(f) + priceType;
        }
        public static string ToStringNumber(this double? number)
        {
            if (number == null) return "0";
            return number.Value.ToString(f) + priceType;
        }

        public static string ToStringNumber(this long number)
        {
            return number.ToString(f) + priceType;
        }
        public static string ToStringNumber(this long? number)
        {
            if (number == null) return "0";
            return number.Value.ToString(f) + priceType;
        }

        public static string ToStringNumber(this int number)
        {
            return number.ToString(f) + priceType;
        }
        public static string ToStringNumber(this int? number)
        {
            if (number == null) return "0";
            return number.Value.ToString(f) + priceType;
        }
    }
}
