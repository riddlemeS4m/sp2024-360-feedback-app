using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Interfaces.IService
{
    public interface IMapFeedback<T> where T : class
    {
        public Task<IEnumerable<T>> MapFeedback(IEnumerable<Feedback> feedback);
        public Task<IEnumerable<T>> MapFeedback(IEnumerable<Feedback> feedback, int noOfRounds);
    }
}
