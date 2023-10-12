using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using Contracts.Interfaces;
using Entities.DataTransferObjects;
using LoggerService;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using NLog;
using Service.DataShaping;
using static CompanyEmployees.Extensions.ServiceExtensions;

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

builder.Services.AddScoped<ValidationFilterAttribute>();

builder .Services.AddScoped<ValidateCompanyExistsAttribute>();

builder.Services.AddScoped<ValidateEmployeeForCompanyExistsAttribute>();

builder.Services.AddScoped<IDataShaper<EmployeeDto>, DataShaper<EmployeeDto>> ();

builder.Services.AddCustomerMediaTypes();

builder.Services.AddScoped<ValidateMediaTypeAttribute>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
	options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddControllers(config =>
{
	config.RespectBrowserAcceptHeader = true;
	config.ReturnHttpNotAcceptable = true;
}).AddNewtonsoftJson()
.AddXmlDataContractSerializerFormatters()
.AddCustomCSVFormatter();


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
