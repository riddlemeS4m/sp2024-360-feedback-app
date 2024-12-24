using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Interfaces.IService
{
    public interface IManageFeedback
    {
        public Task CreateTimeframe(Timeframe timeframe, List<string> projectNames);
        public Task CreateProjectRounds(Project project, List<DateTime> roundStartDates, List<DateTime> roundEndDates);
        public Task CreateProjectRoundsForOneProject(Project project);
    }
}