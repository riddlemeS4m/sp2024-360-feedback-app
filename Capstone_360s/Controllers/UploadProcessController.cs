using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.VMs;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Services.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capstone_360s.Services.Configuration.Organizations;

namespace Capstone_360s.Controllers
{
    [Authorize(Policy = RoleManagerService.AdminOnlyPolicy)]
    [Route("{organizationId}/[controller]/[action]")]
    public class UploadProcessController : Controller
    {
        [FromRoute]
        public string OrganizationId { get; set; }
        public const string Name = "UploadProcess";
        private readonly IGoogleDrive _googleDriveService;
        private readonly FeedbackDbServiceFactory _serviceFactory;
        private readonly CapstoneOrganizationServices _capstoneServices;
        private readonly ILogger<UploadProcessController> _logger;
        public UploadProcessController(
            IGoogleDrive googleDriveService,
            FeedbackDbServiceFactory serviceFactory,
            CapstoneOrganizationServices capstoneServices,
            ILogger<UploadProcessController> logger) 
        { 
            _googleDriveService = googleDriveService;
            _serviceFactory = serviceFactory;
            _capstoneServices = capstoneServices;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Starting upload process...");

            var organizations = await _serviceFactory.OrganizationService.GetAllAsync();

            _logger.LogInformation("Returning organization selection view...");
            return View(organizations);
        }

        public async Task<IActionResult> TimeframesIndex()
        {
            _logger.LogInformation("Moving to the timeframes step...");
            var timeframes = await _serviceFactory.TimeframeService.GetTimeframesByOrganizationId(this.OrganizationId);

            _logger.LogInformation("Returning timeframes selection view...");
            return View(timeframes);
        }

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

            var organizationFolderId = (await _serviceFactory.OrganizationService.GetByIdAsync(timeframe.OrganizationId)).GDFolderId;
            timeframe.GDFolderId = await _googleDriveService.CreateFolderAsync(timeframe.Name, organizationFolderId);

            await _serviceFactory.TimeframeService.AddAsync(timeframe);

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

            await _serviceFactory.ProjectService.AddRange(projects);

            _logger.LogInformation("Returning next view, projects selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = timeframe.OrganizationId, timeframeId = timeframe.Id});
        }

        public async Task<IActionResult> ProjectsIndex(int timeframeId)
        {
            _logger.LogInformation("Moving to the projects step...");
            var projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId);

            _logger.LogInformation("Returning projects selection view...");
            return View(projects);
        }

