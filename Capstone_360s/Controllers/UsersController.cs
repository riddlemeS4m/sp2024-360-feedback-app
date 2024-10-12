using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Services.CSV;
using Capstone_360s.Services.FeedbackDb;
using Capstone_360s.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Capstone_360s.Controllers
{
    public class UsersController : Controller
    {
        private readonly CsvService _csvService;
        private readonly IGoogleDrive _googleDriveService;
        private readonly UserService _userService;
        private readonly ProjectService _projectService;
        private readonly TeamService _teamService;
        private readonly TimeframeService _timeframeService;
        private readonly MetricService _metricService;
        private readonly QuestionService _questionService;
        private readonly FeedbackService _feedbackService;
        private readonly MetricResponseService _metricResponseService;
        private readonly QuestionResponseService _questionResponseService;
        private readonly RoundService _roundService;
        private readonly ILogger<UsersController> _logger;
        public UsersController(CsvService csvService, 
            IGoogleDrive googleDriveService,
            UserService userService,
            ProjectService projectService,
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
            _csvService = csvService;
            _googleDriveService = googleDriveService;
            _userService = userService;
            _projectService = projectService;
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

            if(round == null)
            {
                return View();
            }

            var data = _csvService.ReadCapstoneSurveyResponses(roster, filterDate).ToList();

            if(data.Count() == 0)
            {
                _logger.LogInformation($"{data.Count} rows were read and mapped, returning to home screen...");
                return RedirectToAction(nameof(UploadProcessController.Index), "UploadProcess");
            }

            // check that all metrics are in the database, and add ones that aren't
            var metrics = await _metricService.GetMetricsByOrganizationId(organizationId);
            var metricsToAdd = _csvService.CapstoneMetrics.Where(x => !metrics.Any(y => y.OriginalMetricId == x));
            var defaultMetrics = _csvService.GetDefaultCapstoneMetrics(organizationId);

            if (metricsToAdd.Any())
            {
                foreach(string metric in metricsToAdd)
                {
                    await _metricService.AddAsync(defaultMetrics.Where(x => x.OriginalMetricId == metric).FirstOrDefault());
                }
            }

            defaultMetrics = await _metricService.GetDefaultCapstoneMetrics(organizationId, _csvService.CapstoneMetrics.ToList());

            metrics = await _metricService.GetMetricsByOrganizationId(organizationId);

            // check that all questions are in the database, and add ones that aren't
            var questions = await _questionService.GetQuestionsByOrganizationId(organizationId);
            var questionsToAdd = _csvService.CapstoneQuestions.Where(x => !questions.Any(y => y.OriginalQuestionId == x));
            var defaultQuestions = _csvService.GetDefaultCapstoneQuestions(organizationId);

            if (questionsToAdd.Any())
            {
                foreach(string question in questionsToAdd)
                {
                    await _questionService.AddAsync(defaultQuestions.Where(x => x.OriginalQuestionId == question).FirstOrDefault());
                }
            }

            defaultQuestions = await _questionService.GetDefaultCapstoneQuestions(organizationId, _csvService.CapstoneQuestions.Take(3).ToList());

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

            // finally, iterate through project.members map each teammember to a new feedback object
            projects = await _projectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);
            teamMembers = await _teamService.GetTeamMembersByListOfProjectIds(projects.Select(x => x.Id).ToList());

            var feedback = new List<Feedback>();
            var metricResponses = new List<MetricResponse>();
            var questionResponses = new List<QuestionResponse>();

            data.Sort((x, y) => x.TeamNumber.CompareTo(y.TeamNumber));

            for (int i = 0; i < projects.Count(); i++)
            {
                var project = projects.ElementAt(i);
                var rows = data.Where(x => x.TeamName == project.Name);
                if(rows.Count() != project.NoOfMembers)
                {
                    throw new Exception("The wrong number of projects is preventing the report from being run.");
                }

                for(int j = 0; j < project.NoOfMembers; j++)
                {
                    var row = rows.ElementAt(j);
                    var selfFeedback = new Feedback
                    {
                        ReviewerId = userDicts[row.Email],
                        RevieweeId = userNamesDict[row.FirstName.Trim() + " " + row.LastName.Trim()],
                        ProjectId = project.Id,
                        RoundId = roundId,
                        TimeframeId = timeframeId
                    };
                    feedback.Add(selfFeedback);

                    metricResponses.AddRange(GenerateAllMetricResponses(selfFeedback, defaultMetrics.ToList(),
                    [
                        row.TechnologySelf,
                        row.AnalyticalSelf,
                        row.CommunicationSelf,
                        row.ParticipationSelf,
                        row.PerformanceSelf
                    ]));

                    questionResponses.AddRange(GenerateAllQuestionResponses(selfFeedback, defaultQuestions.ToList(), 
                    [ 
                        row.StrengthsSelf,
                        row.GrowthAreasSelf,
                        row.CommentsSelf
                    ]));

                    if(!string.IsNullOrEmpty(row.Member1NameConfirmation.Trim()))
                    {
                        var feedbackMemberOne = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member1NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberOne);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberOne, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember1,
                            row.AnalyticalMember1,
                            row.CommunicationMember1,
                            row.ParticipationMember1,
                            row.PerformanceMember1
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberOne, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember1,
                            row.GrowthAreasMember1,
                            row.CommentsMember1
                        ]));
                    }                    

                    if(!string.IsNullOrEmpty(row.Member2NameConfirmation.Trim()))
                    {
                        var feedbackMemberTwo = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member2NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberTwo);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberTwo, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember2,
                            row.AnalyticalMember2,
                            row.CommunicationMember2,
                            row.ParticipationMember2,
                            row.PerformanceMember2
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberTwo, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember2,
                            row.GrowthAreasMember2,
                            row.CommentsMember2
                        ]));
                    }
                    
                    if(!string.IsNullOrEmpty(row.Member3NameConfirmation.Trim()))
                    {
                        var feedbackMemberThree = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member3NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberThree);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberThree, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember3,
                            row.AnalyticalMember3,
                            row.CommunicationMember3,
                            row.ParticipationMember3,
                            row.PerformanceMember3
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberThree, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember3,
                            row.GrowthAreasMember3,
                            row.CommentsMember3
                        ]));
                    }
                    
                    if(!string.IsNullOrEmpty(row.Member4NameConfirmation.Trim()))
                    {
                        var feedbackMemberFour = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member4NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberFour);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberFour, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember4,
                            row.AnalyticalMember4,
                            row.CommunicationMember4,
                            row.ParticipationMember4,
                            row.PerformanceMember4
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberFour, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember4,
                            row.GrowthAreasMember4,
                            row.CommentsMember4
                        ]));
                    }   
                    
                    if(!string.IsNullOrEmpty(row.Member5NameConfirmation.Trim()))
                    {
                        var feedbackMemberFive = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member5NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberFive);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberFive, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember5,
                            row.AnalyticalMember5,
                            row.CommunicationMember5,
                            row.ParticipationMember5,
                            row.PerformanceMember5
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberFive, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember5,
                            row.GrowthAreasMember5,
                            row.CommentsMember5
                        ]));
                    }

                    if (!string.IsNullOrEmpty(row.Member6NameConfirmation.Trim()))
                    {
                        var feedbackMemberSix = new Feedback
                        {
                            ReviewerId = userDicts[row.Email],
                            RevieweeId = userNamesDict[row.Member6NameConfirmation.Trim()],
                            ProjectId = project.Id,
                            RoundId = roundId,
                            TimeframeId = timeframeId
                        };
                        feedback.Add(feedbackMemberSix);

                        metricResponses.AddRange(GenerateAllMetricResponses(feedbackMemberSix, defaultMetrics.ToList(),
                        [
                            row.TechnologyMember6,
                            row.AnalyticalMember6,
                            row.CommunicationMember6,
                            row.ParticipationMember6,
                            row.PerformanceMember6
                        ]));

                        questionResponses.AddRange(GenerateAllQuestionResponses(feedbackMemberSix, defaultQuestions.ToList(),
                        [
                            row.StrengthsMember6,
                            row.GrowthAreasMember6,
                            row.CommentsMember6
                        ]));
                    }
                }
            }

            await _feedbackService.AddRange(feedback);
            await _metricResponseService.AddRange(metricResponses);
            await _questionResponseService.AddRange(questionResponses);

            _logger.LogInformation($"{data.Count} rows were read and mapped, returning to home screen...");
            return RedirectToAction(nameof(UploadProcessController.Index), "UploadProcess");
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
            for(int i = 0; i < questions.Count(); i++)
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
