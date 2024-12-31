using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;

namespace Capstone_360s.Interfaces.IService
{
    public interface IManageFeedback
    {
        public Task CreateTimeframe(Timeframe timeframe, List<string> projectNames);
        public Task CreateProjectRounds(Project project, List<DateTime> roundStartDates, List<DateTime> roundEndDates);
        public Task CreateProjectRoundsForOneProject(Project project);
        public Task CreateProject(Project newProject, Timeframe timeframe, string orgId, int timeframeId, 
            string POCEmail, string ManagerEmail, string NewTeamMembers);
        public Task<Project> CreateProjectSilently(Timeframe timeframe, string orgId, int timeframeId, string projectId, string userEmail, string projectName = "");
        public Task<Project> EditProject(Project newProject, Project oldProject, string orgId, int timeframeId, 
            string POCEmail, string ManagerEmail, List<string> NewTeamMembers);
        public Task<ProjectEditVM> CreateProjectCreateViewModel(string orgId, int timeframeId);
        public Task<ProjectEditVM> CreateProjectEditViewModel(string orgId, int timeframeId, string projectId);
        public Task<AssignPOCVM> CreateAssignPOCViewModel(string orgId, string projectId);
        public Task<AssignPOCVM> CreateAssignManagerViewModel(string orgId, string projectId);
        public Task UploadBlackboardRoster(IFormFile csv, string orgId, int timeframeId);
        public Task<Guid> AssignUserPermissions(string orgId, string email, string role = "");
        public Task AssignPOCToProject(string orgId, string projectId, string email);
        public Task AssignManagerToProject(string orgId, string projectId, string email);
    }
}