using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Services.FeedbackDb;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public class UploadProcessController : Controller
    {
        private readonly IGoogleDrive _googleDriveService;
        private readonly OrganizationService _organizationService;
        private readonly TimeframeService _timeframeService;
        private readonly ProjectService _projectService;
        private readonly RoundService _roundService;
        private readonly ProjectRoundService _projectRoundService;
        private readonly MetricService _metricService;
        private readonly QuestionService _questionService;
        private readonly ILogger<UploadProcessController> _logger;
        public UploadProcessController(IGoogleDrive googleDriveService,
            OrganizationService organizationService,
            TimeframeService timeframeService,
            ProjectService projectService,
            RoundService roundService,
            ProjectRoundService projectRoundService,
            MetricService metricService,
            QuestionService questionService,
            ILogger<UploadProcessController> logger) 
        { 
            _googleDriveService = googleDriveService;
            _organizationService = organizationService;
            _timeframeService = timeframeService;
            _projectService = projectService;
            _roundService = roundService;
            _projectRoundService = projectRoundService;
            _metricService = metricService;
            _questionService = questionService;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting upload process...");

            var organizations = await _organizationService.GetAllAsync();

            _logger.LogInformation("Returning organization selection view...");
            return View(organizations);
        }

        public IActionResult OrganizationCreate()
        {
            _logger.LogInformation("A new organization needs to be created...");
            _logger.LogInformation("Returning organization creation view...");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OrganizationCreate([Bind(nameof(Organization.Id),nameof(Organization.Name))] Organization organization, 
            List<string> Names, List<string> Descriptions, List<int> Mins, List<int> Maxs, List<string> Qs, List<string> Examples)
        {
            _logger.LogInformation("Creating a new organization...");
            if(string.IsNullOrEmpty(organization.Name))
            {
                ModelState.AddModelError(nameof(Organization.Name), "Name is required");

                _logger.LogInformation("Returning organization creation view with error...");
                return View(organization);
            }

            organization.GDFolderId = await _googleDriveService.CreateFolderAsync(organization.Name, "");

            await _organizationService.AddAsync(organization);

            var metrics = new List<Metric>();
            for (int i = 0; i < Names.Count; i++)
            {
                var metric = new Metric()
                {
                    Name = Names[i],
                    Description = Descriptions[i],
                    MinValue = Mins[i],
                    MaxValue = Maxs[i],
                    OrganizationId = organization.Id
                };

                metrics.Add(metric);
            }

            await _metricService.AddRange(metrics);

            var questions = new List<Question>();
            for (int i = 0; i < Qs.Count; i++)
            {
                var question = new Question()
                {
                    Q = Qs[i],
                    Example = Examples[i],
                    OrganizationId = organization.Id
                };

                questions.Add(question);
            }

            await _questionService.AddRange(questions);

            _logger.LogInformation("Returning next view, timeframes selection view...");
            return RedirectToAction(nameof(TimeframesIndex), new {id = organization.Id});
        }

        public async Task<IActionResult> TimeframesIndex(string id)
        {
            _logger.LogInformation("Moving to the timeframes step...");
            var timeframes = await _timeframeService.GetTimeframesByOrganizationId(id);

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

        public IActionResult TimeframeCreate(string organizationId)
        {
            _logger.LogInformation("A new timeframe needs to be created...");

            var timeframeVM = new TimeframeCreateVM()
            {
                OrganizationId = Guid.Parse(organizationId)
            };

            _logger.LogInformation("Returning timeframes creation view...");
            return View(timeframeVM);
        }

        [HttpPost]
        public async Task<IActionResult> TimeframeCreate([Bind(nameof(Timeframe.Id),nameof(Timeframe.OrganizationId),nameof(Timeframe.Name),nameof(Timeframe.NoOfProjects),nameof(Timeframe.NoOfRounds))] Timeframe timeframe, IEnumerable<string> ProjectNames)
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

            var organizationFolderId = (await _organizationService.GetByIdAsync(timeframe.OrganizationId)).GDFolderId;
            timeframe.GDFolderId = await _googleDriveService.CreateFolderAsync(timeframe.Name, organizationFolderId);

            await _timeframeService.AddAsync(timeframe);

            if(timeframe.NoOfProjects == 0)
            {
                _logger.LogInformation("Returning next view, projects selection view...");
                return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id });
            }

            var projects = new List<Project>();
            for (int i = 0; i < timeframe.NoOfProjects; i++)
            {
                var projectName = ProjectNames.ElementAt(i) ?? $"Project {i + 1}";

                var projectFolderId = await _googleDriveService.CreateFolderAsync(
                    projectName,
                    timeframe.GDFolderId
                );

                var project = new Project()
                {
                    Name = projectName,
                    TimeframeId = timeframe.Id,
                    OrganizationId = timeframe.OrganizationId,
                    GDFolderId = projectFolderId,
                    NoOfRounds = timeframe.NoOfRounds
                };

                projects.Add(project);
            }

            await _projectService.AddRange(projects);

            _logger.LogInformation("Returning next view, projects selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id});
        }

        public async Task<IActionResult> ProjectsIndex(string organizationId, int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");
            var projects = await _projectService.GetProjectsByTimeframeId(organizationId, timeframeId);

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        public async Task<IActionResult> ProjectRoundCreate(string organizationId, int timeframeId)
        {
            _logger.LogInformation("Project rounds need to be created...");
            var timeframe = await _timeframeService.GetByIdAsync(timeframeId);
            if(timeframe.NoOfRounds > 0)
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(new Project
                {
                    NoOfRounds = timeframe.NoOfRounds
                });
            }

            var projects = await _projectService.GetProjectsByTimeframeId(organizationId, timeframeId);
            if(projects.Count() >  0) 
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(projects.ElementAt(0));
            }

            _logger.LogInformation("Returning rounds creation view...");
            return View(new Project { NoOfRounds = 0 });
        }

        [HttpPost]
        public async Task<IActionResult> ProjectRoundCreate([Bind(nameof(Project.Id),nameof(Project.OrganizationId),nameof(Project.TimeframeId),nameof(Project.NoOfRounds))] Project project, 
            List<DateTime> RoundStartDates, List<DateTime> RoundEndDates)
        {
            _logger.LogInformation("Creating project rounds...");

            var rounds = await _roundService.GetFirstNRounds(project.NoOfRounds);
            var roundsList = rounds.ToList();

            if(roundsList.Count < project.NoOfRounds)
            {
                if(roundsList.Count == 0)
                {
                    for(int i = 1; i <= project.NoOfRounds; i++)
                    {
                        roundsList.Add(new Round
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _roundService.AddRange(roundsList);
                }
                else
                {
                    var noOfRoundsToMake = project.NoOfRounds - roundsList.Count;
                    var roundsToMake = new List<Round>();

                    for(int i = roundsList.Count + 1; i <= noOfRoundsToMake; i++)
                    {
                        roundsToMake.Add(new Round 
                        {
                            Name = $"Round {i}"
                        });
                    }

                    await _roundService.AddRange(roundsToMake);
                }
            }

            rounds = await _roundService.GetFirstNRounds(project.NoOfRounds);
            roundsList = rounds.ToList();
            if (roundsList.Count < project.NoOfRounds)
            {
                throw new Exception("There are not enough rounds in the database.");
            }

            var projects = await _projectService.GetProjectsByTimeframeId(project.OrganizationId.ToString(), project.TimeframeId);
            var projectsList = projects.ToList();

            if(projectsList.Count() == 0 || roundsList.Count() == 0)
            {
                throw new Exception("There are no projects or rounds to link.");
            }

            var projectRounds = new List<ProjectRound>();
            for(int i = 0; i < projectsList.Count; i++)
            {
                for(int j = 0; j < roundsList.Count; j++)
                {
                    var folderName = roundsList[j].Name;
                    var projectRoundFolderId = await _googleDriveService.CreateFolderAsync(folderName, projectsList[i].GDFolderId);

                    var projectRound = new ProjectRound
                    {
                        ProjectId = projectsList[i].Id,
                        RoundId = roundsList[j].Id,
                        GDFolderId = projectRoundFolderId,
                        ReleaseDate = RoundStartDates[j],
                        DueDate = RoundEndDates[j]
                    };

                    projectRounds.Add(projectRound);
                }
            }

            if(projectRounds.Count != projectsList.Count * roundsList.Count)
            {
                throw new Exception("Not every project has the correct amount of rounds.");
            }

            await _projectRoundService.AddRange(projectRounds);

            _logger.LogInformation("Returning to project selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = project.OrganizationId, timeframeId = project.TimeframeId });
        }

        public async Task<IActionResult> ProjectRoundsIndex(string projectId)
        {
            _logger.LogInformation("Moving to the rounds step...");
            var projectRounds = await _projectRoundService.GetProjectRoundsByProjectId(projectId);

            _logger.LogInformation("Returning rounds selection view...");
            return View(projectRounds);
        }
    }
}
