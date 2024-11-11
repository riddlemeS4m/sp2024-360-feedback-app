using Capstone_360s.Data.Contexts;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Services.CSV;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.GoogleDrive;
using Capstone_360s.Services.Maps;
using Capstone_360s.Services.PDF;
using Capstone_360s.Utilities;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SendGrid;
using System.Security.Claims;

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

            var googleCredentialSection = builder.Configuration.GetSection("GoogleCredential") ?? throw new InvalidOperationException($"'GoogleCredential' section not found.");
            var sendGridKey = builder.Configuration["SendGrid"] ?? throw new InvalidOperationException($"'SendGrid' key was not found.");

            string connectionStringPrefix;

            if (environment == Environments.Development || string.IsNullOrEmpty(environment))
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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(
            //    options =>
            //{
            //    options.LoginPath = "/Account/Login";
            //    options.LogoutPath = "/Account/Logout";
            //    options.AccessDeniedPath = "/Account/AccessDenied";
            //    options.ExpireTimeSpan = TimeSpan.FromHours(1);
            //    options.SlidingExpiration = true;
            //    options.Cookie.HttpOnly = true;
            //    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Use Always if on HTTPS
            //    options.Cookie.SameSite = SameSiteMode.None; // Adjust based on your environment
            //    options.Cookie.Path = "/";
            //    options.Cookie.MaxAge = TimeSpan.FromHours(1);
            //}
            )
            .AddMicrosoftAccount(microsoftOptions =>
            {
                microsoftOptions.ClientId = builder.Configuration["Authentication:Microsoft:ClientId"] ?? throw new InvalidOperationException($"Microsoft ClientId not found.");
                microsoftOptions.ClientSecret = builder.Configuration["Authentication:Microsoft:ClientSecret"] ?? throw new InvalidOperationException($"Microsoft ClientSecret not found.");
                microsoftOptions.AuthorizationEndpoint = builder.Configuration["Authentication:Microsoft:AuthorizationEndpoint"] ?? throw new InvalidOperationException($"Microsoft AuthorizationEndpoint not found.");
                microsoftOptions.TokenEndpoint = builder.Configuration["Authentication:Microsoft:TokenEndpoint"] ?? throw new InvalidOperationException($"Microsoft TokenEndpoint not found.");
                //microsoftOptions.CallbackPath = "/signin-microsoft";
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
                options.AddPolicy("SponsorOnly", policy => policy.RequireRole("Sponsor"));
                options.AddPolicy("ProjectManagerOnly", policy => policy.RequireRole("ProjectManager"));
                options.AddPolicy("MemberOnly", policy => policy.RequireRole("Member"));

                //options.DefaultPolicy = new AuthorizationPolicyBuilder()
                //    .RequireAuthenticatedUser()
                //    .Build();

                // Set the role claim type explicitly
                //options.DefaultPolicy = new AuthorizationPolicyBuilder()
                //    .RequireClaim(ClaimTypes.Role)
                //    .Build();
            }).Configure<AuthorizationOptions>(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
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
                    options.UseMySql(feedbackConnectionString, ServerVersion.AutoDetect(feedbackConnectionString), 
                        mySqlOptions => mySqlOptions.EnableRetryOnFailure()));

            // requires the entire database schema to be changed, blegh
            //builder.Services.AddIdentity<User, IdentityRole<Guid>>()
            //    .AddEntityFrameworkStores<FeedbackMySqlDbContext>()
            //    .AddDefaultTokenProviders();

            //builder.Services.Configure<IdentityOptions>(options =>
            //{
            //    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;
            //    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
            //    options.ClaimsIdentity.EmailClaimType = ClaimTypes.Email;
            //});

            // look into service factory/wheel so I don't have to have all this in program.cs
            builder.Services.AddScoped(ServiceFactory.CreateService<FeedbackService>);
            builder.Services.AddScoped(ServiceFactory.CreateService<FeedbackPdfService>);
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

            builder.Services.AddScoped<IGoogleDrive, GoogleDriveService>(serviceProvider =>
            {
                var driveService = serviceProvider.GetRequiredService<DriveService>();
                var cacheService = serviceProvider.GetRequiredService<IMemoryCache>();
                var logger = serviceProvider.GetRequiredService<ILogger<GoogleDriveService>>();
                return new GoogleDriveService(driveService, cacheService, logger);
            });

            builder.Services.AddSingleton(_ =>
            {
                return new SendGridClient(sendGridKey);
            });

            // remember, have an organization context that has method/class types associated with it, and register an entire organization context
            // organization context can inherit an interface with all these types
            builder.Services.AddScoped<CapstoneMapCsvToQualtrics>();

            builder.Services.AddScoped<CapstoneMapToInvertedQualtrics>(serviceProvider =>
            {
                var organizationService = serviceProvider.GetRequiredService<OrganizationService>();
                var timeframeService = serviceProvider.GetRequiredService<TimeframeService>();
                var projectService = serviceProvider.GetRequiredService<ProjectService>();
                var userService = serviceProvider.GetRequiredService<UserService>();
                var metricResponseService = serviceProvider.GetRequiredService<MetricResponseService>();
                var questionResponseService = serviceProvider.GetRequiredService<QuestionResponseService>();
                var logger = serviceProvider.GetRequiredService<ILogger<CapstoneMapToInvertedQualtrics>>();
                return new CapstoneMapToInvertedQualtrics(organizationService, timeframeService, projectService, userService, metricResponseService, questionResponseService, logger);
            });

            builder.Services.AddScoped<CapstoneCsvService>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<CapstoneCsvService>>();
                var map = serviceProvider.GetRequiredService<CapstoneMapCsvToQualtrics>();
                return new CapstoneCsvService(map, logger);
            });

            builder.Services.AddScoped<CapstonePdfService>(serviceProvider =>
            {
                var roundService = serviceProvider.GetRequiredService<RoundService>();
                var userService = serviceProvider.GetRequiredService<UserService>();
                var feedbackService = serviceProvider.GetRequiredService<FeedbackService>();
                var invertQualtricsService = serviceProvider.GetRequiredService<CapstoneMapToInvertedQualtrics>();
                var logger = serviceProvider.GetRequiredService<ILogger<CapstonePdfService>>();
                return new CapstonePdfService(roundService, feedbackService, userService, invertQualtricsService, logger);
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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Use(async (context, next) =>
            {
                var user = context.User;
                if (user.Identity.IsAuthenticated && user.HasClaim(c => c.Type == "Role"))
                {
                    var claimsIdentity = user.Identity as ClaimsIdentity;
                    var roleClaim = claimsIdentity?.FindFirst("Role");

                    if (roleClaim != null)
                    {
                        claimsIdentity.RemoveClaim(roleClaim);
                        claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                    }
                }

                await next();
            });

            app.Run();
        }
    }
}
