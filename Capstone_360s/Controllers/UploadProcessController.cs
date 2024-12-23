using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Services.Configuration;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

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
        private readonly IRoleManager _roleManager;
        private readonly IAuthorizationService _authService;
        private readonly CustomConfigurationService _config;
        private readonly ILogger<UploadProcessController> _logger;
        public UploadProcessController(
            IManageFeedback manager,
            IFeedbackDbServiceFactory serviceFactory,
            IRoleManager roleManager,
            IAuthorizationService authService,
            CustomConfigurationService config,
            ILogger<UploadProcessController> logger) 
        { 
            _manager = manager;
            _dbServiceFactory = serviceFactory;
            _roleManager = roleManager;
            _authService = authService;
            _config = config;
            _logger = logger;
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.AdminOnlyPolicy);

            List<Timeframe>? timeframes;
            if (isAdmin.Succeeded)
            {
                timeframes = (await _dbServiceFactory.TimeframeService.GetTimeframesByOrganizationId(this.OrganizationId)).ToList();
            }
            else
            {
                var timeframeIds = (await _dbServiceFactory.TeamService.GetTimeframeIdsByTeamMember(User.FindFirst(x => x.Type == "uid").Value, this.OrganizationId)).ToList();
                timeframes = (await _dbServiceFactory.TimeframeService.GetTimeframesByIds(timeframeIds)).ToList();
            }

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> TimeframeCreate([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfProjects),nameof(Timeframe.NoOfRounds))] Timeframe timeframe, 
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

        public async Task<IActionResult> ProjectsIndex(int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.AdminOnlyPolicy);
            
            List<Project>? projects;
            if (isAdmin.Succeeded)
            {
                projects = (await _dbServiceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId)).ToList();
            }
            else
            {
                var projectIds = (await _dbServiceFactory.TeamService.GetProjectIdsByTeamMember(User.FindFirst(x => x.Type == "uid").Value, timeframeId, this.OrganizationId)).ToList();
                projects = (await _dbServiceFactory.ProjectService.GetProjectsByIds(projectIds)).ToList();
            }

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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

            var isAdmin = await _authService.AuthorizeAsync(User, RoleManagerService.AdminOnlyPolicy);
            var isSponsor = await _authService.AuthorizeAsync(User, RoleManagerService.SponsorOnlyPolicy);
            
            List<FeedbackPdf>? pdfs;
            if (isAdmin.Succeeded || isSponsor.Succeeded)
            {
                pdfs = (await _dbServiceFactory.FeedbackPdfService.GetFeedbackByProjectIdAndRoundId(this.OrganizationId.ToString(), timeframeId, projectId, roundId)).ToList();
            }
            else
            {
                pdfs = (await _dbServiceFactory.FeedbackPdfService.GetFeedbackPdfsByUserId(this.OrganizationId, timeframeId, projectId, roundId, User.FindFirst(x => x.Type == "uid").Value)).ToList();
            }

            if(pdfs.Count == 0)
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

            _logger.LogInformation("Return feedback pdf view...");
            return View(pdfs);
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> RouteUploadRoster(int timeframeId)
        {
            var orgType = (await _dbServiceFactory.OrganizationService.GetByIdAsync(Guid.Parse(OrganizationId))).Type;
            return RedirectToAction(nameof(BaseController.UploadRoster), orgType, new { area = orgType, organizationId = OrganizationId, timeframeId = timeframeId });
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> AssignPOC(string projectId)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));
            var vm = new AssignPOCVM()
            {
                Project = project,
            };

            var users = await _roleManager.GetUsersByRole(_config.Sponsor);
            var items = new List<SelectListItem>();

            if(users != null)
            {
                foreach(var user in users)
                {
                    items.Add(new SelectListItem{
                        Text = user.GetFullName(),
                        Value = user.Email
                    });
                }
            }

            vm.POCs = items;            

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> AssignPOC(string projectId, string POCEmail, string SelectedPOC)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));

            if(string.IsNullOrEmpty(POCEmail) && string.IsNullOrEmpty(SelectedPOC))
            {
                var pocs = await _roleManager.GetUsersByRole(_config.Sponsor);
                var items = new List<SelectListItem>();

                if(pocs != null)
                {
                    foreach(var poc in pocs)
                    {
                        items.Add(new SelectListItem{
                            Text = poc.GetFullName(),
                            Value = poc.Email
                        });
                    }
                } 

                var vm = new AssignPOCVM()
                {
                    Project = project,
                    POCs = items
                };

                return View(vm);
            }

            var email = string.IsNullOrEmpty(POCEmail) ? SelectedPOC : POCEmail;

            var user = await _dbServiceFactory.UserService.GetUserByEmail(email);
            if(user.Id == Guid.Empty)
            {
                try {
                    user = await _roleManager.AddNewUser(email);
                } 
                catch (UnauthorizedAccessException ex) {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                }

                if(user.Id == Guid.Empty)
                {
                    return BadRequest($"Couldn't add user");
                }

                await _roleManager.AddUserToRole(user.MicrosoftId.ToString(), _config.Sponsor);
            }
            else
            {
                if(user.MicrosoftId == null || user.MicrosoftId == Guid.Empty)
                {
                    try {
                        user = await _roleManager.AddNewUser(email);
                    } 
                    catch (UnauthorizedAccessException ex) {
                        return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                    } 
                }

                var roles = await _roleManager.GetRoles(user.Id);
                if(!roles.Contains(_config.Sponsor))
                {
                    await _roleManager.AddUserToRole(user.MicrosoftId.ToString(), _config.Sponsor);
                }
            }

            project.POCId = user.Id;

            await _dbServiceFactory.ProjectService.UpdateAsync(project);

            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), new { timeframeId = project.TimeframeId, organizationId = OrganizationId });
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        public async Task<IActionResult> AssignManager(string projectId)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));
            var vm = new AssignPOCVM()
            {
                Project = project,
            };

            var users = await _roleManager.GetUsersByRole(_config.Lead);
            var items = new List<SelectListItem>();

            if(users != null)
            {
                foreach(var user in users)
                {
                    items.Add(new SelectListItem{
                        Text = user.GetFullName(),
                        Value = user.Email
                    });
                }
            }

            vm.POCs = items;            

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
        
        public async Task<IActionResult> AssignManager(string projectId, string ManagerEmail, string SelectedManager)
        {
            if(string.IsNullOrEmpty(projectId))
            {
                return BadRequest();
            }

            var project = await _dbServiceFactory.ProjectService.GetByIdAsync(Guid.Parse(projectId));
            var vm = new AssignPOCVM();

            if(string.IsNullOrEmpty(ManagerEmail) && string.IsNullOrEmpty(SelectedManager))
            {
                var pocs = await _roleManager.GetUsersByRole(_config.Lead);
                var items = new List<SelectListItem>();

                if(pocs != null)
                {
                    foreach(var poc in pocs)
                    {
                        items.Add(new SelectListItem{
                            Text = poc.GetFullName(),
                            Value = poc.Email
                        });
                    }
                } 

                vm.Project = project;
                vm.POCs = items;

                return View(vm);
            }

            var email = string.IsNullOrEmpty(ManagerEmail) ? SelectedManager : ManagerEmail;

            var user = await _dbServiceFactory.UserService.GetUserByEmail(email);
            if(user.Id == Guid.Empty)
            {
                try {
                    user = await _roleManager.AddNewUser(email);
                } 
                catch (UnauthorizedAccessException ex) {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                }

                if(user.Id == Guid.Empty)
                {
                    return View(vm);
                }

                await _roleManager.AddUserToRole(user.MicrosoftId.ToString(), _config.Lead);
            }
            else
            {
                if(user.MicrosoftId == null || user.MicrosoftId == Guid.Empty)
                {
                    try {
                        user = await _roleManager.AddNewUser(email);
                    } 
                    catch (UnauthorizedAccessException ex) {
                        return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                    } 
                }

                var roles = await _roleManager.GetRoles(user.Id);
                if(!roles.Contains(_config.Lead))
                {
                    await _roleManager.AddUserToRole(user.MicrosoftId.ToString(), _config.Lead);
                }
            }

            project.ManagerId = user.Id;

            await _dbServiceFactory.ProjectService.UpdateAsync(project);

            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), new { timeframeId = project.TimeframeId, organizationId = OrganizationId });
        }
    }
}
