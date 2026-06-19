using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Common.CustomAttribute;
using Common.enumeration;

namespace Common.Utilities
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetEnumValues<T>(this T input)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException();
            }

            return Enum.GetValues(input.GetType()).Cast<T>();
        }

        public static IEnumerable<T> GetEnumFlags<T>(this T input)
            where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException();
            }

            foreach (var value in Enum.GetValues(input.GetType()))
            {
                if ((input as Enum).HasFlag(value as Enum))
                {
                    yield return (T)value;
                }
            }
        }

        public static string ToDisplay(this Enum value, DisplayProperty property = DisplayProperty.Name)
        {
            Assert.NotNull(value, nameof(value));

            var attribute = value.GetType().GetField(value.ToString()).GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();

            if (attribute == null)
            {
                return value.ToString();
            }

            var propValue = attribute.GetType().GetProperty(property.ToString()).GetValue(attribute, null);
            return propValue.ToString();
        }

        public static Type GetResourceTypeFromEnum<TEnum>(this TEnum value)
            where TEnum : struct, Enum // محدودیت به اِنوم
        {
            // دریافت MemberInfo مربوط به مقدار اِنوم
            MemberInfo memberInfo = typeof(TEnum).GetMember(value.ToString())[0];

            // دریافت DisplayAttribute از MemberInfo
            DisplayAttribute displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();

            // برگرداندن ResourceType یا null
            return displayAttribute?.ResourceType;
        }

        public static Dictionary<int, string> ToDictionary(this Enum value)
        {
            return Enum.GetValues(value.GetType()).Cast<Enum>().ToDictionary(p => Convert.ToInt32(p), q => ToDisplay(q));
        }

        public static Dictionary<T, string> ToDictionary<T>()
            where T : Enum
        {
            var enumDict = new Dictionary<T, string>();

            foreach (T value in Enum.GetValues(typeof(T)))
            {
                string displayName = value.ToDisplay(DisplayProperty.Name);
                enumDict[value] = displayName;
            }
            return enumDict;
        }

        public static string ToDisplayStringWithCode(this Enum value, string title)
        {
            var dict = value.ToDictionary();
            var items = dict.Select(kvp => $"{kvp.Key}={kvp.Value}");
            return $"{title} ({string.Join(", ", items)})";
        }

        private static string GetEnumDisplayName<T>(T value)
            where T : Enum
        {
            var enumType = typeof(T);
            var memberInfo = enumType.GetMember(value.ToString());

            if (memberInfo.Length > 0)
            {
                var attributes = memberInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);

                if (attributes.Length > 0)
                {
                    return ((DisplayAttribute)attributes[0]).Name;
                }
            }

            return value.ToString();
        }

        public static TEnum? GetEnumValueFromDisplayName<TEnum>(string displayName)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
                if (displayAttribute != null && displayAttribute.Name == displayName)
                {
                    return (TEnum)field.GetValue(null);
                }
            }
            return null;
        }

        public static string GetEnumDescription(this Enum enumValue)
        {
            var field = enumValue.GetType().GetField(enumValue.ToString());
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
            {
                return attribute.Description;
            }
            throw new ArgumentException("Item not found.", nameof(enumValue));
        }

        public static string ToCurrencyCode(this CurrencyType currency)
        {
            var field = currency.GetType().GetField(currency.ToString());
            var attr = field?.GetCustomAttribute<CurrencyCodeAttribute>();

            return attr?.Code ?? currency.ToString();
        }
    }

    public enum DisplayProperty
    {
        Description,
        GroupName,
        Name,
        Prompt,
        ShortName,
        Order,
    }
}
