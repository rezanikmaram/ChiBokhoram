using System;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace WebPanel.Infrastructure
{
    public static class FilterPersistenceHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static void Save<T>(HttpContext httpContext, T filter, string key) where T : class
        {
            var json = JsonSerializer.Serialize(filter, JsonOptions);
            httpContext.Response.Cookies.Append(key, json, new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                HttpOnly = true,
                Secure = true
            });
        }

        public static T? Get<T>(HttpContext httpContext, string key) where T : class
        {
            if (httpContext.Request.Cookies.TryGetValue(key, out var json) && !string.IsNullOrEmpty(json))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(json, JsonOptions);
                }
                catch
                {
                    httpContext.Response.Cookies.Delete(key);
                }
            }
            return null;
        }

        public static string Key(HttpContext httpContext, string controller, string action)
        {
            var userId = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            return $"filter_{userId}_{controller}_{action}";
        }
    }
}
