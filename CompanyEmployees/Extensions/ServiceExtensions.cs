using Contracts.Interfaces;
using Entities;
using LoggerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;
using System.Net.Http.Headers;

namespace CompanyEmployees.Extensions
{

	public static class ServiceExtensions
	{
		public static void ConfigureCors(this IServiceCollection services) =>
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy", builder => 
				builder.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader());
			});

		public static void ConfigureIISIntegration(this IServiceCollection services)
		{
			services.Configure<IISOptions>(options =>
			{
			});
		}

		public static void ConfigureLoggerService(this IServiceCollection services) =>
			services.AddScoped<ILoggerManager, LoggerManager>();

		public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration) => 
			services.AddDbContext<RepositoryContext>(opts => opts.UseSqlServer(configuration.GetConnectionString("workSqlConnection"), 
				b => b.MigrationsAssembly("CompanyEmployees")));

		public static void ConfigureRepositoryManager(this IServiceCollection services) =>
			services.AddScoped<IRepositoryManager, RepositoryManager>();

		public static IMvcBuilder AddCustomCSVFormatter(this IMvcBuilder builder) =>
			builder.AddMvcOptions(config => config.OutputFormatters.Add(new CsvOutputFormatter()));

		public static void AddCustomerMediaTypes(this IServiceCollection services)
		{
			services.Configure<MvcOptions>(config =>
			{
				var newtonsoftJsonOutputFormatter = config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();

				if(newtonsoftJsonOutputFormatter != null)
				{
					newtonsoftJsonOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+json");
				}

				var xmlOutputFormatter = config.OutputFormatters.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

				if(xmlOutputFormatter != null)
				{
					xmlOutputFormatter.SupportedMediaTypes.Add("application/vnd.codemaze.hateoas+xml");
				}
			});
		}

        public class ValidateMediaTypeAttribute : IActionFilter
        {
            public void OnActionExecuted(ActionExecutedContext context)
            {
            }

            public void OnActionExecuting(ActionExecutingContext context)
            {
                var acceptHeaderPresent = context.HttpContext.Request.Headers.ContainsKey("Accept");

				if (!acceptHeaderPresent)
				{
					context.Result = new BadRequestObjectResult($"Accept header is missing");
					return;
				}

				var mediaType = context.HttpContext.Request.Headers["Accept"].FirstOrDefault();

                if (!MediaTypeHeaderValue.TryParse(mediaType, out MediaTypeHeaderValue outMediaType))
				{
					context.Result = new BadRequestObjectResult($"Media type not present. Please add Accept header with the required media type.");
					return;
				}

				context.HttpContext.Items.Add("AcceptHeaderMediaType", outMediaType);
            }
        }
    }

	
}
