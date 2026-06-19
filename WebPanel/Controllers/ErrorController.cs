using Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WebPanel.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        // Handles exceptions -> /Error or /Error/Index
        [Route("Error")]
        [Route("Error/Index")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            // Optionally read exception details (not shown to user)
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (feature?.Error != null)
            {
                ErrorLog.SaveError(feature.Error, $"-- {feature.Path}");
            }
            Response.StatusCode = 500;
            return View("500", feature.Error);
        }

        // Handles status code pages -> /Error/{statusCode}
        [Route("Error/{statusCode:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult HandleStatusCode(int statusCode)
        {
            Response.StatusCode = statusCode;
            switch (statusCode)
            {
                case 400:
                    return View("400");
                case 404:
                    return View("404");
                default:
                    return View("500");
            }
        }

        // Handles status code pages -> /Error/StatusCode/{statusCode}
        [Route("Error/StatusCode/{statusCode:int}")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Status(int statusCode)
        {
            Response.StatusCode = statusCode;
            switch (statusCode)
            {
                case 400:
                    return View("400");
                case 404:
                    var reexecute = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
                    // Optional: log 404s
                    if (reexecute != null)
                    {
                        //ErrorLog.SaveLog($"404 Not Found: {reexecute.OriginalPath}{reexecute.OriginalQueryString}");
                    }
                    return View("404");
                default:
                    return View("500");
            }
        }
    }
}
