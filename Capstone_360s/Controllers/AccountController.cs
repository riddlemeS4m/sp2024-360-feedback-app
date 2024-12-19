using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Capstone_360s.Services.Identity;
using Capstone_360s.Services.FeedbackDb;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Graph;
using NuGet.Protocol;

namespace Capstone_360s.Controllers
{
    public class AccountController : Controller
    {
        private readonly RoleManagerService _roleManager;
        private readonly MicrosoftGraphService _microsoftGraphService;
        private readonly FeedbackDbServiceFactory _serviceFactory;
        private readonly ILogger<AccountController> _logger;
        public AccountController(RoleManagerService roleManager,
            MicrosoftGraphService microsoftGraphService,
            FeedbackDbServiceFactory serviceFactory,
            ILogger<AccountController> logger)
        {
            _roleManager = roleManager;
            _microsoftGraphService = microsoftGraphService;
            _serviceFactory = serviceFactory;
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
            // attempt authenticating with microsoft
            _logger.LogWarning("About to authenticate...");
            var authResult = await HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

            var claimsIdentity = new ClaimsIdentity(OpenIdConnectDefaults.AuthenticationScheme);

            foreach (var claim in authResult.Principal.Claims)
            {
                claimsIdentity.AddClaim(claim);
            }

            // assign an application role
            var idClaim = claimsIdentity.Claims.Where(x => x.Type == "uid").FirstOrDefault();
            var emailClaim = claimsIdentity.Claims.Where(x => x.Type == ClaimTypes.Email).FirstOrDefault();
            var roles = new List<string>();

            if (idClaim == null)
            {
                throw new InvalidOperationException("User was unable to be authenticated.");
            }

            var result = await _roleManager.GetRoles(Guid.Parse(idClaim.Value));

            if (result == null)
            {
                _logger.LogInformation("User has no specified role.");
                roles.Add(RoleManagerService.MemberOnlyPolicy);
            }
            else
            {
                roles = result.ToList();
            }

            foreach (var role in roles)
            {
                _logger.LogInformation("User is in role: {0}", role);
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // identify which 'User' the authenticated user is
            try
            {
                var localUserClaim = await IdentifyLocalUser();
                if (localUserClaim != null)
                {
                    claimsIdentity.AddClaim(localUserClaim);
                }
                else
                {
                    _logger.LogError("No local identity was found for the user.");
                    return BadRequest("No local identity was found.");
                }
            }
            catch (ServiceException ex)
            {
                _logger.LogWarning("ServiceException encountered, prompting user to reauthenticate: {0}", ex.Message);
                return Challenge(new AuthenticationProperties { RedirectUri = returnUrl }, OpenIdConnectDefaults.AuthenticationScheme);
            }

            //await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            _logger.LogWarning("About to go back to the frontend...");
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

                var localUser = await _serviceFactory.UserService.GetUserByEmail(email);

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

                    await _serviceFactory.UserService.AddAsync(user);

                    return new Claim("LocalUser", user.Id.ToString());
                }
                else
                {
                    if (localUser.MicrosoftId == null)
                    {
                        localUser.MicrosoftId = Guid.Parse(microsoftUser.Id);
                        await _serviceFactory.UserService.UpdateAsync(localUser);
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

        public async Task<IActionResult> AccessDenied()
        {
            return View();
        }
    }
}
