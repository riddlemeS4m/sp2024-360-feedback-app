using Capstone_360s.Models.Generics;

namespace Capstone_360s.Interfaces.IOrganization
{
    public interface IOrganizationServicesWrapper
    {
        string Type { get; }
        GenericConstants Constants { get; }
    }
}
