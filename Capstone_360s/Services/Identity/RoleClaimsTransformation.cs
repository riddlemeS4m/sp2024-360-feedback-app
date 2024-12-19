using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Capstone_360s.Services.Identity
{
    public class RoleClaimsTransformation : IClaimsTransformation
    {
        private readonly RoleManagerService _roleService;

        public RoleClaimsTransformation(RoleManagerService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                var userId = identity.FindFirst("uid")?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    var roles = await _roleService.GetRoles(Guid.Parse(userId));
                    foreach (var role in roles)
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            return principal;
        }
    }
}
