using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Interfaces.IService
{
    public interface IAccessCsvFile<T> where T : class
    {
        public delegate bool FilterDateFunc(string datefield, DateTime filterDate);
        public IEnumerable<T> ReadSurveyResponses(IFormFile file, IEnumerable<string> expectedHeaders);
        public IEnumerable<T> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate, FilterDateFunc filterDateFunc);
    }
}
