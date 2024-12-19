using Capstone_360s.Models.Generics;

namespace Capstone_360s.Interfaces.IService
{
    public interface IAccessCsvFile<TSurvey> where TSurvey : class
    {
        public delegate bool FilterDateFunc(string datefield, DateTime filterDate);
        public IEnumerable<TSurvey> ReadSurveyResponses(IFormFile file, IEnumerable<string> expectedHeaders);
        public IEnumerable<TSurvey> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate, FilterDateFunc filterDateFunc);
    }
}
