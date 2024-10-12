using Capstone_360s.Models.Roster;

namespace Capstone_360s.Interfaces.IService
{
    public interface IAccessCsvFile
    {
        public IEnumerable<Qualtrics> ReadCapstoneSurveyResponses(IFormFile file, DateTime filterDate);
    }
}
