using Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Threading.Tasks;

namespace WebFramework.Filters
{
    public class ShowErrorPageTypeAttribute : TypeFilterAttribute
    {
        public ShowErrorPageTypeAttribute() : base(typeof(LogsExceptionFilterAttribute))
        {
        }
    }

    public class LogsExceptionFilterAttribute : ExceptionFilterAttribute
    {


        public string ViewName { get; set; } = "Index";
        public Type ExceptionType { get; set; } = null;

        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            try
            {
                if (context.Exception is OperationCanceledException || context.Exception is TaskCanceledException)
                {
                    context.ExceptionHandled = true;
                    context.Result = new StatusCodeResult(400);
                    ErrorLog.SaveLog(context.Exception.Message + "");
                }
                var routeValues = context.ActionDescriptor.RouteValues;
                var controllerName = routeValues["Controller"];
                var actionName = routeValues["Action"];
                //var pageName = routeValues["Page"];
                //var areaName = routeValues["Area"];
                var exception = context.Exception;
                var request = context.HttpContext.Request;
                var stackTrace = context.Exception.StackTrace;
                var message = string.Empty;
                message = exception.InnerException != null ? exception.InnerException.Message : exception.Message;


                ErrorLog.SaveError(context.Exception, $"-- {controllerName}/{actionName}");

                //var spMsg = int.Parse(message.Split(",")[0]);
                //switch (spMsg)
                //{
                //    case 400:
                //        context.Result = new StatusCodeResult(spMsg);
                //        break;
                //    case 403:
                //        context.Result = new StatusCodeResult(spMsg);
                //        break;
                //    case 404:
                //        context.Result = new StatusCodeResult(spMsg);
                //        break;
                //    default:
                //        context.Result = new StatusCodeResult(500);
                //        break;
                //}

            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                context.Result = new StatusCodeResult(500);
            }
        }
    }
}
