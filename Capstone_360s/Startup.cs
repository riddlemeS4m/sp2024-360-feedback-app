
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System.Security.Claims;
using Capstone_360s.Models;
using Capstone_360s.Services.Configuration;
using Capstone_360s.Utilities;
using Capstone_360s.Services.Configuration.Organizations;
using Capstone_360s.Interfaces.IOrganization;

namespace Capstone_360s
{
    public class Startup(IConfiguration configuration)
    {
        private readonly IConfiguration _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            // Get environment variables.
            var environment = Environment.GetEnvironmentVariable(CustomConfiguration.Environment);

            // Register the CustomConfigurationService with validation
            var customConfiguration = new CustomConfigurationService(_configuration, environment);
            services.AddSingleton(customConfiguration);

            // Add logging services
            services.AddLogging();

            // Add all custom services
            services.AddCustomServices(customConfiguration);

            // Configure network settings
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                options.KnownNetworks.Clear();
                options.KnownProxies.Clear();
            });

            // Configure Organizations
            services.RegisterCapstoneServices(customConfiguration.CapstoneOrg);

            services.AddScoped<IOrganizationServiceFactory, OrganizationServiceFactory>();

            // organizationConfigurationService.PerformContextChecks(serviceProvider);

            services.AddControllersWithViews();

            services.AddRazorPages()
                .AddMicrosoftIdentityUI();
                

            services.AddServerSideBlazor()
                .AddMicrosoftIdentityConsentHandler();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();

            // Configure the HTTP request pipeline.
            if (!env.IsDevelopment())
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();      
            });

            // logging hook for roles
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
        }
    }
}
