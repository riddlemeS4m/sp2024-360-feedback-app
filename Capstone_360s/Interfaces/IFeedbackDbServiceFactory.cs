using Capstone_360s.Services.FeedbackDb;

namespace Capstone_360s.Interfaces
{
    public interface IFeedbackDbServiceFactory
    {
        FeedbackService FeedbackService { get; }
        FeedbackPdfService FeedbackPdfService { get; }
        MetricService MetricService { get; }
        MetricResponseService MetricResponseService { get; }
        QuestionService QuestionService { get; }
        QuestionResponseService QuestionResponseService { get; }
        UserService UserService { get; }
        UserOrganizationService UserOrganizationService { get; }
        OrganizationService OrganizationService { get; }
        ProjectService ProjectService { get; }
        ProjectRoundService ProjectRoundService { get; }
        RoundService RoundService { get; }
        TimeframeService TimeframeService { get; }
        UserTimeframeService UserTimeframeService { get; }
        TeamService TeamService { get; }
    }
}