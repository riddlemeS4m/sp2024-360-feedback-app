﻿using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;
using Capstone_360s.Models.FeedbackDb;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Capstone_360s.Interfaces.IDbContext
{
    public interface IFeedbackDbContext
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
        public DbSet<UserTimeframe> UserTimeframes { get; set; }
        IModel Model { get; }

        DbSet<T> Set<T>() where T : class;
        void Add<T>(T entity) where T : class;
        void AddRange<T>(IEnumerable<T> entities) where T : class;
        void Update<T>(T entity) where T : class;
        void Remove<T>(T entity) where T : class;
        EntityEntry<T> Entry<T>(T entity) where T : class;
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        EntityEntry Entry(object entity);
        IEnumerable<EntityEntry<T>> GetChangeTracker<T>() where T : class;
    }
}
