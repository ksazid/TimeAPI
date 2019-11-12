using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Reflection;

namespace TimeAPI.API
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
