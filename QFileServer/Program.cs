using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using QFileServer;
using QFileServer.Configuration;
using QFileServer.Data;
using QFileServer.StorageManagement;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

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

var odataEDM = ODataEdmModelBuilder.Build();

builder.Services.AddControllers()
    .AddOData(options => options.Filter().OrderBy().Count().SetMaxTop(1000).SkipToken()
    .AddRouteComponents("odata/v1", odataEDM));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "QFileServer API",
        Description = "File server api"
    });

    // maps api/ v1 to v1. skips /$metadata, /$count
    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.RelativePath.Contains('$')) return false;
        if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
        return true;

        var routeAttrs = methodInfo.DeclaringType
            .GetCustomAttributes(true)
            .OfType<ODataRouteComponentAttribute>();
        var versions = routeAttrs.Select(i => i.RoutePrefix.Split('/').Last());
        return versions.Any(v => string.Equals(v, docName, StringComparison.CurrentCultureIgnoreCase));
    });

    options.CustomSchemaIds(type => type.FullName);
    options.OperationFilter<ODataOperationFilter>();
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

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
    app.UseSwaggerUI(options =>
    {
        options.DefaultModelsExpandDepth(-1); // hides schemas dropdown
        options.EnableTryItOutByDefault();
    });
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
