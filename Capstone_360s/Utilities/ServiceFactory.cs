using Capstone_360s.Interfaces.IDbContext;

namespace Capstone_360s.Utilities
{
    public static class ServiceFactory
    {
        public static T CreateService<T>(IServiceProvider serviceProvider) where T : class
        {
            var dbContext = serviceProvider.GetRequiredService<IFeedbackDbContext>();
            var logger = serviceProvider.GetService<ILogger<T>>(); // Logger is optional

            return (T)Activator.CreateInstance(typeof(T), dbContext, logger);
        }
    }

}
