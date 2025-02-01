
using Countries.Services.BackJobWithHangfire;
using Countries.Services;
using Hangfire;
using Hangfire.MemoryStorage;

namespace Countries
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register application services
            builder.Services.AddSingleton<BlockCountryService>();
            builder.Services.AddSingleton<IPLookupService>();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // Register Hangfire
            builder.Services.AddHangfire(configuration =>
             configuration.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
             .UseSimpleAssemblyNameTypeSerializer()
             .UseDefaultTypeSerializer()
             .UseMemoryStorage()); // Add this line to use in-memory storage

            builder.Services.AddHangfireServer();


            builder.Services.AddSingleton<BlockLogService>();
            builder.Services.AddScoped<ITempBlockHangfire, TempBlockHangfire>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                DashboardTitle = "My Hangfire Dashboard"
            });

            // Schedule the recurring job every 5 minutes
            RecurringJob.AddOrUpdate<BlockCountryService>(
              "RemoveExpiredTemporaryBlocks",
              s => s.RemoveExpiredTemporaryBlocks(),
              "*/5 * * * *" // Runs every 5 minutes
              );


            app.MapControllers();

            app.Run();
        }
    }
}
