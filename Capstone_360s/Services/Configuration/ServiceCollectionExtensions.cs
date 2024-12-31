using Capstone_360s.Data.Contexts;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Interfaces;
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
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using SendGrid;

using System.Security.Claims;

namespace Capstone_360s.Services.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services)
        {
            services.AddHttpClient("local", client => {
                client.BaseAddress = new Uri("https://localhost:7068/");
            });
            services.AddHttpContextAccessor();

            services.AddFeedbackDbServices();

            services.AddMicrosoftAuth();
            services.AddRoles();

            services.AddGoogleDrive();
            services.AddSendGrid();

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

        public static IServiceCollection AddMicrosoftAuth(this IServiceCollection services)
        {
            services.AddDistributedMemoryCache();

            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfigureEnvironment>();

                services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                // .AddCookie() // Ensure cookie-based persistence
                .AddMicrosoftIdentityWebApp(options =>
                {
                    options.ClientId = config.MicrosoftClientId;
                    options.ClientSecret = config.MicrosoftClientSecret;
                    options.TenantId = config.MicrosoftTenantId;
                    options.Instance = config.MicrosoftInstance;
                    options.Domain = config.MicrosoftDomain;
                    options.CallbackPath = config.MicrosoftCallbackPath;
                    
                    // Ensure tokens are saved after OIDC flow
                    options.SaveTokens = true;
                    
                    // Scopes (including offline_access for refresh tokens)
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("offline_access");

                    // Token validation (customize as needed)
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "roles",
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        SaveSigninToken = true
                    };

                    // options.NonceCookie.SameSite = SameSiteMode.None;
                    // options.NonceCookie.SecurePolicy = CookieSecurePolicy.Always;

                    // Event handlers to ensure persistent OIDC session
                    options.Events = new OpenIdConnectEvents
                    {
                        OnTicketReceived = async context =>
                        {
                            var principal = context.Principal;
                            var claimsIdentity = (ClaimsIdentity)principal.Identity;

                            // Optional: Add additional claims to track OIDC authentication
                            claimsIdentity.AddClaim(new Claim("oidc_authenticated", "true"));

                            // Sign in with cookies to persist OIDC session
                            await context.HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity),
                                context.Properties);
                        },
                        OnRedirectToIdentityProvider = context =>
                        {
                            context.HttpContext.RequestServices
                                .GetService<ILogger<Program>>()?
                                .LogInformation("Redirecting to Microsoft OIDC...");

                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            context.HttpContext.RequestServices
                                .GetService<ILogger<Program>>()?
                                .LogInformation("OIDC Token validated.");
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            context.HttpContext.RequestServices
                                .GetService<ILogger<Program>>()?
                                .LogError("Authentication failed: {0}", context.Exception.Message);
                            return Task.CompletedTask;
                        }
                    };
                })
                .EnableTokenAcquisitionToCallDownstreamApi(new List<string> { config.MicrosoftTokenAcquisitionScopes })
                .AddMicrosoftGraph(options =>
                {
                    options.BaseUrl = config.MicrosoftGraphBaseUrl;
                    options.Scopes = config.MicrosoftGraphScopes;
                })
                .AddDistributedTokenCaches(); // Use Redis/DistributedCache in production
            }


            // Register Microsoft Graph service
            services.AddScoped<IMicrosoftGraph, MicrosoftGraphService>(sp =>
            {
                var tokenAcquisition = sp.GetRequiredService<ITokenAcquisition>();
                var graphClient = sp.GetRequiredService<GraphServiceClient>();
                var httpContext = sp.GetRequiredService<IHttpContextAccessor>();
                var logger = sp.GetRequiredService<ILogger<MicrosoftGraphService>>();
                
                return new MicrosoftGraphService(tokenAcquisition, graphClient, httpContext, logger);
            });


            return services;
        }


        public static IServiceCollection AddRoles(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfigureEnvironment>();

                var sysAdmin = config.SystemAdministrator;
                var programManager = config.ProgramManager;
                var instructor = config.Instructor;
                var teamLead = config.TeamLead;
                var member = config.Member;

                services.AddAuthorization(options =>
                {
                    options.AddPolicy(RoleManagerService.SystemAdministratorOnlyPolicy, policy => policy.RequireRole(sysAdmin));
                    options.AddPolicy(RoleManagerService.ProgramManagerOnlyPolicy, policy => policy.RequireRole(programManager));
                    options.AddPolicy(RoleManagerService.InstructorOnlyPolicy, policy => policy.RequireRole(instructor));
                    options.AddPolicy(RoleManagerService.TeamLeadOnlyPolicy, policy => policy.RequireRole(teamLead));
                    options.AddPolicy(RoleManagerService.MemberOnlyPolicy, policy => policy.RequireRole(member));
                })
                .Configure<AuthorizationOptions>(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                });
            }

            services.AddScoped<IRoleManager, RoleManagerService>(sp =>
            {
                var config = sp.GetRequiredService<IConfigureEnvironment>();
                var graph = sp.GetRequiredService<IMicrosoftGraph>();
                var factory = sp.GetRequiredService<IFeedbackDbServiceFactory>();
                var logger = sp.GetRequiredService<ILogger<RoleManagerService>>();
                return new RoleManagerService(config, graph, factory, logger);
            });

            services.AddScoped<IClaimsTransformation, RoleClaimsTransformation>(sp => {
                var logger = sp.GetRequiredService<ILogger<RoleClaimsTransformation>>();
                var manager = sp.GetRequiredService<IRoleManager>();

                return new RoleClaimsTransformation(manager, logger);
            });

            return services;
        }

        public static IServiceCollection AddFeedbackDbServices(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfigureEnvironment>();

                services.AddDbContext<IFeedbackDbContext, FeedbackMySqlDbContext>(options =>
                    options.UseMySql(config.FeedbackDbConnection, ServerVersion.AutoDetect(config.FeedbackDbConnection),
                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));
            }

            services.AddScoped<IFeedbackDbServiceFactory, FeedbackDbServiceFactory>();

            return services;
        }

        public static IServiceCollection AddGoogleDrive(this IServiceCollection services)
        {
            services.AddScoped(sp =>
            {
                var config = sp.GetRequiredService<IConfigureEnvironment>();

                var jsonCredentials = config.GoogleCredentials;
                var applicationName = "360s Feedback Portal ASP.Net Core MVC";

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

        public static IServiceCollection AddSendGrid(this IServiceCollection services)
        {
            services.AddScoped(sp => {
                var config = sp.GetRequiredService<IConfigureEnvironment>();
                return new SendGridClient(config.SendGridKey);            
            });
            
            return services;
        }

        public static IServiceCollection AddControllerManagers(this IServiceCollection services)
        {
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var config = scope.ServiceProvider.GetRequiredService<IConfigureEnvironment>();

                services.AddTransient<IManageFeedback, ManageFeedbackService>(sp => {
                    var factory = sp.GetRequiredService<IFeedbackDbServiceFactory>();
                    var drive = sp.GetRequiredService<IGoogleDrive>();
                    var manager = sp.GetRequiredService<IRoleManager>();
                    var logger = sp.GetRequiredService<ILogger<ManageFeedbackService>>();
                    
                    return new ManageFeedbackService(factory, drive, manager, config, logger);
                });
            }

            return services;
        }
    }
}