using Data.DbContext;
using Data.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebFramework.CustomMapping;
using WebFramework.Middlewares;

namespace APIservice
{
    public class Startup
    {
        readonly string MyAllowSpecificOrigins = "_MyAllowSubdomainPolicy";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy(
                    name: MyAllowSpecificOrigins,
                    builder =>
                    {
                        builder.AllowAnyOrigin().SetIsOriginAllowedToAllowWildcardSubdomains().AllowAnyHeader().AllowAnyMethod();
                    }
                );
            });

            services.AddHttpContextAccessor();
            services.InitializeAutoMapper();

            services.AddControllers();

            var connStr = Configuration.GetConnectionString("SqlServerConnection") + ";TrustServerCertificate=True";
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(connStr, sqlOptions =>
                {
                    sqlOptions.CommandTimeout(200);
                });
            });

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCustomExceptionHandler();
            app.UseCors(MyAllowSpecificOrigins);
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
