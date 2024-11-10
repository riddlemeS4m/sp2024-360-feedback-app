using Capstone_360s.Data.Constants;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.CapstoneRoster;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Services.CSV;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public class UsersController : Controller
    {
        private readonly CapstoneCsvService _capstoneCsvService;
        private readonly IGoogleDrive _googleDriveService;
        private readonly UserService _userService;
        private readonly ProjectService _projectService;
        private readonly ProjectRoundService _projectRoundService;
        private readonly TeamService _teamService;
        private readonly TimeframeService _timeframeService;
        private readonly MetricService _metricService;
        private readonly QuestionService _questionService;
        private readonly FeedbackService _feedbackService;
        private readonly MetricResponseService _metricResponseService;
        private readonly QuestionResponseService _questionResponseService;
        private readonly RoundService _roundService;
        private readonly ILogger<UsersController> _logger;
        public UsersController(CapstoneCsvService csvService, 
            IGoogleDrive googleDriveService,
            UserService userService,
            ProjectService projectService,
            ProjectRoundService projectRoundService,
            TeamService teamService,
            TimeframeService timeframeService,
            MetricService metricService,
            QuestionService questionService,
            FeedbackService feedbackService,
            MetricResponseService metricResponseService,
            QuestionResponseService questionResponseService,
            RoundService roundService,
            ILogger<UsersController> logger)
        {
            _capstoneCsvService = csvService;
            _googleDriveService = googleDriveService;
            _userService = userService;
            _projectService = projectService;
            _projectRoundService = projectRoundService;
            _teamService = teamService;
            _timeframeService = timeframeService;
            _metricService = metricService;
            _questionService = questionService;
            _feedbackService = feedbackService;
            _metricResponseService = metricResponseService;
            _questionResponseService = questionResponseService;
            _roundService = roundService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UploadCapstoneRoster(string organizationId, int timeframeId)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadCapstoneRoster(IFormFile roster, DateTime filterDate, int roundId)
        {
            var organizationId = Guid.Parse(Request.Query["organizationId"]);
            var timeframeId = int.Parse(Request.Query["timeframeId"]);
            var timeframe = await _timeframeService.GetByIdAsync(timeframeId);

            _logger.LogInformation("Uploading roster...");

            if (roster == null || filterDate == DateTime.MinValue || roundId == 0)
            {
                return View();
            }

            var round = await _roundService.GetByIdAsync(roundId);

            if(round == null || roundId > timeframe.NoOfRounds)
            {
                return View();
            }

            // filter date parameter not working quite right yet
            var data = _capstoneCsvService.ReadSurveyResponsesWithFilterDate(roster, Capstone.ExpectedHeaders, nameof(Qualtrics.StartDate), filterDate, Capstone.CustomDateFilter).ToList();

            if(data.Count == 0)
            {
                _logger.LogInformation($"{data.Count} rows were read and mapped, returning to home screen...");
                return RedirectToAction(nameof(UploadProcessController.Index), "UploadProcess");
            }

            // check that all metrics are in the database, and add ones that aren't
            var metrics = await _metricService.GetMetricsByOrganizationId(organizationId);
            var metricsToAdd = Capstone.CapstoneMetrics.Where(x => !metrics.Any(y => y.OriginalMetricId == x));
            var defaultMetrics = Capstone.GetDefaultCapstoneMetrics(organizationId);

            if (metricsToAdd.Any())
            {
                foreach(string metric in metricsToAdd)
                {
                    await _metricService.AddAsync(defaultMetrics.Where(x => x.OriginalMetricId == metric).FirstOrDefault());
                }
            }

            defaultMetrics = await _metricService.GetDefaultCapstoneMetrics(organizationId, Capstone.CapstoneMetrics.ToList());

            metrics = await _metricService.GetMetricsByOrganizationId(organizationId);

            // check that all questions are in the database, and add ones that aren't
            var questions = await _questionService.GetQuestionsByOrganizationId(organizationId);
            var questionsToAdd = Capstone.CapstoneQuestions.Where(x => !questions.Any(y => y.OriginalQuestionId == x));
            var defaultQuestions = Capstone.GetDefaultCapstoneQuestions(organizationId);

            if (questionsToAdd.Any())
            {
                foreach(string question in questionsToAdd)
                {
                    await _questionService.AddAsync(defaultQuestions.Where(x => x.OriginalQuestionId == question).FirstOrDefault());
                }
            }

            defaultQuestions = await _questionService.GetDefaultCapstoneQuestions(organizationId, Capstone.CapstoneQuestions.Take(3).ToList());

            // check that all users in the roster are in the database, and add ones that aren't
            var users = await _userService.GetUsersByOrganizationId(organizationId);
            var usersToAdd = data.Where(x => !users.Any(u => u.Email == x.Email)).ToList();
            if (usersToAdd.Count != 0)
            {
                var usersList = new List<User>();
                foreach(var user in usersToAdd)
                {
                    usersList.Add(new User
                    {
                        FirstName = user.FirstName.Trim(),
                        LastName = user.LastName.Trim(),
                        Email = user.Email.Trim(),
                        OrganizationId = organizationId
                    });
                }
                await _userService.AddRange(usersList);
            }

            users = await _userService.GetUsersByOrganizationId(organizationId);

            // check that all projects in the roster are in the database, and add ones that aren't
            var projects = await _projectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);
            var projectsToAdd = data.Where(x => !projects.Any(p => p.Name.Trim().Contains(x.TeamName.Trim(), StringComparison.CurrentCultureIgnoreCase)))
                .Select(x => x.TeamName)
                .Distinct() 
                .ToList();

            if(projectsToAdd.Count != 0)
            {
                var projectsList = new List<Project>();
                foreach(var projectName in projectsToAdd)
                {
                    projectsList.Add(new Project
                    {
                        Name = projectName.Trim(),
                        OrganizationId = organizationId,
                        TimeframeId = timeframeId,
                        Description = "This project was auto-generated by a roster upload",
                        GDFolderId = await _googleDriveService.CreateFolderAsync(projectName, timeframe.GDFolderId),
                        NoOfMembers = data.Where(x => x.TeamName == projectName).Count(),
                        NoOfRounds = timeframe.NoOfRounds
                    });
                }

                await _projectService.AddRange(projectsList);
            }

            projects = await _projectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);

            // check if team members are assigned to each user, otherwise add team members to projects
            var teamMembers = await _teamService.GetTeamMembersByListOfProjectIds(projects.Select(x => x.Id).ToList());
            var teamMembersDict = teamMembers.ToDictionary(x => x.User.Email, x => x.Project.Name);
            var dataRolesDict = data.ToDictionary(x => x.Email, x => x.TeamName.Trim().ToLower());
            var projectsDict = projects.ToDictionary(x => x.Name.Trim().ToLower(), x => x.Id);
            var userDicts = users.ToDictionary(x => x.Email, x => x.Id);
            var userNamesDict = users.ToDictionary(x => x.FirstName + " " + x.LastName, x => x.Id);

            var teamMembersToAdd = new List<TeamMember>();
            foreach(var role in dataRolesDict)
            {
                if(!teamMembersDict.TryGetValue(role.Key, out string existingProjectName))
                {
                    teamMembersToAdd.Add(new TeamMember
                    {
                        ProjectId = projectsDict[dataRolesDict[role.Key]],
                        UserId = userDicts[role.Key]
                    });
                }
            }

            await _teamService.AddRange(teamMembersToAdd);

            // check that each project has a child folder for each round, otherwise create child folder and project round object
            var projectRounds = await _projectRoundService.GetProjectRoundsByListOfProjectIdsAndRoundId(projects.Select(x => x.Id).ToList(), roundId);
            var projectRoundsDict = projectRounds.ToDictionary(x => x.ProjectId, x => x.RoundId);
            var projectRoundsToAdd = new List<ProjectRound>();

            foreach(var project in projects)
            {
                if(!projectRoundsDict.ContainsKey(project.Id))
                {
                    var roundFolderId = await _googleDriveService.CreateFolderAsync("Round " + roundId, project.GDFolderId);
                    projectRoundsToAdd.Add(new ProjectRound
                    {
                        ProjectId = project.Id,
                        RoundId = roundId,
                        GDFolderId = roundFolderId
                    });
                }
            }

            await _projectRoundService.AddRange(projectRoundsToAdd);

            // finally, iterate through project.members map each teammember to a new feedback object
            projects = await _projectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);
            teamMembers = await _teamService.GetTeamMembersByListOfProjectIds(projects.Select(x => x.Id).ToList());
            //projectRounds = await _projectRoundService.GetProjectRoundsByListOfProjectIdsAndRoundId(projects.Select(x => x.Id).ToList(), roundId);

            var feedback = new List<Feedback>();
            var metricResponses = new List<MetricResponse>();
            var questionResponses = new List<QuestionResponse>();

            data.Sort((x, y) => x.TeamNumber.CompareTo(y.TeamNumber));

            for (int i = 0; i < projects.Count(); i++)
            {
                var project = projects.ElementAt(i);
                var rows = data.Where(x => x.TeamName == project.Name);
                
                // work on this...this is where the program decides what to do if not everybody has submitted
                if(rows == null)
                {
                    throw new Exception("The wrong number of projects is preventing the report from being run.");
                    
                    for (int o = 0; o < project.NoOfMembers; i++)
                    {
                        rows = rows.Append(new Qualtrics());
                    }
                }

                if(rows.Count() != project.NoOfMembers)
                {
                    throw new Exception("The wrong number of projects is preventing the report from being run.");

                    for (int o = 0; o < project.NoOfMembers - rows.Count(); i++)
                    {
                        rows = rows.Append(new Qualtrics());
                    }
                }

                for (int j = 0; j < project.NoOfMembers; j++)
                {
                    var row = rows.ElementAt(j);

                    // Self Feedback
                    var selfFeedback = CreateFeedback(userDicts[row.Email], userNamesDict[row.FirstName.Trim() + " " + row.LastName.Trim()], project.Id, roundId, timeframeId, row.ResponseId);
                    feedback.Add(selfFeedback);

                    AddResponses(selfFeedback,
                    [
                        row.TechnologySelf,
                        row.AnalyticalSelf,
                        row.CommunicationSelf,
                        row.ParticipationSelf,
                        row.PerformanceSelf
                    ],
                                    [
                        row.StrengthsSelf,
                        row.GrowthAreasSelf,
                        row.CommentsSelf
                    ]);

                    // Loop through members 1 to 6
                    for (int memberIndex = 1; memberIndex < project.NoOfMembers; memberIndex++)
                    {
                        var memberName = (string)typeof(Qualtrics).GetProperty($"Member{memberIndex}NameConfirmation")?.GetValue(row, null);
                        if (!string.IsNullOrEmpty(memberName?.Trim()))
                        {
                            var feedbackMember = CreateFeedback(userDicts[row.Email], userNamesDict[memberName.Trim()], project.Id, roundId, timeframeId, row.ResponseId);
                            feedback.Add(feedbackMember);

                            AddResponses(feedbackMember, new[]
                            {
                                (string)typeof(Qualtrics).GetProperty($"TechnologyMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"AnalyticalMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"CommunicationMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"ParticipationMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"PerformanceMember{memberIndex}")?.GetValue(row, null)
                            }, new[]
                                            {
                                (string)typeof(Qualtrics).GetProperty($"StrengthsMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"GrowthAreasMember{memberIndex}")?.GetValue(row, null),
                                (string)typeof(Qualtrics).GetProperty($"CommentsMember{memberIndex}")?.GetValue(row, null)
                            });
                        }
                    }
                }

                // Helper method to create feedback
                Feedback CreateFeedback(Guid reviewerId, Guid revieweeId, Guid projectId, int roundId, int timeframeId, string originalResponseId)
                {
                    return new Feedback
                    {
                        ReviewerId = reviewerId,
                        RevieweeId = revieweeId,
                        ProjectId = projectId,
                        RoundId = roundId,
                        TimeframeId = timeframeId,
                        OriginalResponseId = originalResponseId
                    };
                }

                // Helper method to add metric and question responses
                void AddResponses(Feedback feedback, string[] metricValues, string[] questionValues)
                {
                    metricResponses.AddRange(GenerateAllMetricResponses(feedback, defaultMetrics.ToList(), metricValues.ToList()));
                    questionResponses.AddRange(GenerateAllQuestionResponses(feedback, defaultQuestions.ToList(), questionValues.ToList()));
                }

            }

            await _feedbackService.AddRange(feedback);
            await _metricResponseService.AddRange(metricResponses);
            await _questionResponseService.AddRange(questionResponses);

            _logger.LogInformation($"{data.Count} rows were read and mapped, returning to home screen...");
            //return RedirectToAction(nameof(UploadProcessController.ProjectRoundCreate), "UploadProcess", new { organizationId = organizationId, timeframeId = timeframeId });
            return RedirectToAction(nameof(UploadProcessController.CreatePdfs), "UploadProcess", new { timeframeId = timeframeId, roundId = roundId });
        }

        public async Task<IActionResult> OrganizationUsersIndex(string organizationId)
        {
            return View();
        }

        public async Task<IActionResult> POCAssignment(string organizationId)
        {
            return View();
        }

        public async Task<IActionResult> TeamAssignments(string organizationId, int timeframeId)
        {
            return View();
        }

        private List<QuestionResponse> GenerateAllQuestionResponses(Feedback feedback, List<Question> questions, List<string> responses)
        {
            var list = new List<QuestionResponse>();
            for(int i = 0; i < questions.Count; i++)
            {
                list.Add(GenerateQuestionResponse(feedback, questions[i].Id, responses[i]));
            }
            return list;
        }

        private QuestionResponse GenerateQuestionResponse(Feedback feedback, int questionId, string response)
        {
            return new QuestionResponse()
            {
                Feedback = feedback,
                QuestionId = questionId,
                Response = response
            };
        }

        private List<MetricResponse> GenerateAllMetricResponses(Feedback feedback, List<Metric> metrics, List<string> responses)
        {
            var list = new List<MetricResponse>();
            for(int i = 0; i < metrics.Count; i++)
            {
                list.Add(GenerateMetricResponse(feedback, metrics[i].Id, responses[i]));
            };
            return list;
        }

        private MetricResponse GenerateMetricResponse(Feedback feedback, int metricId, string response)
        {
            return new MetricResponse()
            {
                Feedback = feedback,
                MetricId = metricId,
                Response = QualtricsStringToNumber.Convert(response)
            };
        }
    }
}
