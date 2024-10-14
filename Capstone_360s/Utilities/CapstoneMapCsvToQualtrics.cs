using Capstone_360s.Models.CapstoneRoster;
using CsvHelper.Configuration;

namespace Capstone_360s.Utilities
{
    public class CapstoneMapCsvToQualtrics : ClassMap<Qualtrics>
    {
        public CapstoneMapCsvToQualtrics()
        {
            // Metadata about the response
            Map(m => m.StartDate).Name("StartDate");
            Map(m => m.EndDate).Name("EndDate");
            Map(m => m.ResponseType).Name("Status");
            Map(m => m.IPAddress).Name("IPAddress");
            Map(m => m.Progress).Name("Progress");
            Map(m => m.DurationSeconds).Name("Duration (in seconds)");
            Map(m => m.IsFinished).Name("Finished");
            Map(m => m.RecordedDate).Name("RecordedDate");
            Map(m => m.ResponseId).Name("ResponseId");

            // Recipient information
            Map(m => m.LastName).Name("RecipientLastName");
            Map(m => m.FirstName).Name("RecipientFirstName");
            Map(m => m.Email).Name("RecipientEmail");
            Map(m => m.ExternalReference).Name("ExternalReference");
            Map(m => m.Latitude).Name("LocationLatitude");
            Map(m => m.Longitude).Name("LocationLongitude");
            Map(m => m.DistributionChannel).Name("DistributionChannel");
            Map(m => m.UserLanguage).Name("UserLanguage");

            // Responses about the respondent's self-assessment
            Map(m => m.TechnologySelf).Name("Q2_1");
            Map(m => m.AnalyticalSelf).Name("Q2_2");
            Map(m => m.CommunicationSelf).Name("Q2_3");
            Map(m => m.ParticipationSelf).Name("Q2_4");
            Map(m => m.PerformanceSelf).Name("Q2_5");
            Map(m => m.StrengthsSelf).Name("Q4");
            Map(m => m.GrowthAreasSelf).Name("Q5");
            Map(m => m.CommentsSelf).Name("Q6");

            // Feedback on team member 1
            Map(m => m.Member1NameConfirmation).Name("Q7");
            Map(m => m.TechnologyMember1).Name("Q8_1");
            Map(m => m.AnalyticalMember1).Name("Q8_2");
            Map(m => m.CommunicationMember1).Name("Q8_3");
            Map(m => m.ParticipationMember1).Name("Q8_4");
            Map(m => m.PerformanceMember1).Name("Q8_5");
            Map(m => m.StrengthsMember1).Name("Q10");
            Map(m => m.GrowthAreasMember1).Name("Q11");
            Map(m => m.CommentsMember1).Name("Q12");

            // Feedback on team member 2
            Map(m => m.Member2NameConfirmation).Name("Q13");
            Map(m => m.TechnologyMember2).Name("Q14_1");
            Map(m => m.AnalyticalMember2).Name("Q14_2");
            Map(m => m.CommunicationMember2).Name("Q14_3");
            Map(m => m.ParticipationMember2).Name("Q14_4");
            Map(m => m.PerformanceMember2).Name("Q14_5");
            Map(m => m.StrengthsMember2).Name("Q16");
            Map(m => m.GrowthAreasMember2).Name("Q17");
            Map(m => m.CommentsMember2).Name("Q18");

            // Feedback on team member 3
            Map(m => m.Member3NameConfirmation).Name("Q19");
            Map(m => m.TechnologyMember3).Name("Q20_1");
            Map(m => m.AnalyticalMember3).Name("Q20_2");
            Map(m => m.CommunicationMember3).Name("Q20_3");
            Map(m => m.ParticipationMember3).Name("Q20_4");
            Map(m => m.PerformanceMember3).Name("Q20_5");
            Map(m => m.StrengthsMember3).Name("Q22");
            Map(m => m.GrowthAreasMember3).Name("Q23");
            Map(m => m.CommentsMember3).Name("Q24");

            // Feedback on team member 4
            Map(m => m.Member4NameConfirmation).Name("Q25");
            Map(m => m.TechnologyMember4).Name("Q26_1");
            Map(m => m.AnalyticalMember4).Name("Q26_2");
            Map(m => m.CommunicationMember4).Name("Q26_3");
            Map(m => m.ParticipationMember4).Name("Q26_4");
            Map(m => m.PerformanceMember4).Name("Q26_5");
            Map(m => m.StrengthsMember4).Name("Q28");
            Map(m => m.GrowthAreasMember4).Name("Q29");
            Map(m => m.CommentsMember4).Name("Q30");

            // Feedback on team member 5
            Map(m => m.Member5NameConfirmation).Name("Q31");
            Map(m => m.TechnologyMember5).Name("Q32_1");
            Map(m => m.AnalyticalMember5).Name("Q32_2");
            Map(m => m.CommunicationMember5).Name("Q32_3");
            Map(m => m.ParticipationMember5).Name("Q32_4");
            Map(m => m.PerformanceMember5).Name("Q32_5");
            Map(m => m.StrengthsMember5).Name("Q34");
            Map(m => m.GrowthAreasMember5).Name("Q35");
            Map(m => m.CommentsMember5).Name("Q36");

            // Feedback on team member 6
            Map(m => m.Member6NameConfirmation).Name("Q38");
            Map(m => m.TechnologyMember6).Name("Q39_1");
            Map(m => m.AnalyticalMember6).Name("Q39_2");
            Map(m => m.CommunicationMember6).Name("Q39_3");
            Map(m => m.ParticipationMember6).Name("Q39_4");
            Map(m => m.PerformanceMember6).Name("Q39_5");
            Map(m => m.StrengthsMember6).Name("Q41");
            Map(m => m.GrowthAreasMember6).Name("Q42");
            Map(m => m.CommentsMember6).Name("Q43");            

            // General survey scores and faculty sponsor information
            Map(m => m.Score).Name("SC0");
            Map(m => m.FacultySponsor).Name("FACULTYSPONSOR");

            // Team information
            Map(m => m.Member1).Name("MEMBER1");
            Map(m => m.Member2).Name("MEMBER2");
            Map(m => m.Member3).Name("MEMBER3");
            Map(m => m.Member4).Name("MEMBER4");
            Map(m => m.Member5).Name("MEMBER5");
            Map(m => m.NumTeamMembers).Name("NUMTEAMMEMBER");
            Map(m => m.TeamName).Name("TEAMNAME");
            Map(m => m.TeamNumber).Name("TEAMNUM");
        }
    }
}
