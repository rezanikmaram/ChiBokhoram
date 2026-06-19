using System;
using System.IO;
using Common.Exceptions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

//#if (RELEASE)
//[assembly: ArmDot.Client.ObfuscateControlFlow]
//[assembly: ArmDot.Client.VirtualizeCode]
//#endif

namespace WebPanel
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var host = Host.CreateDefaultBuilder(args)
                    //.UseServiceProviderFactory(new AutofacServiceProviderFactory())
                    .ConfigureWebHostDefaults(webHostBuilder =>
                    {
                        webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory()).UseIISIntegration()
                        //.UseKestrel()
                        .UseStartup<Startup>();
                    })
                    .Build();

                host.Run();
            }
            catch (Exception ex)
            {
                ErrorLog.SaveError(ex);
                throw;
            }
        }
    }
}
