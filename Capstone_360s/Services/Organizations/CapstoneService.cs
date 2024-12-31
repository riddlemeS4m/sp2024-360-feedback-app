using System.Globalization;
using Capstone_360s.Data.Constants;
using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Organizations.Capstone;
using Capstone_360s.Utilities;
using Capstone_360s.Utilities.Maps;
using CsvHelper;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Capstone_360s.Services.Organizations
{
    public class CapstoneService
    {
        private readonly IFeedbackDbServiceFactory _serviceFactory;
        private readonly IGoogleDrive _driveService;
        private ILogger<CapstoneService> _logger;
        private delegate bool FilterDateFunc(string datefield, DateTime filterDate);
        private delegate void WritePdfContent(Document document, DocumentToPrint documentToPrint);

        public CapstoneService(IFeedbackDbServiceFactory serviceFactory,
            IGoogleDrive driveService,
            ILogger<CapstoneService> logger)
        {
            _serviceFactory = serviceFactory;
            _driveService = driveService;
            _logger = logger;
        }

        public async Task UploadRoster(IFormFile roster, DateTime filterDate, int roundId, int timeframeId, Guid organizationId)
        {
            var timeframe = await _serviceFactory.TimeframeService.GetByIdAsync(timeframeId);

            _logger.LogInformation("Uploading roster...");

            if (roster == null || filterDate == DateTime.MinValue || roundId == 0)
            {
                throw new ArgumentNullException("One of the following was empty: roster, filterDate, roundId");
            }

            var round = await _serviceFactory.RoundService.GetByIdAsync(roundId);

            if(round == null || roundId > timeframe.NoOfRounds)
            {
                throw new ArgumentNullException("Requested round not found.");
            }

            // filter date parameter not working quite right yet (?)
            var data = ReadSurveyResponsesWithFilterDate(roster, Capstone.ExpectedHeaders, nameof(Qualtrics.StartDate), filterDate, Capstone.CustomDateFilter).ToList();

            if(data.Count == 0)
            {
                //_logger.LogInformation($"{data.Count} rows were read and mapped, returning to home screen...");
                throw new InvalidOperationException("No data was processed. Is your filter date correct?");
            }

            // check that all metrics are in the database, and add ones that aren't
            var metrics = await _serviceFactory.MetricService.GetMetricsByOrganizationId(organizationId);
            var metricsToAdd = Capstone.CapstoneMetrics.Where(x => !metrics.Any(y => y.OriginalMetricId == x));
            var defaultMetrics = Capstone.GetDefaultCapstoneMetrics(organizationId);

            if (metricsToAdd.Any())
            {
                foreach(string metric in metricsToAdd)
                {
                    await _serviceFactory.MetricService.AddAsync(defaultMetrics.Where(x => x.OriginalMetricId == metric).FirstOrDefault());
                }
            }

            defaultMetrics = await _serviceFactory.MetricService.GetDefaultCapstoneMetrics(organizationId, Capstone.CapstoneMetrics.ToList());

            metrics = await _serviceFactory.MetricService.GetMetricsByOrganizationId(organizationId);

            // check that all questions are in the database, and add ones that aren't
            var questions = await _serviceFactory.QuestionService.GetQuestionsByOrganizationId(organizationId);
            var questionsToAdd = Capstone.CapstoneQuestions.Where(x => !questions.Any(y => y.OriginalQuestionId == x));
            var defaultQuestions = Capstone.GetDefaultCapstoneQuestions(organizationId);

            if (questionsToAdd.Any())
            {
                foreach(string question in questionsToAdd)
                {
                    await _serviceFactory.QuestionService.AddAsync(defaultQuestions.Where(x => x.OriginalQuestionId == question).FirstOrDefault());
                }
            }

            defaultQuestions = await _serviceFactory.QuestionService.GetDefaultCapstoneQuestions(organizationId, Capstone.CapstoneQuestions.Take(3).ToList());

            // check that all users in the roster are in the database, and add ones that aren't
            var submittedEmails = data.Select(x => x.Email).ToList();
            var users = (await _serviceFactory.UserService.GetUsersByListOfEmails(submittedEmails)).ToList();
            
            var usersToAdd = data.Where(x => !users.Any(u => u.Email == x.Email)).ToList();
            if (usersToAdd.Count != 0)
            {
                var usersList = new List<Capstone_360s.Models.FeedbackDb.User>();
                var userOrganizationsList = new List<UserOrganization>();
                foreach(var user in usersToAdd)
                {
                    usersList.Add(new Capstone_360s.Models.FeedbackDb.User
                    {
                        FirstName = user.FirstName.Trim(),
                        LastName = user.LastName.Trim(),
                        Email = user.Email.Trim(),
                        //OrganizationId = organizationId
                    });
                }

                // should probably make sure each user gets their microsoft id here too
                await _serviceFactory.UserService.AddRange(usersList);
            }

            users = (await _serviceFactory.UserService.GetUsersByListOfEmails(submittedEmails)).ToList();

            // add each user to the class/organization
            var userOrganizations = await _serviceFactory.UserOrganizationService.GetUsersByOrganizationId(organizationId);
            var missingUserOrgs = users.Where(x => !userOrganizations.Any(u => u.UserId == x.Id)).ToList();
            var userOrgsToAdd = new List<UserOrganization>();

            if(missingUserOrgs.Count > 0)
            {
                foreach(var uo in missingUserOrgs)
                {
                    userOrgsToAdd.Add(new UserOrganization {
                        UserId = uo.Id,
                        OrganizationId = organizationId,
                        AddedDate = DateTime.Now
                    });
                }

                await _serviceFactory.UserOrganizationService.AddRange(userOrgsToAdd);
            }

            // add each user to a semester's class roster/timeframe
            var userTimeframes = await _serviceFactory.UserTimeframeService.GetUsersByTimeframeId(timeframeId);
            var missingUserTimeframes = users.Where(x => !userTimeframes.Any(u => u.UserId == x.Id)).ToList();
            var userTimeframesToAdd = new List<UserTimeframe>();

            if(missingUserTimeframes.Count > 0)
            {
                foreach(var ut in missingUserTimeframes)
                {
                    userTimeframesToAdd.Add(new UserTimeframe {
                        UserId = ut.Id,
                        TimeframeId = timeframeId,
                        AddedDate = DateTime.Now
                    });
                }

                await _serviceFactory.UserTimeframeService.AddRange(userTimeframesToAdd);
            }

            // check that all projects in the roster are in the database, and add ones that aren't
            var projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);
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
                        GDFolderId = await _driveService.CreateFolderAsync(projectName, timeframe.GDFolderId),
                        NoOfMembers = data.Where(x => x.TeamName == projectName).Count(),
                        NoOfRounds = timeframe.NoOfRounds
                    });
                }

                await _serviceFactory.ProjectService.AddRange(projectsList);
            }

            projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);

            // check if team members are assigned to each user, otherwise add team members to projects
            var teamMembers = await _serviceFactory.TeamService.GetTeamMembersByListOfProjectIds(projects.Select(x => x.Id).ToList());
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

            await _serviceFactory.TeamService.AddRange(teamMembersToAdd);

            // check that each project has a child folder for each round, otherwise create child folder and project round object
            var projectRounds = await _serviceFactory.ProjectRoundService.GetProjectRoundsByListOfProjectIdsAndRoundId(projects.Select(x => x.Id).ToList(), roundId);
            var projectRoundsDict = projectRounds.ToDictionary(x => x.ProjectId, x => x.RoundId);
            var projectRoundsToAdd = new List<ProjectRound>();

            foreach(var project in projects)
            {
                if(!projectRoundsDict.ContainsKey(project.Id))
                {
                    var roundFolderId = await _driveService.CreateFolderAsync("Round " + roundId, project.GDFolderId);
                    projectRoundsToAdd.Add(new ProjectRound
                    {
                        ProjectId = project.Id,
                        RoundId = roundId,
                        GDFolderId = roundFolderId
                    });
                }
            }

            await _serviceFactory.ProjectRoundService.AddRange(projectRoundsToAdd);

            // finally, iterate through project.members map each teammember to a new feedback object
            projects = await _serviceFactory.ProjectService.GetProjectsByTimeframeId(organizationId.ToString(), timeframeId);
            teamMembers = await _serviceFactory.TeamService.GetTeamMembersByListOfProjectIds(projects.Select(x => x.Id).ToList());
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
                    var row = (Qualtrics)rows.ElementAt(j);

                    // Self Feedback
                    var selfFeedback = CreateFeedback(userDicts[row.Email], userNamesDict[row.FirstName.Trim() + " " + row.LastName.Trim()], project.Id, roundId, timeframeId, row.ResponseId, row.StartDate, row.EndDate);
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
                        var memberName = (string?)typeof(Qualtrics).GetProperty($"Member{memberIndex}NameConfirmation")?.GetValue(row, null)!;
                        if (!string.IsNullOrEmpty(memberName?.Trim()))
                        {
                            var feedbackMember = CreateFeedback(userDicts[row.Email], userNamesDict[memberName.Trim()], project.Id, roundId, timeframeId, row.ResponseId, row.StartDate, row.EndDate);
                            feedback.Add(feedbackMember);

                            // might need to take out exclamation marks and question marks 
                            AddResponses(feedbackMember, new[]
                            {
                                (string?)typeof(Qualtrics).GetProperty($"TechnologyMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"AnalyticalMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"CommunicationMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"ParticipationMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"PerformanceMember{memberIndex}")?.GetValue(row, null)!
                            }, new[]
                                            {
                                (string?)typeof(Qualtrics).GetProperty($"StrengthsMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"GrowthAreasMember{memberIndex}")?.GetValue(row, null)!,
                                (string?)typeof(Qualtrics).GetProperty($"CommentsMember{memberIndex}")?.GetValue(row, null)!
                            });
                        }
                    }
                }

                // Helper method to create feedback
                Feedback CreateFeedback(Guid reviewerId, Guid revieweeId, Guid projectId, int roundId, int timeframeId, string originalResponseId, DateTime? startTime, DateTime? endTime)
                {
                    TimeSpan difference = new(0,0,0);
                    if(endTime != null && startTime != null){
                        difference = (DateTime)endTime - (DateTime)startTime;
                    }

                    return new Feedback
                    {
                        ReviewerId = reviewerId,
                        RevieweeId = revieweeId,
                        ProjectId = projectId,
                        RoundId = roundId,
                        TimeframeId = timeframeId,
                        OriginalResponseId = originalResponseId,
                        StartTime = startTime,
                        EndTime = endTime,
                        DurationSeconds = (int)difference.TotalSeconds
                    };
                }

                // Helper method to add metric and question responses
                void AddResponses(Feedback feedback, string[] metricValues, string[] questionValues)
                {
                    metricResponses.AddRange(GenerateAllMetricResponses(feedback, defaultMetrics.ToList(), metricValues.ToList()));
                    questionResponses.AddRange(GenerateAllQuestionResponses(feedback, defaultQuestions.ToList(), questionValues.ToList()));
                }

            }

            await _serviceFactory.FeedbackService.AddRange(feedback);
            await _serviceFactory.MetricResponseService.AddRange(metricResponses);
            await _serviceFactory.QuestionResponseService.AddRange(questionResponses);

            _logger.LogInformation($"{data.Count} rows were read and mapped.");
        }

        private static List<QuestionResponse> GenerateAllQuestionResponses(Feedback feedback, List<Question> questions, List<string> responses)
        {
            var list = new List<QuestionResponse>();
            for(int i = 0; i < questions.Count; i++)
            {
                list.Add(GenerateQuestionResponse(feedback, questions[i].Id, responses[i]));
            }
            return list;
        }

        private static QuestionResponse GenerateQuestionResponse(Feedback feedback, int questionId, string response)
        {
            return new QuestionResponse()
            {
                Feedback = feedback,
                QuestionId = questionId,
                Response = response
            };
        }

        private static List<MetricResponse> GenerateAllMetricResponses(Feedback feedback, List<Metric> metrics, List<string> responses)
        {
            var list = new List<MetricResponse>();
            for(int i = 0; i < metrics.Count; i++)
            {
                list.Add(GenerateMetricResponse(feedback, metrics[i].Id, responses[i]));
            };
            return list;
        }

        private static MetricResponse GenerateMetricResponse(Feedback feedback, int metricId, string response)
        {
            return new MetricResponse()
            {
                Feedback = feedback,
                MetricId = metricId,
                Response = QualtricsStringToNumber.Convert(response)
            };
        }

        private static List<Qualtrics> ReadSurveyResponsesWithFilterDate(IFormFile file, IEnumerable<string> expectedHeaders, string dateField, DateTime filterDate,
            FilterDateFunc filterDateFunc)
        {
            // Check if the file is null or empty
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("CSV file is required and cannot be empty.");
            }

            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            // Read the header row
            csv.Read();
            csv.ReadHeader();

            var headers = csv.HeaderRecord;

            // Verify that the headers match
            var missingHeaders = expectedHeaders.Except(headers).ToList();
            if (missingHeaders.Count != 0)
            {
                throw new Exception($"The following expected headers are missing: {string.Join(", ", missingHeaders)}");
            }

            // Register the class map and read the records
            csv.Context.RegisterClassMap(new CapstoneMapCsvToQualtrics());

            // Manually read records and filter rows before mapping
            var validRecords = new List<Qualtrics>();
            while (csv.Read())
            {
                // Use the custom date filter logic (abstracted via delegate)
                string startDateField = csv.GetField(dateField); // Assuming StartDate field is in the CSV
                if (filterDateFunc(startDateField, filterDate))
                {
                    // Map the record if it passes the filter
                    var record = csv.GetRecord<Qualtrics>();
                    validRecords.Add(record);
                }
            }

            return validRecords;
        }

        public async Task CreatePdfs(Guid organizationId, int timeframeId, int roundId)
        {
            _logger.LogInformation("Moving to the create pdfs step...");

            if(timeframeId == 0 || roundId == 0)
            {
                throw new Exception("Timeframe or round was not specified.");
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

            var invertedQualtrics = await MapFeedback(feedback, roundId);
            _logger.LogInformation($"{invertedQualtrics.Count()} feedback objects have now been mapped...");

            if(feedback.Count() != invertedQualtrics.Count())
            {
                throw new Exception("Not every feedback object has been mapped.");
            }

            var pdfs = await GeneratePdfs(invertedQualtrics, roundId);
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
                var fileId = await _driveService.UploadFile(pdfs[i].Data, pdfs[i].FileName, pdfs[i].ParentGDFolderId);
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
        }

        private async Task<IEnumerable<InvertedQualtrics>> MapFeedback(IEnumerable<Feedback> feedback, int noOfRounds)
        {
            if(feedback.Count() == 0)
            {
                throw new ArgumentException("No feedback to map");
            }

            if(noOfRounds == 0)
            {
                throw new ArgumentException("No rounds to map");
            }

            var users = feedback.Select(x => x.RevieweeId).Distinct().ToList();
            var feedbackIds = feedback.Select(x => x.Id).ToList();

            var project = await _serviceFactory.ProjectService.GetByIdAsync(feedback.ElementAt(0).ProjectId);
            var projectsDict = feedback.Select(x => ( x.Id, x.Project )).Distinct().ToDictionary();

            var organization = await _serviceFactory.OrganizationService.GetByIdAsync(project.OrganizationId);
            var timeframe = await _serviceFactory.TimeframeService.GetByIdAsync(project.TimeframeId);

            var usersInfoUO = await _serviceFactory.UserOrganizationService.GetUsersByOrganizationId(organization.Id);
            var usersInfo = usersInfoUO.Select(x => x.User).ToList();

            var feedbackMetricsDict = await _serviceFactory.MetricResponseService.GetMetricResponsesDictByFeedbackIds(feedbackIds);
            var feedbackQuestionsDict = await _serviceFactory.QuestionResponseService.GetQuestionResponsesDictByFeedbackIds(feedbackIds);

            var list = new List<InvertedQualtrics>();


            for (int i = 0; i < noOfRounds; i++)
            {
                var roundFeedback = feedback.Where(x => x.RoundId == i + 1).ToList();

                if(roundFeedback.Count == 0)
                {
                    break;
                }

                var round = roundFeedback[0].Round;

                foreach (var reviewee in users)
                {
                    var revieweeInfo = usersInfo.Where(x => x.Id == reviewee).FirstOrDefault();
                    var revieweeFeedback = roundFeedback.Where(x => x.RevieweeId == reviewee).ToList();

                    foreach (var oneReceivedFeedback in revieweeFeedback)
                    {
                        var metricResponses = feedbackMetricsDict[oneReceivedFeedback.Id];
                        var questionResponses = feedbackQuestionsDict[oneReceivedFeedback.Id];

                        metricResponses = metricResponses.OrderBy(x => x.MetricId);
                        questionResponses = questionResponses.OrderBy(x => x.QuestionId);
                        
                        var ratings = new Dictionary<string, int>();
                        var questions = new Dictionary<string, string>();

                        if (!metricResponses.Any() || !questionResponses.Any())
                        {
                            throw new ArgumentException($"No metric responses or question responses found for feedback id {oneReceivedFeedback.Id}");
                        }

                        for (int j = 0; j < Capstone.MetricKeys.Count; j++)
                        {
                            ratings.Add(Capstone.MetricKeys[j], metricResponses.ElementAt(j).Response);
                        }

                        if(ratings.Count != metricResponses.Count())
                        {
                            throw new ArgumentException($"Mismatch between number of ratings items and number of metric responses for feedback id {oneReceivedFeedback.Id}");
                        }

                        for(int k = 0; k < Capstone.QuestionKeys.Count; k++)
                        {
                            questions.Add(Capstone.QuestionKeys[k], questionResponses.ElementAt(k).Response);
                        }

                        if(questions.Count != questionResponses.Count())
                        {
                            throw new ArgumentException($"Mismatch between number of questions items and number of question responses for feedback id {oneReceivedFeedback.Id}");
                        }

                        list.Add(new InvertedQualtrics
                        {
                            Organization = organization,
                            Timeframe = timeframe,
                            Project = projectsDict[oneReceivedFeedback.Id],
                            Round = round,
                            FirstName = revieweeInfo.FirstName,
                            LastName = revieweeInfo.LastName,
                            Email = revieweeInfo.Email,
                            Feedback = revieweeFeedback,
                            Ratings = ratings,
                            Questions = questions,
                            ReviewerEmail = oneReceivedFeedback.Reviewer.Email
                        });
                    }
                }
            }

            return list;
        }

        private async Task<List<FeedbackPdf>> GeneratePdfs(IEnumerable<InvertedQualtrics> invertedQualtrics, int currentRoundId)
        {
            ArgumentNullException.ThrowIfNull(invertedQualtrics);

            if(currentRoundId == 0)
            {
                throw new Exception($"'{nameof(currentRoundId)}' cannot be empty.");
            }

            var filesToReturn = new List<FeedbackPdf>();
            var documentsToReturn = new List<byte[]>();

            var documentsToPrint = await MapInvertedQualtricsToDocuments(invertedQualtrics, currentRoundId);
            var documentsToPrintList = documentsToPrint.OrderBy(x => x.Email).ToList();

            var userEmails = documentsToPrint.Select(x => x.Email).ToList();
            var users = await _serviceFactory.UserService.GetUsersByListOfEmails(userEmails);
            var usersList = users.OrderBy(x => x.Email).ToList();

            if(usersList.Count != documentsToPrintList.Count)
            {
                throw new Exception("Not every document has a valid user.");
            }
            
            for ( int i = 0; i < documentsToPrintList.Count; i++)
            {
                filesToReturn.Add(new FeedbackPdf()
                {
                    UserId = usersList[i].Id,
                    User = usersList[i],
                    ProjectId = documentsToPrintList[i].ProjectId,
                    RoundId = documentsToPrintList[i].RoundNumber,
                    FileName = documentsToPrintList[i].FullName,
                    Data = await WritePdfAsync(IndividualCapstonePdf, documentsToPrintList[i])
                });
            }   
            
            return filesToReturn;
        }

        private async Task<IEnumerable<DocumentToPrint>> MapInvertedQualtricsToDocuments(IEnumerable<InvertedQualtrics> feedbackList, int currentRoundId)
        {
            ArgumentNullException.ThrowIfNull(feedbackList);

            if (currentRoundId == 0)
            {
                throw new Exception("Current round id cannot be 0.");
            }

            var list = new List<DocumentToPrint>();

            // Create dictionary to store feedback for each round
            var dict = new Dictionary<int, List<InvertedQualtrics>>();
            for (int i = currentRoundId; i > 0; i--)
            {
                var roundFeedback = feedbackList.Where(x => x.Round.Id == i).ToList() ?? new List<InvertedQualtrics>();
                dict.Add(i, roundFeedback);
            }

            var projectIds = dict[currentRoundId].Select(x => x.Project.Id).ToList();
            // will need to ensure round folders are created before doing this
            // var projectRoundFolderIds = _projectRoundService.GetByProjectIdsAndRoundId(projectIds, roundId)

            // If feedback for a round is not found, get feedback from the database
            for (int i = currentRoundId; i > 0; i--)
            {
                if (!dict.TryGetValue(i, out List<InvertedQualtrics> bogusList))
                {
                    var oldRoundFeedback = await _serviceFactory.FeedbackService.GetFeedbackByTimeframeIdAndRoundId(feedbackList.ElementAt(0).Timeframe.Id, i);
                    var oldRoundFeedbackList = oldRoundFeedback.ToList();

                    if (oldRoundFeedbackList.Count == 0)
                    {
                        throw new Exception($"Was unable to get feedback for round {i}");
                    }

                    var oldRoundInvertedQualtrics = await MapFeedback(oldRoundFeedback, i);

                    dict.Add(i, oldRoundInvertedQualtrics.ToList());
                }
            }

            // Create nested dict with round -> email -> individual feedback object
            // so each email still has several feedback objects associated with it
            var biggerDict = new Dictionary<int, Dictionary<string, List<InvertedQualtrics>>>();
            foreach (var item in dict)
            {
                var aggregatedFeedback = AggregateFeedbackByPerson(item.Value);
                biggerDict[item.Key] = aggregatedFeedback;
            }

            var numberOfRounds = biggerDict.Keys.Count;

            if (numberOfRounds != currentRoundId)
            {
                throw new Exception("The algorithm fetched data for more rounds than it was asked to.");
            }

            string[] rounds = new string[numberOfRounds];
            var stringRounds = await _serviceFactory.RoundService.GetFirstNRounds(numberOfRounds);
            rounds = stringRounds.OrderBy(x => x.Id).Select(x => x.Name).ToArray();

            var users = biggerDict[currentRoundId].Keys ?? throw new Exception("'users' is null.");

            if (users.Count != dict[currentRoundId].Select(x => x.ReviewerEmail).Distinct().Count())
            {
                throw new Exception("Dictionaries were not created properly.");
            }

            // Iterate through feedback items and generate sections for each
            for (int j = 0; j < dict[currentRoundId].Select(x => x.ReviewerEmail).Distinct().Count(); j++)
            {
                string user;
                try
                {
                    user = users.ElementAt(j);
                }
                catch
                {
                    throw new Exception($"There is not a '{j}' element in 'users'");
                }

                var thisUsersFirstFeedback = biggerDict[currentRoundId][user][0] ?? throw new Exception("'thisUsersFirstFeedback' is null.");

                var firstname = thisUsersFirstFeedback.FirstName;
                var lastname = thisUsersFirstFeedback.LastName;
                var email = thisUsersFirstFeedback.Email;
                var projectName = thisUsersFirstFeedback.Project.Name;
                var projectId = thisUsersFirstFeedback.Project.Id;
                var timeframe = thisUsersFirstFeedback.Timeframe.Name;

                string[] technical = new string[numberOfRounds + 1];
                string[] analytical = new string[numberOfRounds + 1];
                string[] communication = new string[numberOfRounds + 1];
                string[] participation = new string[numberOfRounds + 1];
                string[] performance = new string[numberOfRounds + 1];

                string[] strengths = new string[thisUsersFirstFeedback.Project.NoOfMembers];
                string[] improvements = new string[thisUsersFirstFeedback.Project.NoOfMembers];
                string[] comments = new string[thisUsersFirstFeedback.Project.NoOfMembers];

                for (int k = 1; k <= numberOfRounds; k++)
                {
                    var allRoundFeedback = biggerDict[k] ?? throw new Exception($"Super dictionary does not have key for round {k}");
                    var allRoundFeedbackForUser = allRoundFeedback[user] ?? throw new Exception($"User '{user}' was not found in round {k}");

                    double techScore = 0;
                    double anaScore = 0;
                    double comScore = 0;
                    double partScore = 0;
                    double perfScore = 0;

                    if (allRoundFeedbackForUser.Count != thisUsersFirstFeedback.Project.NoOfMembers)
                    {
                        throw new Exception($"User {user} has more submissions that team members.");
                    }

                    for (int l = 0; l < allRoundFeedbackForUser.Count; l++)
                    {
                        if (allRoundFeedbackForUser[l].ReviewerEmail == user)
                        {
                            technical[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Technology].ToString();
                            analytical[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Analytical].ToString();
                            communication[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Communication].ToString();
                            participation[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Participation].ToString();
                            performance[0] = allRoundFeedbackForUser[l].Ratings[Capstone.Performance].ToString();
                        }
                        else
                        {
                            techScore += allRoundFeedbackForUser[l].Ratings[Capstone.Technology];
                            anaScore += allRoundFeedbackForUser[l].Ratings[Capstone.Analytical];
                            comScore += allRoundFeedbackForUser[l].Ratings[Capstone.Communication];
                            partScore += allRoundFeedbackForUser[l].Ratings[Capstone.Participation];
                            perfScore += allRoundFeedbackForUser[l].Ratings[Capstone.Performance];
                        }

                        // only needs to be set the last time through the k loop
                        if (k == numberOfRounds)
                        {
                            strengths[l] = allRoundFeedbackForUser[l].Questions[Capstone.Strengths];
                            improvements[l] = allRoundFeedbackForUser[l].Questions[Capstone.Improvements];
                            comments[l] = allRoundFeedbackForUser[l].Questions[Capstone.Comments];
                        }
                    }

                    if (k == numberOfRounds)
                    {
                        foreach (string value in strengths)
                        {
                            if (value == null)
                            {
                                throw new Exception("Comments were not populated properly.");
                            }
                        }
                    }

                    techScore = Math.Round(techScore / (allRoundFeedbackForUser.Count - 1), 1);
                    anaScore = Math.Round(anaScore / (allRoundFeedbackForUser.Count - 1), 1);
                    comScore = Math.Round(comScore / (allRoundFeedbackForUser.Count - 1), 1);
                    partScore = Math.Round(partScore / (allRoundFeedbackForUser.Count - 1), 1);
                    perfScore = Math.Round(perfScore / (allRoundFeedbackForUser.Count - 1), 1);

                    technical[k] = techScore.ToString();
                    analytical[k] = anaScore.ToString();
                    communication[k] = comScore.ToString();
                    participation[k] = partScore.ToString();
                    performance[k] = perfScore.ToString();

                    if (techScore == 0)
                    {
                        throw new Exception("Rating scores were not calculated properly.");
                    }

                    if (technical[0].Length > 1)
                    {
                        throw new Exception("Personal scores were not assigned properly.");
                    }

                }

                foreach (string value in technical)
                {
                    if (value == null)
                    {
                        throw new Exception("Ratings were not populated properly.");
                    }
                }

                list.Add(new DocumentToPrint
                {
                    Organization = "Capstone 360 Reviews",
                    FirstName = firstname,
                    LastName = lastname,
                    FullName = firstname + " " + lastname,
                    Email = email,
                    TimeframeName = timeframe,
                    ProjectId = projectId,
                    ProjectName = projectName,
                    RoundNumber = currentRoundId,
                    RoundName = "Round " + currentRoundId,
                    Technical = technical,
                    Analytical = analytical,
                    Communication = communication,
                    Participation = participation,
                    Performance = performance,
                    Strengths = strengths,
                    AreasForImprovement = improvements,
                    Comments = comments,
                    Rounds = rounds
                });
            }

            return list;
        }

        private static ListItem CreateSkillSetListItem(string skill, int tabs, params string[] values)
        {
            var itemString = $"{skill}";

            var tabString = "";
            for (int i = 0; i < tabs; i++)
            {
                tabString += "\t";
            }

            for (int j = 0; j < values.Length; j++)
            {
                if(j == 0)
                {
                    itemString += $"{values[j]}{tabString}";
                } else if(j == values.Length - 1)
                {
                    itemString += $"{values[j]}";
                } else
                {
                    itemString += $"{values[j]}\t\t\t\t";
                }
            }

            return new ListItem(itemString);
        }

        private static List CreateList(params string[] values)
        {
            var listItems = new List().SetSymbolIndent(12).SetListSymbol("\u2022");
            
            foreach(var value in values)
            {
                listItems.Add(new ListItem(value + "\n"));
            }
            return listItems;
        }

        private static Dictionary<string, List<InvertedQualtrics>> AggregateFeedbackByPerson(IEnumerable<InvertedQualtrics> invertedQualtrics)
        {
            var list = invertedQualtrics.ToList();
            list.Sort((x,y) => x.Email.CompareTo(y.Email));
            var aggregatedFeedbackByPerson = new Dictionary<string, List<InvertedQualtrics>>();
            foreach (var feedback in list)
            {
                if (aggregatedFeedbackByPerson.TryGetValue(feedback.Email, out List<InvertedQualtrics>? value))
                {
                    value.Add(feedback);
                }
                else
                {
                    aggregatedFeedbackByPerson.Add(feedback.Email, [feedback]);
                }
            }
            return aggregatedFeedbackByPerson;
        }

        private void IndividualCapstonePdf(Document document, DocumentToPrint documentMaterial)
        {
            // Identifying information
            document.Add(new Paragraph(
                documentMaterial.FullName + "\n" +
                documentMaterial.Email + "\n" +
                documentMaterial.ProjectName + "\n" +
                documentMaterial.TimeframeName + " " +
                documentMaterial.RoundName + " " +
                documentMaterial.Organization
            ).SetTextAlignment(TextAlignment.LEFT).SetFontSize(12));

            // Add a section title
            document.Add(new Paragraph("\nSkills Review").SetBold().SetFontSize(14));

            // Create the skills review list (bullet points)
            var skillsList = new List().SetSymbolIndent(12).SetListSymbol("\u2022");

            skillsList.Add(CreateSkillSetListItem("Skill Set:\t\tPersonal\t\t", 2, documentMaterial.Rounds));
            skillsList.Add(CreateSkillSetListItem("Technical:\t\t\t", 3, documentMaterial.Technical));
            skillsList.Add(CreateSkillSetListItem("Analytical:\t\t\t", 3, documentMaterial.Analytical));
            skillsList.Add(CreateSkillSetListItem("Communication:\t", 3, documentMaterial.Communication));
            skillsList.Add(CreateSkillSetListItem("Participation:\t\t", 3, documentMaterial.Participation));
            skillsList.Add(CreateSkillSetListItem("Performance:\t\t", 3, documentMaterial.Performance));

            document.Add(skillsList);

            // Add rating explanation
            document.Add(new Paragraph("\nExcellent = 5 | Very Good = 4 | Satisfactory = 3 | Fair = 2 | Poor = 1").SetItalic());

            // Add Strengths
            document.Add(new Paragraph("\nStrengths:\n"));
            document.Add(CreateList(documentMaterial.Strengths));

            // Add Improvements
            document.Add(new Paragraph("\nImprovements:\n"));
            document.Add(CreateList(documentMaterial.AreasForImprovement));

            // Add Additional Comments
            document.Add(new Paragraph("\nAdditional Comments:\n"));
            document.Add(CreateList(documentMaterial.Comments));
        }

        private async Task<byte[]> WritePdfAsync(WritePdfContent pdfWriter, DocumentToPrint documentContent)
        {
            _logger.LogInformation("Writing PDF...");

            // Create a MemoryStream to hold the PDF in memory
            using var memoryStream = new MemoryStream();

            // Initialize PDF writer and document
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Start writing the pdf

            pdfWriter(document, documentContent);

            // Stop writing the pdf

            // Close document
            document.Close();

            // Return the PDF as a byte array
            return memoryStream.ToArray();
        }
    }
}