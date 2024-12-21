using Capstone_360s.Data.Contexts;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Interfaces;
using Capstone_360s.Services;
using Capstone_360s.Services.Configuration;
using Capstone_360s.Services.GoogleDrive;
using Capstone_360s.Services.Identity;
using Capstone_360s.Services.Organizations;
using Capstone_360s.Services.Controllers;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using SendGrid;

namespace Capstone_360s.Utilities
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, CustomConfigurationService customConfiguration)
        {
            services.AddMicrosoftAuth(customConfiguration);
            services.AddRoles(customConfiguration);
            services.AddFeedbackDbServices(customConfiguration);
            services.AddGoogleDrive(customConfiguration);
            services.AddSendGrid(customConfiguration);
            services.AddControllerManagers();

            services.RegisterOrganizations();

            return services;
        }

        public static IServiceCollection RegisterOrganizations(this IServiceCollection services)
        {
            services.AddTransient<CapstoneService>(sp => {
               var logger = sp.GetRequiredService<ILogger<CapstoneService>>();
               var drive = sp.GetRequiredService<IGoogleDrive>();
               var factory = sp.GetRequiredService<IFeedbackDbServiceFactory>();

               return new CapstoneService(factory, drive, logger);
            });

            // services.RegisterGbaServices(services, customConfiguration.GbaOrg);

            return services;
        }

        public static IServiceCollection AddMicrosoftAuth(this IServiceCollection services, CustomConfigurationService config)
        {
            services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.ClientId = config.MicrosoftClientId;
                    options.ClientSecret = config.MicrosoftClientSecret;
                    options.TenantId = config.MicrosoftTenantId;
                    options.Instance = config.MicrosoftInstance;
                    options.Domain = config.MicrosoftDomain;
                    options.CallbackPath = config.MicrosoftCallbackPath;
                })
                .EnableTokenAcquisitionToCallDownstreamApi(new List<string> { config.MicrosoftTokenAcquisitionScopes })
                .AddMicrosoftGraph(options =>
                {
                    options.BaseUrl = config.MicrosoftGraphBaseUrl;
                    options.Scopes = config.MicrosoftGraphScopes;
                })
                .AddInMemoryTokenCaches();

            services.AddScoped<IMicrosoftGraph, MicrosoftGraphService>(serviceProvider =>
            {
                var graphClient = serviceProvider.GetRequiredService<GraphServiceClient>();
                return new MicrosoftGraphService(graphClient);
            });

            return services;
        }

        public static IServiceCollection AddRoles(this IServiceCollection services, CustomConfigurationService config)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(RoleManagerService.AdminOnlyPolicy, policy => policy.RequireRole(config.Administrator));
                options.AddPolicy(RoleManagerService.SponsorOnlyPolicy, policy => policy.RequireRole(config.Sponsor));
                options.AddPolicy(RoleManagerService.LeadOnlyPolicy, policy => policy.RequireRole(config.Lead));
                options.AddPolicy(RoleManagerService.MemberOnlyPolicy, policy => policy.RequireRole(config.Member));
            })
            .Configure<AuthorizationOptions>(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>();

            services.AddTransient(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<RoleManagerService>>();
                return new RoleManagerService(config.RolesDbConnection, logger);
            });

            return services;
        }

        public static IServiceCollection AddFeedbackDbServices(this IServiceCollection services, CustomConfigurationService config)
        {
            services.AddDbContext<IFeedbackDbContext, FeedbackMySqlDbContext>(options =>
                options.UseMySql(config.FeedbackDbConnection, ServerVersion.AutoDetect(config.FeedbackDbConnection),
                    mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

            services.AddScoped<IFeedbackDbServiceFactory, FeedbackDbServiceFactory>();

            return services;
        }

        public static IServiceCollection AddGoogleDrive(this IServiceCollection services, CustomConfigurationService config)
        {
            services.AddSingleton(provider =>
            {
                var jsonCredentials = config.GoogleCredentials;
                var applicationName = "Capstone 360s App ASP.Net Core MVC";

                GoogleCredential credential;
                using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonCredentials)))
                {
                    credential = GoogleCredential.FromStream(stream).CreateScoped(DriveService.Scope.DriveFile);
                }

                return new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = applicationName
                });
            });

            services.AddMemoryCache();

            services.AddScoped<IGoogleDrive, GoogleDriveService>(provider =>
            {
                var driveService = provider.GetRequiredService<DriveService>();
                var cacheService = provider.GetRequiredService<IMemoryCache>();
                var logger = provider.GetRequiredService<ILogger<GoogleDriveService>>();
                return new GoogleDriveService(driveService, cacheService, logger);
            });

            return services;
        }

        public static IServiceCollection AddSendGrid(this IServiceCollection services, CustomConfigurationService config)
        {
            services.AddSingleton(_ => new SendGridClient(config.SendGridKey));
            return services;
        }

        public static IServiceCollection AddControllerManagers(this IServiceCollection services)
        {
            services.AddTransient<IManageFeedback, ManageFeedbackService>(sp => {
                var factory = sp.GetRequiredService<IFeedbackDbServiceFactory>();
                var drive = sp.GetRequiredService<IGoogleDrive>();
                var logger = sp.GetRequiredService<ILogger<ManageFeedbackService>>();
                
                return new ManageFeedbackService(factory, drive, logger);
            });

            return services;
        }
    }
}