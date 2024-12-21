using Capstone_360s.Interfaces.IService;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Capstone_360s.Services.Identity
{
    public class RoleClaimsTransformation : IClaimsTransformation
    {
        private readonly IRoleManager _roleService;
        private readonly ILogger<RoleClaimsTransformation> _logger;

        public RoleClaimsTransformation(IRoleManager roleService,
            ILogger<RoleClaimsTransformation> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is ClaimsIdentity identity && identity.IsAuthenticated)
            {
                // Check if roles already exist in the claims
                var existingRoles = identity.Claims
                    .Where(c => c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();

                if (existingRoles.Count == 0)  // If no roles exist, query the database
                {
                    _logger.LogWarning("No existing roles were found.");
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
                else
                {
                    _logger.LogWarning($"Existing roles were found: {existingRoles.ElementAt(0)}");
                }
            }
            return principal;
        }
    }
}
