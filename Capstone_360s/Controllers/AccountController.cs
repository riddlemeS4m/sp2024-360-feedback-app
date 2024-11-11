using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Capstone_360s.Controllers
{
    public class AccountController : Controller
    {
        private readonly ILogger<AccountController> _logger;
        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        public async Task Login(string returnUrl = null)
        {
            _logger.LogWarning("Challenge received...");
            await HttpContext.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("MicrosoftResponse", "Account", new { returnUrl })
                });
        }

        public async Task<IActionResult> MicrosoftResponse(string returnUrl = null)
        {
            _logger.LogWarning("About to authenticate...");
            var result = await HttpContext.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

            var claimsIdentity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);

            foreach (var claim in result.Principal.Claims)
            {
                claimsIdentity.AddClaim(claim);
            }

            // role assignment/check logic will go here
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));


            //return Json(claims);
            _logger.LogWarning("About to go back to the frontend...");
            return LocalRedirect(returnUrl ?? Url.Action(nameof(HomeController.Index), HomeController.Name));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(HomeController.Index), HomeController.Name);
        }

    }
}
