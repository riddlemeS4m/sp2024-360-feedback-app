using Capstone_360s.Data.Contexts;
using Capstone_360s.Interfaces.IOrganization;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.Organizations.Capstone;
using Capstone_360s.Services.Configuration;
using Capstone_360s.Services.CSV;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.GoogleDrive;
using Capstone_360s.Services.Identity;
using Capstone_360s.Services.Maps;
using Capstone_360s.Services.PDF;
using CsvHelper.Configuration;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Web;
using SendGrid;
using Capstone_360s.Data.Constants;
using Microsoft.Graph;
using Microsoft.AspNetCore.Authentication.Cookies;
using Capstone_360s.Utilities.Maps;
using Capstone_360s.Services.Configuration.Organizations;
using EllipticCurve;
using Capstone_360s.Models.Generics;
using System.Reflection;

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

            return services;
        }

        public static IServiceCollection RegisterCapstoneServices(this IServiceCollection services, string organizationName)
        {
            services.AddTransient<ClassMap<Qualtrics>, CapstoneMapCsvToQualtrics>();

            services.AddTransient<IMapFeedback<InvertedQualtrics>, CapstoneMapToInvertedQualtrics>(sp =>
            {
                var feedbackFactory = sp.GetRequiredService<FeedbackDbServiceFactory>();
                var logger = sp.GetRequiredService<ILogger<CapstoneMapToInvertedQualtrics>>();
                return new CapstoneMapToInvertedQualtrics(feedbackFactory, logger);
            });

            services.AddTransient<IAccessCsvFile<Qualtrics>, CapstoneCsvService>(sp =>
            {
                var classMap = sp.GetRequiredService<ClassMap<Qualtrics>>();
                var logger = sp.GetRequiredService<ILogger<CapstoneCsvService>>();
                return new CapstoneCsvService(classMap, logger);
            });

            services.AddTransient<IWritePdf<DocumentToPrint, InvertedQualtrics>, CapstonePdfService>(sp =>
            {
                var invertedMap = sp.GetRequiredService<IMapFeedback<InvertedQualtrics>>();
                var feedbackFactory = sp.GetRequiredService<FeedbackDbServiceFactory>();
                var logger = sp.GetRequiredService<ILogger<CapstonePdfService>>();
                return new CapstonePdfService(feedbackFactory, invertedMap, logger);
            });

            services.AddTransient<CapstoneOrganizationServices>(sp =>
            {
                var name = organizationName;
                var classMap = sp.GetRequiredService<ClassMap<Qualtrics>>();
                var invertedMap = sp.GetRequiredService<IMapFeedback<InvertedQualtrics>>();
                var csvService = sp.GetRequiredService<IAccessCsvFile<Qualtrics>>();
                var pdfService = sp.GetRequiredService<IWritePdf<DocumentToPrint, InvertedQualtrics>>();
                return new CapstoneOrganizationServices(classMap, invertedMap, csvService, pdfService);
            });

            services.AddTransient<IOrganizationServices<Qualtrics, InvertedQualtrics, DocumentToPrint>, CapstoneOrganizationServices>(sp => {
                var capstoneServices = sp.GetRequiredService<CapstoneOrganizationServices>();
                return capstoneServices;
            });

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

            services.AddScoped<MicrosoftGraphService>(serviceProvider =>
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

            services.AddScoped<FeedbackDbServiceFactory>();

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
    }
}