using Capstone_360s.Data.Constants;
using Capstone_360s.Interfaces.IService;
using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.CapstoneRoster;
using Capstone_360s.Services.FeedbackDb;

namespace Capstone_360s.Services.Maps
{
    public class CapstoneMapToInvertedQualtrics : IMapFeedback<InvertedQualtrics>
    {
        private readonly OrganizationService _organizationService;
        private readonly TimeframeService _timeframeService;
        private readonly ProjectService _projectService;
        private readonly UserService _userService;
        private readonly MetricResponseService _metricResponseService;
        private readonly QuestionResponseService _questionResponseService;
        private readonly ILogger<CapstoneMapToInvertedQualtrics> _logger;
        public CapstoneMapToInvertedQualtrics(OrganizationService organizationService, 
            TimeframeService timeframeService,
            ProjectService projectService, 
            UserService userService, 
            MetricResponseService metricResponseService, 
            QuestionResponseService questionResponseService,
            ILogger<CapstoneMapToInvertedQualtrics> logger)
        {
            _organizationService = organizationService;
            _timeframeService = timeframeService;
            _projectService = projectService;
            _userService = userService;
            _metricResponseService = metricResponseService;
            _questionResponseService = questionResponseService;
            _logger = logger;
        }

        public async Task<IEnumerable<InvertedQualtrics>> MapFeedback(IEnumerable<Feedback> feedback)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Accepts list of feedback and number of rounds to map to InvertedQualtrics objects. Accounts for multiple reviewees.
        /// </summary>
        /// <param name="feedback"></param>
        /// <param name="noOfRounds"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<IEnumerable<InvertedQualtrics>> MapFeedback(IEnumerable<Feedback> feedback, int noOfRounds)
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

            var project = await _projectService.GetByIdAsync(feedback.ElementAt(0).ProjectId);
            var projectsDict = feedback.Select(x => ( x.Id, x.Project )).Distinct().ToDictionary();

            var organization = await _organizationService.GetByIdAsync(project.OrganizationId);
            var timeframe = await _timeframeService.GetByIdAsync(project.TimeframeId);

            var usersInfo = await _userService.GetUsersByOrganizationId(organization.Id);

            var feedbackMetricsDict = await _metricResponseService.GetMetricResponsesDictByFeedbackIds(feedbackIds);
            var feedbackQuestionsDict = await _questionResponseService.GetQuestionResponsesDictByFeedbackIds(feedbackIds);

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
    }
}
