using System.Text.Json;
using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    [Authorize]
    [Route("{organizationId}/[controller]/[action]")]
    public class UploadProcessController : Controller
    {
        [FromRoute]
        public string OrganizationId { get; set; }
        public const string Name = "UploadProcess";
        private readonly IManageFeedback _manager;
        private readonly IFeedbackDbServiceFactory _dbServiceFactory;
        private readonly IAuthorizationService _authService;
        private readonly ILogger<UploadProcessController> _logger;

        public UploadProcessController(
            IManageFeedback manager,
            IFeedbackDbServiceFactory serviceFactory,
            IAuthorizationService authService,
            ILogger<UploadProcessController> logger) 
        { 
            _manager = manager;
            _dbServiceFactory = serviceFactory;
            _authService = authService;
            _logger = logger;
        }

        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting upload process...");

            var organizations = await _dbServiceFactory.OrganizationService.GetAllAsync();

            _logger.LogInformation("Returning organization selection view...");
            return View(organizations);
        }

        public async Task<IActionResult> TimeframesIndex()
        {
            _logger.LogInformation("Moving to the timeframes step...");

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.ProgramManagerOnlyPolicy);
            var isInstructor = await _authService.AuthorizeAsync(User, RoleManagerService.InstructorOnlyPolicy);

            List<Timeframe>? timeframes;
            if (isInstructor.Succeeded)
            {
                timeframes = (await _dbServiceFactory.TimeframeService.GetTimeframesByOrganizationId(this.OrganizationId)).ToList();
            }
            else
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                var userTimeframes = (await _dbServiceFactory.UserTimeframeService.GetUserTimeframesByUserId(userId)).ToList();
                timeframes = userTimeframes.Select(x => x.Timeframe).Distinct().ToList();
            }

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public IActionResult TimeframeCreate()
        {
            _logger.LogInformation("A new timeframe needs to be created...");

            var timeframeVM = new TimeframeCreateVM()
            {
                OrganizationId = Guid.Parse(this.OrganizationId)
            };

            _logger.LogInformation("Returning timeframes creation view...");
            return View(timeframeVM);
        }

        [HttpPost]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> TimeframeCreate([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfProjects),nameof(Timeframe.NoOfRounds),nameof(Timeframe.StartDate), nameof(Timeframe.EndDate))] Timeframe timeframe, 
            IEnumerable<string> ProjectNames)
        {
            _logger.LogInformation("Creating a new timeframe...");
            if (string.IsNullOrEmpty(timeframe.Name))
            {
                ModelState.AddModelError(nameof(TimeframeCreateVM.Name), "Name is required");

                var timeframeVM = new TimeframeCreateVM()
                {
                    OrganizationId = timeframe.OrganizationId,
                };

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframeVM);
            }

            if(timeframe.NoOfProjects < 0)
            {
                ModelState.AddModelError(nameof(Timeframe.NoOfProjects), "Number of projects must be greater than or equal to 0");

                var timeframeVM = new TimeframeCreateVM()
                {
                    OrganizationId = timeframe.OrganizationId,
                };

                _logger.LogInformation("Returning organization creation view with error...");
                return View(timeframeVM);
            }

            await _manager.CreateTimeframe(timeframe, ProjectNames.ToList());

            _logger.LogInformation("Returning next view, projects selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id});
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> TimeframeEdit(int timeframeId)
        {
            _logger.LogInformation("Editing a timeframe...");

            var timeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframeId);

            if (timeframe == null)
            {
                _logger.LogWarning("Couldn't find timeframe...");
                return NotFound();
            }

            return View(timeframe);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> TimeframeEdit([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfRounds),nameof(Timeframe.StartDate), nameof(Timeframe.EndDate))] Timeframe timeframe)
        {
            if(string.IsNullOrEmpty(timeframe.Name))
            {
                ModelState.AddModelError(nameof(Timeframe.Name), "Name is required");
                return View(timeframe);
            }

            var oldTimeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframe.Id);

            oldTimeframe.Name = timeframe.Name;
            oldTimeframe.StartDate = timeframe.StartDate;
            oldTimeframe.EndDate = timeframe.EndDate;

            if(oldTimeframe.NoOfRounds > timeframe.NoOfRounds)
            {
                ModelState.AddModelError(nameof(Timeframe.NoOfRounds), "Number of rounds must be greater than or equal to the number of rounds that the round was created with.");
                return View(timeframe);
            }
            else
            {
                oldTimeframe.NoOfRounds = timeframe.NoOfRounds;
            }

            await _dbServiceFactory.TimeframeService.UpdateAsync(oldTimeframe);

            return RedirectToAction(nameof(UploadProcessController.TimeframesIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId});
        }

        public async Task<IActionResult> ProjectsIndex(int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.ProgramManagerOnlyPolicy);
            var isInstructor = await _authService.AuthorizeAsync(User, RoleManagerService.InstructorOnlyPolicy);
            
            List<Project>? projects;
            if (isInstructor.Succeeded)
            {
                projects = (await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId)).ToList();
            }
            else
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                var projectIds = (await _dbServiceFactory.TeamService.GetProjectIdsByTeamMember(userId, timeframeId, this.OrganizationId)).ToList();
                projects = (await _dbServiceFactory.ProjectService.GetProjectsByIds(projectIds)).ToList();
            }

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectCreate(int timeframeId)
        {
            if(timeframeId == 0)
            {
                _logger.LogWarning("No timeframe was specified.");
                return BadRequest();
            }

            var vm = await _manager.CreateProjectCreateViewModel(this.OrganizationId, timeframeId);

            _logger.LogInformation("Returning project create view...");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectCreate([Bind(nameof(Project.Name), nameof(Project.Description))] Project project, 
            string POCEmail, string ManagerEmail, string NewTeamMembers, int timeframeId)
        {
            if(timeframeId == 0)
            {
                _logger.LogWarning("No timeframe was specified.");
                return BadRequest();
            }

            var timeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframeId);

            if(timeframe == null)
            {
                _logger.LogWarning("Timeframe not found.");
                return NotFound();
            }

            var vm = await _manager.CreateProjectCreateViewModel(this.OrganizationId, timeframeId);

            if(string.IsNullOrEmpty(project.Name))
            {
                _logger.LogWarning("Project name is empty");
                return View(vm);
            }

            var returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectsIndex?timeframeId={timeframeId}";

            try {
                await _manager.CreateProject(project, timeframe, this.OrganizationId, timeframeId, POCEmail, ManagerEmail, NewTeamMembers);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = returnUrl});
            }

            _logger.LogInformation("Project created successfully, returning to index...");
            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId, timeframeId = timeframeId });
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectEdit(int timeframeId, string projectId)
        {
            if(timeframeId == 0 || string.IsNullOrEmpty(projectId))
            {
                _logger.LogWarning("No timeframe or project id was specified.");
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId);

            if(project == null)
            {
                _logger.LogWarning("Project with id {projectId} was not found.", projectId);
                return NotFound();
            }

            var vm = await _manager.CreateProjectEditViewModel(this.OrganizationId, timeframeId, projectId);

            _logger.LogInformation("Returning project edit view...");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectEdit(ProjectEditVM vm, string POCEmail, string ManagerEmail, string NewTeamMembers)
        {
            var newProject = vm.Project;

            if(newProject == null || newProject.Id == Guid.Empty)
            {
                _logger.LogWarning("Received project was empty.");
                return BadRequest();
            }

            var oldProject = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(newProject.Id.ToString());

            if(oldProject == null)
            {
                _logger.LogWarning("Can't find the old project.");
                return NotFound();
            }

            var newVM = await _manager.CreateProjectEditViewModel(this.OrganizationId, oldProject.TimeframeId, oldProject.Id.ToString());

            vm.PotentialPOCs = newVM.PotentialPOCs;
            vm.PotentialManagers = newVM.PotentialManagers;
            vm.PotentialTeamMembers = newVM.PotentialTeamMembers;
            vm.Project.TeamMembers = newVM.Project.TeamMembers;   

            if(string.IsNullOrEmpty(NewTeamMembers))
            {
                ModelState.AddModelError("Project.NoOfMembers", "Number of members and number of selected members did not match.");
                _logger.LogWarning("Did not get expected list of team members.");
                return View(vm);
            }

            var newTeamMembers = NewTeamMembers.Split(",").ToList();

            if(newTeamMembers.Count != newProject.NoOfMembers && newProject.NoOfMembers != 0)
            {
                ModelState.AddModelError("Project.NoOfMembers", "Number of members and number of selected members did not match.");
                _logger.LogWarning("Number of requested team members didn't team members received.");
                return View(vm);
            }

            if(newTeamMembers.Count < 2 && newProject.NoOfMembers != 0)
            {
                ModelState.AddModelError("NewTeamMembers", "There must be at least 2 members on this team.");
                _logger.LogWarning("Number of requested team members was less than 2.");
                return View(vm);
            }

            if(newProject.NoOfRounds < oldProject.NoOfRounds && newProject.NoOfRounds != 0)
            {
                ModelState.AddModelError("Project.NoOfRounds", "Number of rounds can't be less than the number of existing round folders.");
                _logger.LogWarning("Number of round folders requested is less than the number of round folders already specified.");
                return View(vm);
            }

            if(newProject.NoOfRounds != oldProject.Timeframe.NoOfRounds && newProject.NoOfRounds != 0)
            {
                ModelState.AddModelError("Project.NoOfRounds", "This timeframe already requires {oldProject.Timeframe.NoOfRounds} rounds.");
                _logger.LogWarning("Can't create more round folders than have already been specified by the timeframe.");
                return View(vm);
            }

            var returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectEdit?timeframeId={oldProject.TimeframeId}&projectId={oldProject.Id}";
            
            try {
                oldProject = await _manager.EditProject(newProject, oldProject, this.OrganizationId, oldProject.TimeframeId, POCEmail, ManagerEmail, newTeamMembers);
                await _dbServiceFactory.ProjectService.UpdateAsync(oldProject);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = returnUrl});
            }            

            _logger.LogInformation("Completed editing project, returning to the projects index...");
            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId, timeframeId = vm.Project.TimeframeId, projectId = vm.Project.Id });
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectRoundCreate(int timeframeId)
        {
            _logger.LogInformation("Project rounds need to be created...");

            var timeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframeId);
            if(timeframe.NoOfRounds > 0)
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(new Project
                {
                    NoOfRounds = timeframe.NoOfRounds
                });
            }

            var projects = await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId, true);
            if(projects.Count() >  0) 
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(projects.ElementAt(0));
            }

            _logger.LogInformation("Returning rounds creation view...");
            return View(new Project { NoOfRounds = 0 });
        }

        [HttpPost]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectRoundCreate([Bind(nameof(Project.Id),nameof(Project.OrganizationId),nameof(Project.TimeframeId),nameof(Project.NoOfRounds))] Project project, 
            List<DateTime> RoundStartDates, List<DateTime> RoundEndDates)
        {
            _logger.LogInformation("Creating project rounds...");

            await _manager.CreateProjectRounds(project, RoundStartDates, RoundEndDates);

            _logger.LogInformation("Returning to project selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = project.OrganizationId, timeframeId = project.TimeframeId });
        }

        public async Task<IActionResult> ProjectRoundsIndex(int timeframeId, string projectId)
        {
            _logger.LogInformation("Moving to the rounds step...");
            var projectRounds = (await _dbServiceFactory.ProjectRoundService.GetProjectRoundsByProjectId(projectId)).ToList();

            if(projectRounds.Count == 0)
            {
                projectRounds =
                [
                    new ProjectRound {
                        Project = new Project(),
                        Round = new Round()
                    },
                ];
            }

            var vm = new ProjectRoundsIndexVM
            {
                ProjectRounds = projectRounds,
                OrganizationId = this.OrganizationId,
                TimeframeId = timeframeId,
            };

            _logger.LogInformation("Returning rounds selection view...");
            return View(vm);
        }

        public async Task<IActionResult> FeedbackPdfsIndex(int timeframeId, string projectId, int roundId)
        {
            _logger.LogInformation("Moving to the feedback step...");

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.ProgramManagerOnlyPolicy);
            var isSponsor = await _authService.AuthorizeAsync(User, RoleManagerService.InstructorOnlyPolicy);
            
            List<FeedbackPdf>? pdfs;
            if (isAdmin.Succeeded || isSponsor.Succeeded)
            {
                pdfs = (await _dbServiceFactory.FeedbackPdfService.GetFeedbackByProjectIdAndRoundId(this.OrganizationId.ToString(), timeframeId, projectId, roundId)).ToList();
            }
            else
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                pdfs = (await _dbServiceFactory.FeedbackPdfService.GetFeedbackPdfsByUserId(this.OrganizationId, timeframeId, projectId, roundId, userId)).ToList();
            }

            if(pdfs.Count == 0)
            {
                if(!isAdmin.Succeeded && !isSponsor.Succeeded)
                {
                    return RedirectToAction(nameof(UploadProcessController.FeedbackIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId, timeframeId = timeframeId, projectId = projectId, roundId = roundId });
                }
                else
                {
                    pdfs =
                    [
                        new FeedbackPdf {
                            Project = new Project(),
                            Round = new Round(),
                            User = new User()
                        },
                    ];
                }
            }

            _logger.LogInformation("Return feedback pdf view...");
            return View(pdfs);
        }

        public async Task<IActionResult> FeedbackIndex(int timeframeId, string projectId, int roundId)
        {
            _logger.LogInformation("Moving to the survey submission step...");

            var isSysAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.SystemAdministratorOnlyPolicy);

            List<Feedback>? feedback;

            if(isSysAdmin.Succeeded)
            {
                feedback = (await _dbServiceFactory.FeedbackService.GetFeedbackByTimeframeIdAndRoundIdAndProjectId(this.OrganizationId, timeframeId, projectId, roundId)).ToList();
            }
            else
            {
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                feedback = (await _dbServiceFactory.FeedbackService.GetFeedbackByTimeframeIdAndRoundIdAndProjectIdAndUserId(this.OrganizationId, timeframeId, projectId, roundId, userId)).ToList();
            }

            return View(feedback);
        }

        [HttpGet]
        public async Task<IActionResult> SubmitFeedback(string feedbackId)
        {
            _logger.LogInformation("Submit feedback...");

            var isSysAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.SystemAdministratorOnlyPolicy);

            if(string.IsNullOrEmpty(feedbackId))
            {
                return BadRequest();
            }

            var feedback = await _dbServiceFactory.FeedbackService.GetByIdAsync(feedbackId);

            if(feedback == null)
            {
                return NotFound();
            }

            var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
            var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

            userId = userId == localUserId ? userId : localUserId;

            if(feedback.ReviewerId != Guid.Parse(userId) && !isSysAdmin.Succeeded)
            {
                _logger.LogCritical("UNAUTHORIZED ACCESS ATTEMPT: User with id {0} tried to access feedback form for user with id {1}. Feedback Id: {2}", userId, feedback.RevieweeId, feedback.Id);
                return RedirectToAction(nameof(AccountController.AccessDenied), AccountController.Name);
            }

            return View(feedback);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(Feedback feedback)
        {
            return View();
        }

        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> RouteUploadRoster(int timeframeId)
        {
            var orgType = (await _dbServiceFactory.OrganizationService.GetByIdAsync(Guid.Parse(OrganizationId))).Type;
            return RedirectToAction(nameof(BaseController.UploadRoster), orgType, new { area = orgType, organizationId = OrganizationId, timeframeId = timeframeId });
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> AssignPOC(string projectId)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var vm = await _manager.CreateAssignPOCViewModel(this.OrganizationId, projectId);            

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> AssignPOC(string projectId, string POCEmail, string SelectedPOC)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId);

            if(project == null)
            {
                return NotFound();
            }

            var vm = await _manager.CreateAssignPOCViewModel(this.OrganizationId, projectId);            

            if(string.IsNullOrEmpty(POCEmail) && string.IsNullOrEmpty(SelectedPOC))
            {
                return View(vm);
            }

            var email = string.IsNullOrEmpty(POCEmail) ? SelectedPOC : POCEmail;

            var returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}";
            
            try {
                await _manager.AssignPOCToProject(this.OrganizationId, projectId, email);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = returnUrl});
            }

            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), new { timeframeId = project.TimeframeId, organizationId = OrganizationId });
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.InstructorOnlyPolicy)]
        public async Task<IActionResult> AssignManager(string projectId)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var vm = await _manager.CreateAssignManagerViewModel(this.OrganizationId, projectId);          

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.InstructorOnlyPolicy)]
        public async Task<IActionResult> AssignManager(string projectId, string ManagerEmail, string SelectedManager)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetProjectAndTeamMembersById(projectId);

            if(project == null)
            {
                return NotFound();
            }

            var vm = await _manager.CreateAssignManagerViewModel(this.OrganizationId, projectId);          

            if(string.IsNullOrEmpty(ManagerEmail) && string.IsNullOrEmpty(SelectedManager))
            {
                return View(vm);
            }

            var email = string.IsNullOrEmpty(ManagerEmail) ? SelectedManager : ManagerEmail;

            var returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignManager?projectId={projectId}";

            try {
                await _manager.AssignManagerToProject(this.OrganizationId, projectId, email);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = returnUrl});
            }

            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), new { timeframeId = project.TimeframeId, organizationId = OrganizationId });
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public IActionResult UploadBlackboardRoster(int timeframeId)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> UploadBlackboardRoster(IFormFile roster, int timeframeId)
        {
            if(roster == null || timeframeId == 0)
            {
                return BadRequest("One or more parameters was null.");
            }

            try {
                await _manager.UploadBlackboardRoster(roster, this.OrganizationId, timeframeId);
                return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId, timeframeId = timeframeId });
            }
            catch
            {
                return BadRequest("There was an error processing the uploaded feedback.");
            }
        }

        [HttpGet]
        [Authorize(Policy = RoleManagerService.InstructorOnlyPolicy)]
        public async Task<IActionResult> ManageRoster(int timeframeId)
        {
            _logger.LogInformation("Moving to the manage roster page...");

            if(timeframeId == 0)
            {
                _logger.LogInformation("Couldn't find the timeframe id.");
                return BadRequest();
            }

            var userTimeframes = await _dbServiceFactory.UserTimeframeService.GetUsersByTimeframeId(timeframeId);
            var users = userTimeframes.Select(x => x.User).ToList();
            var teamMembers = (await _dbServiceFactory.TeamService.GetTeamMembersByTimeframeId(this.OrganizationId, timeframeId)).ToList();

            foreach(var user in users)
            {
                if(!teamMembers.Any(x => x.UserId == user.Id))
                {
                    teamMembers.Add(new TeamMember {
                        UserId = user.Id,
                        User = user
                    });
                }
            }

            teamMembers = teamMembers.OrderBy(x => x.User.FirstName).ToList();

            var projects = (await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId)).ToList() ?? new List<Project>();

            var vm = new ManageRosterVM
            {
                Roster = teamMembers,
                Projects = projects
            };

            _logger.LogInformation("Fetched users, returning the roster page.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.ProgramManagerOnlyPolicy)]
        public async Task<IActionResult> ProjectCreateSilent([FromBody] JsonElement data, [FromQuery] int timeframeId)
        {
            string userEmail = data.GetProperty("userEmail").GetString();
            string projectId = data.GetProperty("projectId").GetString();
            string newProjectName = data.GetProperty("newProjectName").GetString();

            if(timeframeId == 0)
            {
                _logger.LogWarning("No timeframe was specified.");
                return BadRequest();
            }

            var timeframe = await _dbServiceFactory.TimeframeService.GetByIdAsync(timeframeId);

            if(timeframe == null)
            {
                _logger.LogWarning("Timeframe not found.");
                return NotFound();
            }

            

            if(string.IsNullOrEmpty(userEmail))
            {
                _logger.LogWarning("No user email was specified.");
                return BadRequest();
            }

            var user = await _dbServiceFactory.UserService.GetUserByEmail(userEmail);

            if(user == null)
            {
                _logger.LogWarning("User not found.");
                return NotFound();
            }

            if(projectId == "0" && string.IsNullOrEmpty(newProjectName))
            {
                _logger.LogWarning("No project Id was specified.");
                return BadRequest();
            }

            if(projectId != "0")
            {
                var existingProject = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));

                if(existingProject == null)
                {
                    _logger.LogWarning("Project not found.");
                    return NotFound();
                }

                await _manager.CreateProjectSilently(timeframe, this.OrganizationId, timeframeId, projectId, userEmail);
                return StatusCode(StatusCodes.Status200OK, new { message = "Existing project updated successfully.", newProject = false, projectId = projectId, projectName = existingProject.Name });
            }
            else if(!string.IsNullOrEmpty(newProjectName))
            {
                var project = await _manager.CreateProjectSilently(timeframe, this.OrganizationId, timeframeId, projectId, userEmail, newProjectName);
                return StatusCode(StatusCodes.Status200OK, new { message = "New project created successfully.", newProject = true, projectId = project.Id.ToString(), projectName = project.Name });
            }   
            else
            {
                return BadRequest();
            }         
        }
    }
}
