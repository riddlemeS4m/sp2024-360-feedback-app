namespace Capstone_360s.Utilities
{
    public class QualtricsStringToNumber
    {
        private static readonly Dictionary<string, int> keyValuePairs = new Dictionary<string, int>()
        {
            { "Excellent", 5 },
            { "Very Good", 4 },
            { "Satisfactory", 3 },
            { "Fair", 2 },
            { "Poor", 1 }
        };

        public static int Convert(string qualtricsString)
        {
            if(keyValuePairs.TryGetValue(qualtricsString, out int value))
            {
                return value;
            } 
            else
            {
                return 0;
            }
        }
    }
}
