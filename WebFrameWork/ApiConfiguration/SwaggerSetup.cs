using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WebFrameWork.ApiConfiguration
{
    //https://www.thecodebuzz.com/jwt-authorize-swagger-using-ioperationfilter-asp-net-core/
    //https://stackoverflow.com/questions/58197244/swaggerui-with-netcore-3-0-bearer-token-authorization


    public class AddAuthHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var isAuthorized =
                (
                    context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                    && !context.MethodInfo.DeclaringType.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                ) //this excludes controllers with AllowAnonymous attribute in case base controller has Authorize attribute
                || (
                    context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
                    && !context.MethodInfo.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any()
                ); // this excludes methods with AllowAnonymous attribute

            if (!isAuthorized)
                return;

            //operation.Responses.TryAdd("401", new OpenApiResponse { Description = "Unauthorized" });
            //operation.Responses.TryAdd("403", new OpenApiResponse { Description = "Forbidden" });

            var jwtbearerScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "bearer" },
            };

            operation.Security = new List<OpenApiSecurityRequirement> { new OpenApiSecurityRequirement { [jwtbearerScheme] = new string[] { } } };
        }
    }

    /// <summary>
    /// Add enum value descriptions to Swagger
    /// </summary>
    public class SwaggerEnumDocumentFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // add enum descriptions to result models
            foreach (var property in swaggerDoc.Components.Schemas)
            {
                var propertyEnums = property.Value.Enum;
                if (propertyEnums is { Count: > 0 })
                {
                    property.Value.Description += DescribeEnum(propertyEnums, property.Key);
                }
            }

            if (swaggerDoc.Paths.Count <= 0)
            {
                return;
            }

            // add enum descriptions to input parameters
            foreach (var pathItem in swaggerDoc.Paths.Values)
            {
                DescribeEnumParameters(pathItem.Parameters);

                var affectedOperations = new List<OperationType> { OperationType.Get, OperationType.Post, OperationType.Put, OperationType.Patch };

                foreach (var operation in pathItem.Operations)
                {
                    if (affectedOperations.Contains(operation.Key))
                    {
                        DescribeEnumParameters(operation.Value.Parameters);
                    }
                }
            }
        }

        private static void DescribeEnumParameters(IList<OpenApiParameter> parameters)
        {
            if (parameters == null)
                return;

            foreach (var param in parameters)
            {
                if (param.Schema.Reference != null)
                {
                    var enumType = GetEnumTypeByName(param.Schema.Reference.Id);
                    var names = Enum.GetNames(enumType).ToList();

                    param.Description += string.Join(", ", names.Select(name => $"{Convert.ToInt32(Enum.Parse(enumType, name))} - {name}").ToList());
                }
            }
        }

        private static Type GetEnumTypeByName(string enumTypeName)
        {
            if (string.IsNullOrEmpty(enumTypeName))
            {
                return null;
            }

            try
            {
                return AppDomain
                    .CurrentDomain.GetAssemblies()
                    .SelectMany(x => x.GetTypes())
                    .Single(x => x.FullName != null && x.Name == enumTypeName);
            }
            catch (InvalidOperationException e)
            {
                throw new Exception(
                    $"SwaggerDoc: Can not find a unique Enum for specified typeName '{enumTypeName}'. Please provide a more unique enum name."
                );
            }
        }

        private static string DescribeEnum(IEnumerable<IOpenApiAny> enums, string propertyTypeName)
        {
            var enumType = GetEnumTypeByName(propertyTypeName);

            if (enumType == null)
            {
                return null;
            }

            var parsedEnums = new List<OpenApiInteger>();
            foreach (var @enum in enums)
            {
                if (@enum is OpenApiInteger enumInt)
                {
                    parsedEnums.Add(enumInt);
                }
            }

            return string.Join(", ", parsedEnums.Select(x => $"{x.Value} - {Enum.GetName(enumType, x.Value)}"));
        }
    }
}
