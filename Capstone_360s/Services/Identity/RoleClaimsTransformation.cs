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

                        existingRoles.AddRange(roles);
                    }
                }
                else
                {
                    _logger.LogWarning($"Existing roles were found: {existingRoles.ElementAt(0)}");
                }

                // Apply role inheritance/privilege elevation
                ApplyRoleInheritance(identity, existingRoles);
            }            

            return principal;
        }

        private void ApplyRoleInheritance(ClaimsIdentity identity, List<string> roles)
        {
            // Role hierarchy map (Higher -> Lower roles)
            var roleHierarchy = new Dictionary<string, List<string>>
            {
                { "SystemAdministrator", new List<string> { "ProgramManager", "Instructor", "TeamLead" } },
                { "ProgramManager", new List<string> { "Instructor", "TeamLead" } },
                { "TeamLead", new List<string> { "TeamLead" }}
            };

            // Iterate over the user's current roles and add inherited roles
            foreach (var role in roles)
            {
                if (roleHierarchy.TryGetValue(role, out var inheritedRoles))
                {
                    foreach (var inheritedRole in inheritedRoles)
                    {
                        if (!roles.Contains(inheritedRole))  // Avoid duplication
                        {
                            identity.AddClaim(new Claim(ClaimTypes.Role, inheritedRole));
                            _logger.LogInformation($"Role '{inheritedRole}' inherited by '{role}'");
                        }
                    }
                }
            }
        }
    }
}
