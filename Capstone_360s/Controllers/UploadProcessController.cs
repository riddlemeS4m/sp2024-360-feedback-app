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
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                var timeframeIds = (await _dbServiceFactory.TeamService.GetTimeframeIdsByTeamMember(userId, this.OrganizationId)).ToList();
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
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                var projectIds = (await _dbServiceFactory.TeamService.GetProjectIdsByTeamMember(userId, timeframeId, this.OrganizationId)).ToList();
                projects = (await _dbServiceFactory.ProjectService.GetProjectsByIds(projectIds)).ToList();
            }

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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

            var currentTeamMemberIds = project.TeamMembers.Select(x => x.UserId);

            var pocs = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Sponsor);
            var managers = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Lead);
            var teamMembers = await _dbServiceFactory.UserOrganizationService.GetUsersByOrganizationId(Guid.Parse(this.OrganizationId));

            var pocItems = pocs.OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.FirstName} {x.LastName}",
                    Value = x.Email
                }).ToList();
            var managerItems = managers.OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.FirstName} {x.LastName}",
                    Value = x.Email
                })
                .ToList();
            var memberItems = teamMembers.OrderBy(x => x.User.FirstName)
                .Where(x => !currentTeamMemberIds.Contains(x.UserId))
                .Select(x => new SelectListItem { 
                    Text = $"{x.User.FirstName} {x.User.LastName}",
                    Value = x.User.Email
                })
                .ToList();

            var vm = new ProjectEditVM()
            {
                PotentialPOCs = pocItems ?? [],
                PotentialManagers = managerItems ?? [],
                PotentialTeamMembers = memberItems ?? [],
                Project = project
            };

            _logger.LogInformation("Returning project edit view...");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
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
                return BadRequest();
            }

            var currentTeamMemberIds = oldProject.TeamMembers.Select(x => x.UserId);
                      

            var pocs = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Sponsor);
            var managers = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Lead);
            var teamMembers = await _dbServiceFactory.UserOrganizationService.GetUsersByOrganizationId(Guid.Parse(this.OrganizationId));

            var pocItems = pocs.OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.FirstName} {x.LastName}",
                    Value = x.Email
                })
                .ToList();
            var managerItems = managers.OrderBy(x => x.FirstName)
                .Select(x => new SelectListItem { 
                    Text = $"{x.FirstName} {x.LastName}",
                    Value = x.Email
                })
                .ToList();
            var memberItems = teamMembers.OrderBy(x => x.User.FirstName)
                .Where(x => !currentTeamMemberIds.Contains(x.UserId))
                .Select(x => new SelectListItem { 
                    Text = $"{x.User.FirstName} {x.User.LastName}",
                    Value = x.User.Email
                })
                .ToList();

            vm.PotentialPOCs = pocItems ?? [];
            vm.PotentialManagers = managerItems ?? [];
            vm.PotentialTeamMembers = memberItems ?? [];
            vm.Project.TeamMembers = oldProject.TeamMembers;   

            var pocCondition = oldProject.POC.Email != POCEmail && !string.IsNullOrEmpty(POCEmail);
            var managerCondition =  oldProject.Manager.Email != ManagerEmail && !string.IsNullOrEmpty(ManagerEmail);        

            oldProject.Name = oldProject.Name != newProject.Name && !string.IsNullOrEmpty(newProject.Name) ? newProject.Name : oldProject.Name;
            oldProject.Description = oldProject.Description != newProject.Description && !string.IsNullOrEmpty(newProject.Description) ? newProject.Description : oldProject.Description;

            if(pocCondition)
            {
                try {
                    oldProject.POC = await _roleManager.AddNewUser(this.OrganizationId, POCEmail);
                }
                catch {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectEdit?timeframeId={oldProject.TimeframeId}&projectId={oldProject.Id}"});
                }
                
                await _roleManager.AddUserToRole(this.OrganizationId, oldProject.POC.Id.ToString(), _config.Sponsor);
            }

            if(managerCondition)
            {
                try {
                    oldProject.Manager = await _roleManager.AddNewUser(this.OrganizationId, ManagerEmail);
                }
                catch {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectEdit?timeframeId={oldProject.TimeframeId}&projectId={oldProject.Id}"});
                }

                await _roleManager.AddUserToRole(this.OrganizationId, oldProject.Manager.Id.ToString(), _config.Lead);
            }

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

            var oldTeamMembers = oldProject.TeamMembers.Select(x => x.User.Email).ToList();

            if(newTeamMembers.Count > 0 && oldTeamMembers.Count >= 0)
            {
                foreach(var email in newTeamMembers)
                {
                    if(oldTeamMembers.Count == 0 || !oldTeamMembers.Contains(email))
                    {
                        var user = await _dbServiceFactory.UserService.GetUserByEmail(email);
                        if(user.Id == Guid.Empty)
                        {
                            try {
                                user = await _roleManager.AddNewUser(this.OrganizationId, email);
                            } 
                            catch (UnauthorizedAccessException ex) {
                                return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectEdit?timeframeId={oldProject.TimeframeId}&projectId={oldProject.Id}"});
                            }

                            if(user.Id == Guid.Empty)
                            {
                                return BadRequest($"Couldn't add user");
                            }
                        }
                        else
                        {
                            if(user.MicrosoftId == null || user.MicrosoftId == Guid.Empty)
                            {
                                try {
                                    user = await _roleManager.AddNewUser(this.OrganizationId, email);
                                } 
                                catch (UnauthorizedAccessException ex) {
                                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/ProjectEdit?timeframeId={oldProject.TimeframeId}&projectId={oldProject.Id}"});
                                } 
                            }
                        }

                        await _dbServiceFactory.TeamService.AddAsync(new TeamMember {
                            UserId = user.Id,
                            ProjectId = oldProject.Id,
                        });
                    }
                }
            }

            if(newTeamMembers.Count >= 0 && oldTeamMembers.Count > 0)
            {
                foreach(var email in oldTeamMembers)
                {
                    if(!newTeamMembers.Contains(email))
                    {
                        var teamMember = oldProject.TeamMembers.Where(x => x.User.Email == email).FirstOrDefault();
                        await _dbServiceFactory.TeamService.Remove(teamMember);
                    }
                }
            }

            oldProject.NoOfMembers = oldProject.NoOfMembers != newTeamMembers.Count ? newTeamMembers.Count : oldProject.NoOfMembers;

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

            if(oldProject.NoOfRounds < newProject.NoOfRounds && newProject.NoOfRounds != 0)
            {
                oldProject.NoOfRounds = newProject.NoOfRounds;
                await _manager.CreateProjectRoundsForOneProject(oldProject);
            }

            await _dbServiceFactory.ProjectService.UpdateAsync(oldProject);

            _logger.LogInformation("Completed editing project, returning to the projects index...");
            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), UploadProcessController.Name, new { organizationId = this.OrganizationId, timeframeId = vm.Project.TimeframeId, projectId = vm.Project.Id });
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
                var userId = User.Claims.FirstOrDefault(x => x.Type == "uid")?.Value;
                var localUserId = User.Claims.FirstOrDefault(x => x.Type == "LocalUser")?.Value;

                userId = userId == localUserId ? userId : localUserId;

                pdfs = (await _dbServiceFactory.FeedbackPdfService.GetFeedbackPdfsByUserId(this.OrganizationId, timeframeId, projectId, roundId, userId)).ToList();
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

            var users = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Sponsor);
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
                var pocs = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Sponsor);
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
                    user = await _roleManager.AddNewUser(this.OrganizationId, email);
                } 
                catch (UnauthorizedAccessException ex) {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                }

                if(user.Id == Guid.Empty)
                {
                    return BadRequest($"Couldn't add user");
                }

                await _roleManager.AddUserToRole(this.OrganizationId, user.MicrosoftId.ToString(), _config.Sponsor);
            }
            else
            {
                if(user.MicrosoftId == null || user.MicrosoftId == Guid.Empty)
                {
                    try {
                        user = await _roleManager.AddNewUser(this.OrganizationId, email);
                    } 
                    catch (UnauthorizedAccessException ex) {
                        return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignPOC?projectId={projectId}"});
                    } 
                }

                var roles = await _roleManager.GetRoles(user.Id);
                if(!roles.Contains(_config.Sponsor))
                {
                    await _roleManager.AddUserToRole(this.OrganizationId, user.MicrosoftId.ToString(), _config.Sponsor);
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

            var users = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Lead);
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
                var pocs = await _roleManager.GetUsersByRole(this.OrganizationId, _config.Lead);
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
                    user = await _roleManager.AddNewUser(this.OrganizationId, email);
                } 
                catch (UnauthorizedAccessException ex) {
                    return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignManager?projectId={projectId}"});
                }

                if(user.Id == Guid.Empty)
                {
                    return View(vm);
                }

                await _roleManager.AddUserToRole(this.OrganizationId, user.MicrosoftId.ToString(), _config.Lead);
            }
            else
            {
                if(user.MicrosoftId == null || user.MicrosoftId == Guid.Empty)
                {
                    try {
                        user = await _roleManager.AddNewUser(this.OrganizationId, email);
                    } 
                    catch (UnauthorizedAccessException ex) {
                        return RedirectToAction(nameof(AccountController.Login), AccountController.Name, new { returnUrl = $"/{this.OrganizationId}/UploadProcess/AssignManager?projectId={projectId}"});
                    } 
                }

                var roles = await _roleManager.GetRoles(user.Id);
                if(!roles.Contains(_config.Lead))
                {
                    await _roleManager.AddUserToRole(this.OrganizationId, user.MicrosoftId.ToString(), _config.Lead);
                }
            }

            project.ManagerId = user.Id;

            await _dbServiceFactory.ProjectService.UpdateAsync(project);

            return RedirectToAction(nameof(UploadProcessController.ProjectsIndex), new { timeframeId = project.TimeframeId, organizationId = OrganizationId });
        }
    }
}
