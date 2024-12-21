using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Models.FeedbackDb;

namespace Capstone_360s.Data.Contexts
{
    public class FeedbackMySqlDbContext : DbContext, IFeedbackDbContext
    {
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricResponse> MetricResponses { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionResponse> QuestionResponses { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectRound> ProjectRounds { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<TeamMember> Teams { get; set; }
        public DbSet<Timeframe> Timeframes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserOrganization> UserOrganizations { get; set; }

        public FeedbackMySqlDbContext(DbContextOptions<FeedbackMySqlDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email);

            builder.Entity<Project>()
                .HasIndex(p => p.Name);

            builder.Entity<Project>()
                .HasIndex(p => new { p.OrganizationId, p.TimeframeId });
        }

        public DbSet<T> Set<T>() where T : class
        {
            return base.Set<T>();
        }

        public void Add<T>(T entity) where T : class
        {
            base.Add(entity);
        }

        public void AddRange<T>(IEnumerable<T> entities) where T : class
        {
            base.AddRange(entities);
        }

        public void Update<T>(T entity) where T : class
        {
            base.Update(entity);
        }

        public void Remove<T>(T entity) where T : class
        {
            base.Remove(entity);
        }

        public EntityEntry<T> Entry<T>(T entity) where T : class
        {
            return base.Entry(entity);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
