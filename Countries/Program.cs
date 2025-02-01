using Countries.Services;
using Countries.Services.BackJobWithHangfire;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Countries
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Enable Swagger with metadata
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Blocked Countries API",
                    Version = "v1",
                    Description = "API for managing blocked countries and validating IP addresses",
                    Contact = new OpenApiContact
                    {
                        Name = "Khadega Alaa Eldeen Abdallah",
                        Email = "khadegaalaaeldeen84@gmail.com",
                        Url = new Uri("https://github.com/KhadigaAlaa/Countries")
                    }
                });

                // Include XML comments for better documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);
            });

            // Register application services
            builder.Services.AddSingleton<BlockCountryService>();
            builder.Services.AddSingleton<IPLookupService>();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSingleton<BlockLogService>();
            builder.Services.AddScoped<ITempBlockHangfire, TempBlockHangfire>();

            // Register Hangfire (In-Memory Storage)
            builder.Services.AddHangfire(configuration =>
                configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage());

            builder.Services.AddHangfireServer();

            var app = builder.Build();

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blocked Countries API v1");
                    options.RoutePrefix = string.Empty; // Serves Swagger at root URL
                });
            }

            app.UseAuthorization();

            // Enable Hangfire Dashboard
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "My Hangfire Dashboard"
            });

            // Schedule the recurring job every 5 minutes
            RecurringJob.AddOrUpdate<BlockCountryService>(
                "RemoveExpiredTemporaryBlocks",
                s => s.RemoveExpiredTemporaryBlocks(),
                "*/5 * * * *"); // Runs every 5 minutes

            app.MapControllers();
            app.Run();
        }
    }
}
