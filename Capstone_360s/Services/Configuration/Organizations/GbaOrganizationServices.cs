using Capstone_360s.Interfaces.IOrganization;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.Generics;
using Capstone_360s.Models.Organizations.GBA;
using CsvHelper.Configuration;

namespace Capstone_360s.Services.Configuration.Organizations
{
    public class GbaOrganizationServices : IOrganizationServices<GbaSurvey, GbaInvertedSurvey, GbaDocument>
    {
        public string Type => "Gba";

        public IWritePdf<GbaDocument, GbaInvertedSurvey> PdfService { get; }

        public IAccessCsvFile<GbaSurvey> CsvService { get; }

        public IMapFeedback<GbaInvertedSurvey> DataMap { get; }

        public ClassMap<GbaSurvey> CsvMap { get; }

        public GenericConstants Constants { get; }

        public GbaOrganizationServices(IWritePdf<GbaDocument, 
            GbaInvertedSurvey> pdfService, 
            IAccessCsvFile<GbaSurvey> csvService, 
            IMapFeedback<GbaInvertedSurvey> dataMap, 
            ClassMap<GbaSurvey> csvMap, 
            GenericConstants constants)
        {
            PdfService = pdfService;
            CsvService = csvService;
            DataMap = dataMap;
            CsvMap = csvMap;
            Constants = constants;
        }
    }
}
