using System;
using System.ComponentModel.DataAnnotations;
using Common.Utilities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace WebPanel.TagHelpers
{
    [HtmlTargetElement("price-input")]
    public class PriceInputTagHelper : TagHelper
    {
        public PriceInputTagHelper()
            : base() { }

        public ModelExpression AspFor { get; set; }
        public string Placeholder { get; set; }
        public int DecimalPlaces { get; set; } = 2; // kept for validation, but display trims zeros

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var value = AspFor.Model?.ToString();
            var name = AspFor.Name;
            var id = name.Replace(".", "_");
            var metadata = AspFor.Metadata;

            // Detect if the property type is nullable
            var modelType = AspFor.ModelExplorer.ModelType;
            bool isNullable = modelType.IsGenericType && modelType.GetGenericTypeDefinition() == typeof(Nullable<>);

            decimal? numericValue = null;
            if (!string.IsNullOrEmpty(value))
            {
                decimal parsed;
                if (decimal.TryParse(value, out parsed))
                {
                    numericValue = parsed;
                }
            }
            // If not nullable, default to 0
            if (!isNullable && !numericValue.HasValue)
            {
                numericValue = 0;
            }

            // Format without unnecessary decimal places
            string formattedValue;
            if (!numericValue.HasValue)
            {
                formattedValue = "";
            }
            else if (numericValue.Value == 0)
            {
                formattedValue = "0";
            }
            else
            {
                if (numericValue.Value % 1 == 0)
                {
                    formattedValue = numericValue.Value.ToString("N0");
                }
                else
                {
                    formattedValue = numericValue.Value.ToString($"N{DecimalPlaces}").TrimEnd('0').TrimEnd('.');
                }
            }

            var idInput2 = "PriceText";

            // Display input (text with formatting)
            var priceTextInput = new TagBuilder("input");
            priceTextInput.Attributes.Add("id", $"{id}{idInput2}");
            priceTextInput.Attributes.Add("type", "text");
            priceTextInput.Attributes.Add("class", "form-control rounded");
            priceTextInput.Attributes.Add("inputmode", "decimal");
            priceTextInput.Attributes.Add("pattern", @"^\d{1,3}(,\d{3})*(\.\d{1,2})?$");
            priceTextInput.Attributes.Add("data-nullable", isNullable ? "true" : "false");

            if (!string.IsNullOrEmpty(Placeholder))
                priceTextInput.Attributes.Add("placeholder", Placeholder);
            else
                priceTextInput.Attributes.Add("placeholder", "0");

            priceTextInput.Attributes.Add("value", formattedValue);
            priceTextInput.Attributes.Add(
                "onkeyup",
                $"changeToNumber(this, '{id}{idInput2}', '{id}', {DecimalPlaces}, {isNullable.ToString().ToLower()})"
            );
            priceTextInput.Attributes.Add(
                "onblur",
                $"formatOnBlur(this, '{id}{idInput2}', '{id}', {DecimalPlaces}, {isNullable.ToString().ToLower()})"
            );
            priceTextInput.Attributes.Add("onkeydown", "restrictToNumberAndDecimal(event)");

            // Hidden numeric input (for form submission)
            var priceNumberInput = new TagBuilder("input");
            priceNumberInput.Attributes.Add("name", name);
            priceNumberInput.Attributes.Add("type", "number");
            priceNumberInput.Attributes.Add("class", "form-control d-none");
            priceNumberInput.Attributes.Add("id", id);
            priceNumberInput.Attributes.Add("step", "0.01");
            priceNumberInput.Attributes.Add(
                "value",
                numericValue.HasValue ? numericValue.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : ""
            );
            priceNumberInput.Attributes.Add("data-nullable", isNullable ? "true" : "false");

            // Transfer all other attributes to hidden input (for validation)
            foreach (var attribute in context.AllAttributes)
            {
                if (attribute.Name.Equals("asp-for") || attribute.Name.Equals("placeholder"))
                    continue;
                priceNumberInput.Attributes.Add(attribute.Name, attribute.Value?.ToString());
            }

            // Add validation attributes from DataAnnotations
            if (metadata.ValidatorMetadata.Count > 0)
            {
                priceNumberInput.Attributes.Add("data-val", "true");
                foreach (var validator in metadata.ValidatorMetadata)
                {
                    if (validator is RangeAttribute rangeAttr)
                    {
                        priceNumberInput.Attributes.Add("data-val-range", rangeAttr.ErrorMessage);
                        priceNumberInput.Attributes.Add("data-val-range-min", rangeAttr.Minimum.ToString());
                        priceNumberInput.Attributes.Add("data-val-range-max", rangeAttr.Maximum.ToString());
                    }
                    else if (validator is RequiredAttribute requiredAttr)
                    {
                        priceNumberInput.Attributes.Add("data-val-required", requiredAttr.ErrorMessage);
                    }
                }
            }

            output.Content.AppendHtml(priceTextInput);
            output.Content.AppendHtml(priceNumberInput);
        }
    }
}
