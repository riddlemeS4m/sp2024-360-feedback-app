using Capstone_360s.Data.Constants;
using Capstone_360s.Interfaces.IOrganization;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.Organizations.Capstone;
using Capstone_360s.Models.Generics;
using CsvHelper.Configuration;

namespace Capstone_360s.Services.Configuration.Organizations
{
    public class CapstoneOrganizationServices : IOrganizationServices<Qualtrics, InvertedQualtrics, DocumentToPrint>
    {
        public string Type => "Capstone";
        public IWritePdf<DocumentToPrint, InvertedQualtrics> PdfService { get; }
        public IAccessCsvFile<Qualtrics> CsvService { get; }
        public IMapFeedback<InvertedQualtrics> DataMap { get; }
        public ClassMap<Qualtrics> CsvMap { get; }
        public GenericConstants Constants { get; }

        public CapstoneOrganizationServices(
            ClassMap<Qualtrics> csvMap,
            IMapFeedback<InvertedQualtrics> dataMap,
            IAccessCsvFile<Qualtrics> csvService,          
            IWritePdf<DocumentToPrint, InvertedQualtrics> pdfService)
        {
            PdfService = pdfService;
            CsvService = csvService;
            DataMap = dataMap;
            CsvMap = csvMap;
            Constants = new Capstone();
        }
    }
}
