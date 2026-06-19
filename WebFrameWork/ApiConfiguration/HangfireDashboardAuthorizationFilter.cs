using Hangfire.Dashboard;

namespace WebFrameWork.ApiConfiguration
{
    public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        //https://docs.hangfire.io/en/latest/configuration/using-dashboard.html#configuring-authorization
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Allow all authenticated users to see the Dashboard (potentially dangerous).
            return httpContext.User.Identity.IsAuthenticated;
        }
    }
}
