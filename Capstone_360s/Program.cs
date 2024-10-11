using Capstone_360s.Data.Contexts;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.GoogleDrive;
using Capstone_360s.Utilities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace Capstone_360s
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Get environment variables.
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Edit configuration.
            builder.Configuration.AddUserSecrets<Program>();

            var googleCredentialSection = builder.Configuration.GetSection("GoogleCredential");

            string connectionStringPrefix;

            if (string.IsNullOrEmpty(environment) || environment == Environments.Development)
            {
                connectionStringPrefix = "ConnectionStrings:Development:";
            }
            else
            {
                connectionStringPrefix = "ConnectionStrings:Production:";
            }

            var feedbackConnectionString = builder.Configuration[$"{connectionStringPrefix}Feedback"] ?? throw new InvalidOperationException($"Connection string '{connectionStringPrefix}Feedback' not found.");


            // Add services to the container.
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });
            
            builder.Services.AddSingleton(_ => {
                string applicationName = "Capstone 360s App ASP.Net Core MVC";
                string json = GoogleDriveUtility.SerializeCredentials(googleCredentialSection);

                GoogleCredential credential;
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(new[]
                    {
                        DriveService.Scope.DriveFile
                    });
                }

                return new DriveService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName
                });
            });

            //when updating the db, run the following commands in the package manager console... (otherwise, you'll need to run the equivalent dotnet commands in the terminal)
            //1. create the entity framework migration
            //add-migration <migrationname> -context feedbackMySqlDbContext -outputdir Data\Migrations\FeedbackDb
            //dotnet equivalent: dotnet ef migrations add <migrationname> -c feedbackMySqlDbContext -o Data\Migrations\FeedbackDb
            //2. update the database 
            //update-database -context feedbackMySqlDbContext
            //dotnet equivalent: dotnet ef database update -c feedbackMySqlDbContext
            //CI/CD pipeline updates prod databases if tests pass

            builder.Services.AddMemoryCache();

            builder.Services.AddDbContext<IFeedbackDbContext, FeedbackMySqlDbContext>(options =>
                    options.UseMySql(feedbackConnectionString, ServerVersion.AutoDetect(feedbackConnectionString)));

            builder.Services.AddScoped(ServiceFactory.CreateService<FeedbackService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<MetricService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<MetricResponseService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<QuestionService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<QuestionResponseService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<OrganizationService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<ProjectService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<TimeframeService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<UserService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<TeamService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<RoundService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<ProjectRoundService>);

            builder.Services.AddScoped<IGoogleDrive>(serviceProvider =>
            {
                var driveService = serviceProvider.GetRequiredService<DriveService>();
                var cacheService = serviceProvider.GetRequiredService<IMemoryCache>();
                var logger = serviceProvider.GetRequiredService<ILogger<GoogleDriveService>>();
                return new GoogleDriveService(driveService, cacheService, logger);
            });

            builder.Services.AddControllersWithViews();

            var serviceProvider = builder.Services.BuildServiceProvider();

            // Attempt to resolve the GoogleDrive service for validation
            using (var scope = serviceProvider.CreateScope())
            {
                var googleDriveService = scope.ServiceProvider.GetService<IGoogleDrive>();
                if (googleDriveService == null)
                {
                    throw new InvalidOperationException($"Failed to resolve {nameof(IGoogleDrive)}.");
                }
                else
                {
                    // Log or debug to ensure it resolved properly
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("IGoogleDrive resolved successfully.");
                }
            }


            var app = builder.Build();

            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            } 
            else
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }

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
