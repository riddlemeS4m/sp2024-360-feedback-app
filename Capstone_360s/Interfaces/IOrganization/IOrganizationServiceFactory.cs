using Capstone_360s.Models.Generics;

namespace Capstone_360s.Interfaces.IOrganization
{
    public interface IOrganizationServiceFactory
    {
         /// <summary>
    /// Gets the service set for the specified organization.
    /// </summary>
        /// <param name="orgType">The organization type (e.g., "Capstone", "Gba").</param>
        /// <returns>The organization service set.</returns>
        IOrganizationServicesWrapper GetServices(string orgType);
    }
}
