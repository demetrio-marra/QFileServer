using Microsoft.AspNetCore.Http.Features;
using QFileServer.Mvc.Configuration;
using QFileServer.Mvc.Services;

namespace QFileServer.Mvc
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAutoMapper(cfg => cfg.AddProfile(new AutomapperProfile()));

            builder.Services.AddHttpClient<QFileServerApiService>(cfg =>
            {
                cfg.BaseAddress = new Uri(builder.Configuration.GetValue<string>(Constants.QFileServerHttpClientBaseUrlKey));
            });

            builder.Services.AddSession();

            // Configure the maximum request body size before calling UseRouting, UseEndpoints, etc.
            // 1_000_000_000 represents the new size limit in bytes (about 1GB in this example).
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = long.MaxValue;
            });

            builder.Services.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = null; // 1_000_000_000;
            });

            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Limits.MaxRequestBodySize = null; // 1_000_000_000;
            });

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}