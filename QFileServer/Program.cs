using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QFileServer;
using QFileServer.Configuration;
using QFileServer.Data;
using QFileServer.StorageManagement;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var qFileServiceConfiguration = new QFileServerServiceConfiguration();
qFileServiceConfiguration.FileServerRootPath = builder.Configuration
    .GetSection("QFileServiceConfiguration")
    .GetValue<string>("FileServerRootPath");

builder.Services.AddLogging();
builder.Services.AddSingleton(qFileServiceConfiguration);
builder.Services.AddSingleton<IStorageDirectorySelector, DateStorageDirectorySelector>();

builder.Services.AddDbContext<QFileServerDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

builder.Services.AddScoped<QFileServerRepository>();
builder.Services.AddAutoMapper(cfg => cfg.AddProfile(new AutomapperProfile()));
builder.Services.AddScoped<QFileServerService>();

builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

builder.Services.AddControllers()
    .AddOData(options => options.Filter().OrderBy().Count().SetMaxTop(1000).SkipToken());

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// db init and creation
using (var serviceScope = app.Services.GetService<IServiceScopeFactory>()!.CreateScope())
{
    var context = serviceScope.ServiceProvider.GetRequiredService<QFileServerDbContext>();
    context.Database.Migrate();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
