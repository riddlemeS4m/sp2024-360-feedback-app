using Capstone_360s.Data.Migrations.FeedbackDb;
using Capstone_360s.Interfaces.IOrganization;
using Capstone_360s.Models.Generics;
using Capstone_360s.Models.Organizations.Capstone;
using Capstone_360s.Models.Organizations.GBA;

namespace Capstone_360s.Services.Configuration.Organizations
{
    public class OrganizationServiceFactory : IOrganizationServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public static readonly Dictionary<string, Type> _organizationServiceTypes = new Dictionary<string, Type>
        {
            { "Capstone", typeof(CapstoneOrganizationServices) },
            { "Gba", typeof(GbaOrganizationServices) }
        };

        public static readonly Dictionary<string, Type> _surveyTypes = new()
        {
            { "Capstone", typeof(Qualtrics) },
            { "Gba", typeof(GbaSurvey) }
        };

        public static readonly Dictionary<string, Type> _inversionTypes = new()
        {
            { "Capstone", typeof(InvertedQualtrics) },
            { "Gba", typeof(GbaInvertedSurvey) }
        };

        public static readonly Dictionary<string, Type> _documentTypes = new()
        {
            { "Capstone", typeof(DocumentToPrint) },
            { "Gba", typeof(GbaDocument) }
        };

        public OrganizationServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IOrganizationServicesWrapper GetServices(string orgType)
        {
            if (!_organizationServiceTypes.TryGetValue(orgType, out var serviceType))
            {
                throw new ArgumentException($"No services registered for organization type '{orgType}'");
            }

            return (IOrganizationServicesWrapper)_serviceProvider.GetRequiredService(serviceType);
        }
    }

}