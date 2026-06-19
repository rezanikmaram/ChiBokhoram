
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebFrameWork.ApiConfiguration
{
    public static class HangFireConfig
    {
        public static void AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
           
            services.AddHangfire(x => x.UseSqlServerStorage(configuration.GetConnectionString("SqlServerHang") + ";TrustServerCertificate=True"));
            services.AddHangfireServer();

        }
    }
}
