using Capstone_360s.Models.FeedbackDb;
using CsvHelper.Configuration;

namespace Capstone_360s.Utilities.Maps
{
    public class BlackboardMapCsvToUser : ClassMap<User>
    {
        public BlackboardMapCsvToUser()
        {
            Map(m => m.LastName).Name("Last Name");
            Map(m => m.FirstName).Name("First Name");
            Map(m => m.Email).Name("Username")
                .Convert(args => $"{args.Row.GetField("Username")}@crimson.ua.edu");
        }
    }
}