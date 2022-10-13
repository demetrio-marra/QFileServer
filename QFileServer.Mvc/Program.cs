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

            // Add services to the container.
            builder.Services.AddHttpClient(Constants.QFileServerHttpClientName, 
                c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>(Constants.QFileServerHttpClientBaseUrlKey)));
            builder.Services.AddHttpClient(Constants.ODataQFileServerHttpClientName,
                c => c.BaseAddress = new Uri(builder.Configuration.GetValue<string>(Constants.ODataQFileServerHttpClientBaseUrlKey)));

            builder.Services.AddSingleton<IQFileServerApiService, QFileServerApiService>();

            builder.Services.AddSession();

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