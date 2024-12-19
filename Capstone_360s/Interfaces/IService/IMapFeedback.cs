using Capstone_360s.Models.FeedbackDb;
using Capstone_360s.Models.Generics;

namespace Capstone_360s.Interfaces.IService
{
    public interface IMapFeedback<TInversion> where TInversion : class
    {
        public Task<IEnumerable<TInversion>> MapFeedback(IEnumerable<Feedback> feedback);
        public Task<IEnumerable<TInversion>> MapFeedback(IEnumerable<Feedback> feedback, int noOfRounds);
    }
}
