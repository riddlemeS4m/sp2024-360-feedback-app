using Capstone_360s.Interfaces.IService;
using CsvHelper.Configuration;

namespace Capstone_360s.Interfaces.IOrganization
{
    public interface IOrganizationServices<TSurvey, TInversion, TDocumentContent> : IOrganizationServicesWrapper
        where TSurvey : class
        where TInversion : class
        where TDocumentContent : class
    {
        IWritePdf<TDocumentContent, TInversion> PdfService { get; }
        IAccessCsvFile<TSurvey> CsvService { get; }
        IMapFeedback<TInversion> DataMap { get; }
        ClassMap<TSurvey> CsvMap { get; }
    }
}