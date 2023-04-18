using CompanyEmployees.Extensions;
using Contracts.Interfaces;
using LoggerService;
using Microsoft.AspNetCore.HttpOverrides;
using NLog;

LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));



var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var logger = new LoggerManager();


// Add services to the container.
builder.Services.ConfigureCors();
builder.Services.ConfigureIISIntegration();
builder.Services.ConfigureLoggerService();
builder.Services.ConfigureSqlContext(configuration);
builder.Services.ConfigureRepositoryManager();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers(config =>
{
	config.RespectBrowserAcceptHeader = true;
	config.ReturnHttpNotAcceptable = true;
}).AddXmlDataContractSerializerFormatters()
.AddCustomCSVFormatter();


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

if(app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
} else
{
	app.UseHsts();
}

app.ConfigureExceptionHandler(logger);
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("CorsPolicy");

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
	ForwardedHeaders = ForwardedHeaders.All
});

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
