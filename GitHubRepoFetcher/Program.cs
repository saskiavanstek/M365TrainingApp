using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace GitHubRepoFetcher
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
                    webBuilder.UseStartup<Startup>(); // Ensure Startup is used
                });
    }
}
// Note: Ensure you have the necessary NuGet packages installed for ASP.NET Core