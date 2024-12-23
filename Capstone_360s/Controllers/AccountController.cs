using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Interfaces;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Graph;

namespace Capstone_360s.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRoleManager _roleManager;
        private readonly IMicrosoftGraph _microsoftGraphService;
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly ILogger<AccountController> _logger;
        public AccountController(IRoleManager roleManager,
            IMicrosoftGraph microsoftGraphService,
            IFeedbackDbServiceFactory dbServiceFactory,
            ILogger<AccountController> logger)
        {
            _roleManager = roleManager;
            _microsoftGraphService = microsoftGraphService;
            _dbServiceFactory = dbServiceFactory;
            _logger = logger;
        }

        public async Task Login(string returnUrl = null)
        {
            _logger.LogWarning("Challenge received...");
            await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("MicrosoftResponse", "Account", new { returnUrl })
                });
        }

        public async Task<IActionResult> MicrosoftResponse(string returnUrl = null)
        {
            _logger.LogWarning("About to authenticate...");

            // Authenticate the user using the OpenIdConnect scheme
            var authResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

            if (!authResult.Succeeded || authResult.Principal == null)
            {
                _logger.LogError("Microsoft authentication failed.");
                return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
            }

            // Transfer claims to a new identity (Cookie Authentication Scheme)
            var claimsIdentity = new ClaimsIdentity(
                authResult.Principal.Claims, 
                CookieAuthenticationDefaults.AuthenticationScheme
            );

            // Get user ID (uid) and fetch roles
            var idClaim = claimsIdentity.FindFirst("uid");
            var roles = new List<string>();

            if (idClaim == null)
            {
                _logger.LogError("User ID claim (uid) is missing.");
                throw new InvalidOperationException("User could not be authenticated.");
            }

            var result = await _roleManager.GetRoles(Guid.Parse(idClaim.Value));
            roles = result?.ToList() ?? new List<string> { RoleManagerService.MemberOnlyPolicy };

            foreach (var role in roles)
            {
                _logger.LogInformation("Adding user to role: {0}", role);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Identify the local user (internal mapping)
            try
            {
                var localUserClaim = await IdentifyLocalUser();
                if (localUserClaim != null)
                {
                    claimsIdentity.AddClaim(localUserClaim);
                }
                else
                {
                    _logger.LogError("No local identity found for the user.");
                    return BadRequest("No local identity found.");
                }
            }
            catch (ServiceException ex)
            {
                _logger.LogWarning("ServiceException encountered: {0}. Redirecting for reauthentication.", ex.Message);
                return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
            }

            // Create a claims principal for the cookie session
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign in to Cookies
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                claimsPrincipal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTime.UtcNow.AddHours(1)
                });

            // This redirect finalizes the OIDC session - DO NOT call SignInAsync for OIDC
            _logger.LogWarning("User authenticated successfully. Redirecting...");
            return LocalRedirect(returnUrl ?? Url.Action(nameof(HomeController.Index), HomeController.Name));
        }

        private async Task<Claim> IdentifyLocalUser()
        {
            var email = User.Claims.FirstOrDefault(x => x.Type == "preferred_username")?.Value
            ?? User.Claims.FirstOrDefault(x => x.Type == "email")?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("No valid email claim found for the authenticated user.");
            }

            try
            {
                var microsoftUser = await _microsoftGraphService.GetUserIdByEmailAsync(email);

                var localUser = await _dbServiceFactory.UserService.GetUserByEmail(email);

                if (localUser.Id == Guid.Empty)
                {
                    var user = new Capstone_360s.Models.FeedbackDb.User()
                    {
                        Id = Guid.Parse(microsoftUser.Id),
                        MicrosoftId = Guid.Parse(microsoftUser.Id),
                        FirstName = microsoftUser.GivenName,
                        LastName = microsoftUser.Surname,
                        Email = email
                    };

                    await _dbServiceFactory.UserService.AddAsync(user);

                    return new Claim("LocalUser", user.Id.ToString());
                }
                else
                {
                    if (localUser.MicrosoftId == null)
                    {
                        localUser.MicrosoftId = Guid.Parse(microsoftUser.Id);
                        await _dbServiceFactory.UserService.UpdateAsync(localUser);
                    }

                    return new Claim("LocalUser", localUser.Id.ToString());
                }
            }
            catch (ServiceException ex)
            {
                _logger.LogWarning("Microsoft Graph ServiceException: {0}", ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError("An error occurred in IdentifyLocalUser: {0}", ex.Message);
                throw;
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