        public async Task<IActionResult> ProjectRoundCreate(int timeframeId)
        {
            _logger.LogInformation("Project rounds need to be created...");
            var timeframe = await _serviceFactory.TimeframeService.GetByIdAsync(timeframeId);
            if(timeframe.NoOfRounds > 0)
            {
                _logger.LogInformation("Returning rounds creation view...");
                return View(new Project
                {
                    NoOfRounds = timeframe.NoOfRounds
                });
            }

            var projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(this.OrganizationId, timeframeId);
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

            var rounds = await _serviceFactory.RoundService.GetFirstNRounds(project.NoOfRounds);
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

                    await _serviceFactory.RoundService.AddRange(roundsList);
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

                    await _serviceFactory.RoundService.AddRange(roundsToMake);
                }
            }

            rounds = await _serviceFactory.RoundService.GetFirstNRounds(project.NoOfRounds);
            roundsList = rounds.ToList();
            if (roundsList.Count < project.NoOfRounds)
            {
                throw new Exception("There are not enough rounds in the database.");
            }

            var projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(project.OrganizationId.ToString(), project.TimeframeId);
            var projectsList = projects.ToList();

            if(projectsList.Count() == 0 || roundsList.Count() == 0)
            {
                throw new Exception("There are no projects or rounds to link.");
            }

            if(projectsList.Select(x => x.NoOfRounds).Distinct().Count() > 1)
            {
                throw new Exception("Not every project has the same number of rounds.");
            }

            if(projectsList.Select(x => x.NoOfRounds).Distinct().FirstOrDefault() != roundsList.Count)
            {
                throw new Exception("Not every project has the correct amount of rounds.");
            }

            var projectRounds = new List<ProjectRound>();
            for(int i = 0; i < projectsList.Count; i++)
            {
                for(int j = 0; j < roundsList.Count; j++)
                {
                    var folderName = roundsList[j].Name;
                    var projectRoundFolderId = await _googleDriveService.CreateFolderAsync(folderName, projectsList[i].GDFolderId);

                    DateTime? startDate;
                    DateTime? endDate;

                    if(RoundStartDates.Count == roundsList.Count)
                    {
                        startDate = RoundStartDates[j];
                    } else { startDate = null; }
                    
                    if(RoundEndDates.Count == roundsList.Count)
                    {
                        endDate = RoundEndDates[j];
                    } else { endDate = null; }

                    var projectRound = new ProjectRound
                    {
                        ProjectId = projectsList[i].Id,
                        RoundId = roundsList[j].Id,
                        GDFolderId = projectRoundFolderId,
                        ReleaseDate = startDate,
                        DueDate = endDate
                    };

                    projectRounds.Add(projectRound);
                }
            }

            if(projectRounds.Count != projectsList.Count * roundsList.Count)
            {
                throw new Exception("Not every project has the correct amount of rounds.");
            }

            await _serviceFactory.ProjectRoundService.AddRange(projectRounds);

            _logger.LogInformation("Returning to project selection view...");
            return RedirectToAction(nameof(ProjectsIndex), new { organizationId = project.OrganizationId, timeframeId = project.TimeframeId });
        }

        public async Task<IActionResult> ProjectRoundsIndex(int timeframeId, string projectId)
        {
            _logger.LogInformation("Moving to the rounds step...");
            var projectRounds = await _serviceFactory.ProjectRoundService.GetProjectRoundsByProjectId(projectId);

            var vm = new ProjectRoundsIndexVM
            {
                ProjectRounds = projectRounds,
                OrganizationId = this.OrganizationId,
                TimeframeId = timeframeId,
            };

            _logger.LogInformation("Returning rounds selection view...");
            return View(vm);
        }

        public async Task<IActionResult> FeedbackIndex(string projectId, int timeframeId, int roundId)
        {
            return RedirectToAction(nameof(CreatePdfs), new { timeframeId, roundId });
        }

        public async Task<IActionResult> CreatePdfs(int timeframeId, int roundId)
        {
            _logger.LogInformation("Moving to the create pdfs step...");

            if(timeframeId == 0 || roundId == 0)
            {
                return BadRequest();
            }

            var feedback = await _serviceFactory.FeedbackService.GetMultipleRoundsOfFeedbackByTimeframeIdAndRoundId(timeframeId, roundId);
            _logger.LogInformation($"{feedback.Count()} rows of feedback about to be mapped to {feedback.Select(x => x.RevieweeId).Distinct().Count()} recipients...");

            var noOfRoundsList = feedback.Select(x => new { x.ProjectId, x.Project.NoOfMembers }).Distinct().ToList();
            int sum = 0;

            if(!noOfRoundsList.Any())
            {
                throw new Exception("Feedback objects don't have any project information.");
            }

            foreach (var noOfRound in noOfRoundsList)
            {
                var number = noOfRound.NoOfMembers;
                sum += (number * number);
            }

            if(sum * roundId != feedback.Count())
            {
                throw new Exception("Unexpected number of feedback objects was returned");
            }

            var invertedQualtrics = await _capstoneServices.DataMap.MapFeedback(feedback, roundId);
            _logger.LogInformation($"{invertedQualtrics.Count()} feedback objects have now been mapped...");

            if(feedback.Count() != invertedQualtrics.Count())
            {
                throw new Exception("Not every feedback object has been mapped.");
            }

            var pdfs = await _capstoneServices.PdfService.GeneratePdfs(invertedQualtrics, roundId);
            _logger.LogInformation($"{pdfs.Count()} pdfs have been generated...");

            if(pdfs.Count() != feedback.Select(x => x.RevieweeId).Distinct().Count())
            {
                throw new Exception("Not every feedback recipient has a pdf.");
            }

            var projectIds = pdfs.Select(x => x.ProjectId).Distinct().ToList();
            var projectRounds = await _serviceFactory.ProjectRoundService.GetProjectRoundsByListOfProjectIdsAndRoundId(projectIds, roundId);
            var projectRoundsDict = projectRounds.ToDictionary(x => x.ProjectId, x => x.GDFolderId);
            for(int i = 0; i < pdfs.Count; i++)
            {
                pdfs[i].ParentGDFolderId = projectRoundsDict[pdfs[i].ProjectId];
                _logger.LogInformation($"Requesting to upload file {i + 1} of {pdfs.Count}...");
                var fileId = await _googleDriveService.UploadFile(pdfs[i].Data, pdfs[i].FileName, pdfs[i].ParentGDFolderId);
                pdfs[i].GDFileId = fileId;
            }

            var feedbackDict = new Dictionary<Guid, List<Feedback>>();
            for (int k = 0; k < feedback.Count(); k++)
            {
                var oneFeedback = feedback.ElementAt(k);

                if (!feedbackDict.TryGetValue(oneFeedback.RevieweeId, out List<Feedback> bogusList))
                {
                    feedbackDict[oneFeedback.RevieweeId] = new List<Feedback> { oneFeedback };
                }
                else
                {
                    feedbackDict[oneFeedback.RevieweeId].Add(oneFeedback);
                }
            }

            await _serviceFactory.FeedbackPdfService.AddRange(pdfs);

            // update feedback table to link to feedbackpdfs 
            var feedbackPdfs = await _serviceFactory.FeedbackPdfService.GetFeedbackPdfsByProjectIdsAndRoundId(projectIds, roundId);
            var feedbackPdfsDict = new Dictionary<Guid, Guid>();

            foreach(var feedbackPdf in feedbackPdfs)
            {
                if(!feedbackPdfsDict.TryGetValue(feedbackPdf.UserId, out Guid pdfId))
                {
                    feedbackPdfsDict[feedbackPdf.UserId] = feedbackPdf.Id;
                }
            }

            var updatedFeedbacks = new List<Feedback>();

            for (int j = 0; j < pdfs.Count(); j++)
            {
                var onePdf = pdfs.ElementAt(j);

                foreach(var oneFeedback in feedbackDict[onePdf.UserId])
                {
                    oneFeedback.FeedbackPdfId = feedbackPdfsDict[onePdf.UserId];
                    updatedFeedbacks.Add(oneFeedback);
                }
            }

            await _serviceFactory.FeedbackService.UpdateRangeAsync(updatedFeedbacks);

            return View(new Project { NoOfRounds = roundId});
        }

        public async Task<IActionResult> FeedbackPdfsIndex(int timeframeId, string projectId, int roundId)
        {
            var pdfs = await _serviceFactory.FeedbackPdfService.GetFeedbackByProjectIdAndRoundId(Guid.Parse(projectId), roundId);

            return View(pdfs);
        }
    }
}
