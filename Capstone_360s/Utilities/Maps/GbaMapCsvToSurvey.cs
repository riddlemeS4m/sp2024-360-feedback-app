using Capstone_360s.Models.Organizations.GBA;
using CsvHelper.Configuration;

namespace Capstone_360s.Utilities.Maps
{
    [Organization("Gba")]
    public class GbaMapCsvToSurvey : ClassMap<GbaSurvey>
    {
        public GbaMapCsvToSurvey()
        {

        }
    }
}
