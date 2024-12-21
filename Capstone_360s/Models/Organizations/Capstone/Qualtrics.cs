namespace Capstone_360s.Models.Organizations.Capstone
{
    public class Qualtrics
    {
        // Metadata about the response
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ResponseType { get; set; }
        public string IPAddress { get; set; }
        public double? Progress { get; set; }
        public int? DurationSeconds { get; set; }
        public bool? IsFinished { get; set; }
        public DateTime? RecordedDate { get; set; }
        public string ResponseId { get; set; }

        // Recipient information
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string ExternalReference { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string DistributionChannel { get; set; }
        public string UserLanguage { get; set; }

        // Responses about the respondent's self-assessment
        public string TechnologySelf { get; set; }
        public string AnalyticalSelf { get; set; }
        public string CommunicationSelf { get; set; }
        public string ParticipationSelf { get; set; }
        public string PerformanceSelf { get; set; }
        public string StrengthsSelf { get; set; }
        public string GrowthAreasSelf { get; set; }
        public string CommentsSelf { get; set; }

        // Feedback on team member 1
        public string Member1NameConfirmation { get; set; }
        public string TechnologyMember1 { get; set; }
        public string AnalyticalMember1 { get; set; }
        public string CommunicationMember1 { get; set; }
        public string ParticipationMember1 { get; set; }
        public string PerformanceMember1 { get; set; }
        public string StrengthsMember1 { get; set; }
        public string GrowthAreasMember1 { get; set; }
        public string CommentsMember1 { get; set; }

        // Feedback on team member 2
        public string Member2NameConfirmation { get; set; }
        public string TechnologyMember2 { get; set; }
        public string AnalyticalMember2 { get; set; }
        public string CommunicationMember2 { get; set; }
        public string ParticipationMember2 { get; set; }
        public string PerformanceMember2 { get; set; }
        public string StrengthsMember2 { get; set; }
        public string GrowthAreasMember2 { get; set; }
        public string CommentsMember2 { get; set; }

        // Feedback on team member 3
        public string Member3NameConfirmation { get; set; }
        public string TechnologyMember3 { get; set; }
        public string AnalyticalMember3 { get; set; }
        public string CommunicationMember3 { get; set; }
        public string ParticipationMember3 { get; set; }
        public string PerformanceMember3 { get; set; }
        public string StrengthsMember3 { get; set; }
        public string GrowthAreasMember3 { get; set; }
        public string CommentsMember3 { get; set; }

        // Feedback on team member 4
        public string Member4NameConfirmation { get; set; }
        public string TechnologyMember4 { get; set; }
        public string AnalyticalMember4 { get; set; }
        public string CommunicationMember4 { get; set; }
        public string ParticipationMember4 { get; set; }
        public string PerformanceMember4 { get; set; }
        public string StrengthsMember4 { get; set; }
        public string GrowthAreasMember4 { get; set; }
        public string CommentsMember4 { get; set; }

        // Feedback on team member 5
        public string Member5NameConfirmation { get; set; }
        public string TechnologyMember5 { get; set; }
        public string AnalyticalMember5 { get; set; }
        public string CommunicationMember5 { get; set; }
        public string ParticipationMember5 { get; set; }
        public string PerformanceMember5 { get; set; }
        public string StrengthsMember5 { get; set; }
        public string GrowthAreasMember5 { get; set; }
        public string CommentsMember5 { get; set; }

        // Feedback on team member 1
        public string Member6NameConfirmation { get; set; }
        public string TechnologyMember6 { get; set; }
        public string AnalyticalMember6 { get; set; }
        public string CommunicationMember6 { get; set; }
        public string ParticipationMember6 { get; set; }
        public string PerformanceMember6 { get; set; }
        public string StrengthsMember6 { get; set; }
        public string GrowthAreasMember6 { get; set; }
        public string CommentsMember6 { get; set; }

        // General survey scores and faculty sponsor information
        public double? Score { get; set; }
        public string FacultySponsor { get; set; }

        // Team information
        public string Member1 { get; set; }
        public string Member2 { get; set; }
        public string Member3 { get; set; }
        public string Member4 { get; set; }
        public string Member5 { get; set; }
        public int? NumTeamMembers { get; set; }
        public string TeamName { get; set; }
        public string TeamNumber { get; set; }
    }
}
