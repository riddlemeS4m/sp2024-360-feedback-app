using Capstone_360s.Interfaces;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Services.FeedbackDb;
using System.Reflection;

namespace Capstone_360s.Services
{
    public class FeedbackDbServiceFactory : IFeedbackDbServiceFactory
    {
        public FeedbackService FeedbackService { get; set; }
        public FeedbackPdfService FeedbackPdfService { get; set;  }
        public MetricService MetricService { get; set; }
        public MetricResponseService MetricResponseService { get; set; }
        public QuestionService QuestionService { get; set;  }
        public QuestionResponseService QuestionResponseService { get; set;  }
        public UserService UserService { get; set;  }
        public UserOrganizationService UserOrganizationService { get; set; }
        public OrganizationService OrganizationService { get; set; }
        public ProjectService ProjectService { get; set;  }
        public ProjectRoundService ProjectRoundService { get; set; }
        public RoundService RoundService { get; set;  }
        public TimeframeService TimeframeService { get; set;  }
        public TeamService TeamService { get; set; }

        public FeedbackDbServiceFactory(IServiceProvider serviceProvider)
        {
            foreach (var property in typeof(FeedbackDbServiceFactory).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var serviceType = property.PropertyType;
                var createdService = CreateService(serviceType, serviceProvider);
                property.SetValue(this, createdService);
            }
        }

        private object CreateService(Type serviceType, IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<IFeedbackDbContext>();
            var loggerType = typeof(ILogger<>).MakeGenericType(serviceType);
            var logger = serviceProvider.GetService(loggerType); // Optional logger

            return Activator.CreateInstance(serviceType, dbContext, logger);
        }
    }
}
