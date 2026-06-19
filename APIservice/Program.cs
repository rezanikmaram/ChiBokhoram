using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


//#if (RELEASE)
//[assembly: ArmDot.Client.ObfuscateControlFlow]
//[assembly: ArmDot.Client.VirtualizeCode]
//#endif
namespace APIservice
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
