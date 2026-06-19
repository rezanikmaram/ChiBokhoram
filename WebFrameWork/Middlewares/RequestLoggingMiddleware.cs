using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Common.enumeration;
using Data.DbContext;
using Entities.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WebFramework.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private const int BodySizeLimit = 32_768; // Limit request/response body snapshots
        private const int HeaderSizeLimit = 8_192;

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;
        private readonly IServiceScopeFactory _scopeFactory;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        public async Task Invoke(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var controllerAction = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

            if (controllerAction == null || controllerAction.ControllerName.ToLower() == "error")
            {
                await _next(context);
                return;
            }

            var logEntry = CreateInitialLog(context);

            Exception capturedException = null;

            context.Request.EnableBuffering();

            if (context.Request.Body.CanSeek)
            {
                logEntry.RequestBody = await ReadStreamAsync(context.Request.Body, BodySizeLimit);
            }

            logEntry.RequestHeaders = SerializeHeaders(context.Request.Headers, HeaderSizeLimit);

            var originalBodyStream = context.Response.Body;
            await using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            logEntry.ControllerName = controllerAction.ControllerName;
            logEntry.ActionName = controllerAction.ActionName;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                capturedException = ex;
                logEntry.ErrorMessage = ex.Message;
                logEntry.ExceptionDetail = ex.ToString();
            }
            finally
            {
                logEntry.ResponseTime = DateTimeOffset.Now;
                logEntry.DurationMs = (long)(logEntry.ResponseTime - logEntry.RequestTime).TotalMilliseconds;
                logEntry.StatusCode = context.Response.StatusCode;
                logEntry.IsSuccess = capturedException == null && context.Response.StatusCode < StatusCodes.Status400BadRequest;
                logEntry.ResponseHeaders = SerializeHeaders(context.Response.Headers, HeaderSizeLimit);

                if (responseBody.CanSeek)
                {
                    logEntry.ResponseBody = await ReadStreamAsync(responseBody, BodySizeLimit);
                }

                context.Response.Body = originalBodyStream;
                responseBody.Seek(0, SeekOrigin.Begin);
                await responseBody.CopyToAsync(originalBodyStream);

                await PersistLogAsync(logEntry);
            }

            if (capturedException != null)
            {
                ExceptionDispatchInfo.Capture(capturedException).Throw();
            }
        }

        private LogShipment CreateInitialLog(HttpContext context)
        {
            var log = new LogShipment
            {
                Method = context.Request.Method,
                Scheme = context.Request.Scheme,
                Host = context.Request.Host.Value,
                Path = context.Request.Path.Value,
                QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                Url = context.Request.GetDisplayUrl(),
                RequestTime = DateTimeOffset.Now,
                ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                RecordStatus = DataStatus.Active,
            };

            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim =
                    context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                    ?? context.User.FindFirst("sub")
                    ?? context.User.FindFirst("userId");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
                {
                    log.UserId = userId;
                }

                log.UserName = context.User.Identity.Name;
            }

            return log;
        }

        private async Task PersistLogAsync(LogShipment logEntry)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await dbContext.LogShipments.AddAsync(logEntry);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist request log for {Path}", logEntry.Path);
            }
        }

        private static async Task<string> ReadStreamAsync(Stream stream, int limit)
        {
            if (!stream.CanSeek)
            {
                return null;
            }

            stream.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
            var builder = new StringBuilder(Math.Max(0, limit));
            var buffer = new char[Math.Max(1, Math.Min(1024, Math.Max(1, limit)))];
            int remaining = Math.Max(0, limit);
            int read;

            while (remaining > 0 && (read = await reader.ReadAsync(buffer, 0, Math.Min(buffer.Length, remaining))) > 0)
            {
                builder.Append(buffer, 0, read);
                remaining -= read;
            }

            if (remaining <= 0 && await reader.ReadAsync(buffer, 0, 1) > 0)
            {
                builder.Append("...");
            }

            stream.Seek(0, SeekOrigin.Begin);
            return builder.ToString();
        }

        private static string SerializeHeaders(IHeaderDictionary headers, int limit)
        {
            if (headers == null || !headers.Any())
            {
                return null;
            }

            var dictionary = headers.ToDictionary(h => h.Key, h => (IReadOnlyList<string>)h.Value.ToArray());
            var json = JsonSerializer.Serialize(dictionary);
            return Truncate(json, limit);
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
            {
                return value;
            }

            return value.Substring(0, maxLength);
        }
    }
}
