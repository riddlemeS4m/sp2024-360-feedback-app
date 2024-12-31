using Capstone_360s.Interfaces.IDbContext;
using Capstone_360s.Interfaces.IService;
using Microsoft.EntityFrameworkCore;

namespace Capstone_360s.Services.FeedbackDb
{
    public class GenericFeedbackDbService<T> : IAccessData<T> where T : class
    {
        protected readonly IFeedbackDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericFeedbackDbService(IFeedbackDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context), "Database context cannot be null.");
            _dbSet = context.Set<T>();

            if (_dbSet == null)
            {
                throw new InvalidOperationException($"Database set '{typeof(T).Name}' has not been initialized.");
            }
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<T> GetByIdAsync(object id)
        {
            return await _dbSet.FindAsync(id);
        }

        public async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var trackedEntity = _context.GetChangeTracker<T>()
                    .FirstOrDefault(e => e.Entity.Equals(entity));

                if (trackedEntity == null)
                {
                    _dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Added;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<T> UpdateAsync(T entity)
        {
            var trackedEntity = _context.GetChangeTracker<T>()
                .FirstOrDefault(e => e.Entity.Equals(entity));

            if (trackedEntity != null)
            {
                trackedEntity.CurrentValues.SetValues(entity);
            }
            else
            {
                var key = _context.Model.FindEntityType(typeof(T))
                            .FindPrimaryKey()
                            .Properties
                            .Select(x => x.Name)
                            .FirstOrDefault();

                var keyValue = typeof(T).GetProperty(key)?.GetValue(entity);

                var existingEntity = await _dbSet.FindAsync(keyValue);
                if (existingEntity != null)
                {
                    _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                }
                else
                {
                    _dbSet.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var trackedEntity = _context.GetChangeTracker<T>()
                    .FirstOrDefault(e => e.Entity.Equals(entity));

                if (trackedEntity != null)
                {
                    trackedEntity.CurrentValues.SetValues(entity);
                }
                else
                {
                    var key = _context.Model.FindEntityType(typeof(T))
                                .FindPrimaryKey()
                                .Properties
                                .Select(x => x.Name)
                                .FirstOrDefault();

                    var keyValue = typeof(T).GetProperty(key)?.GetValue(entity);

                    var existingEntity = await _dbSet.FindAsync(keyValue);
                    if (existingEntity != null)
                    {
                        _context.Entry(existingEntity).CurrentValues.SetValues(entity);
                    }
                    else
                    {
                        _dbSet.Attach(entity);
                        _context.Entry(entity).State = EntityState.Modified;
                    }
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(object id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity == null)
            {
                return false;
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task Remove(T entity)
        {
            var trackedEntity = _context.GetChangeTracker<T>()
                .FirstOrDefault(e => e.Entity.Equals(entity));

            if (trackedEntity == null)
            {
                _dbSet.Attach(entity);
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                var trackedEntity = _context.GetChangeTracker<T>()
                    .FirstOrDefault(e => e.Entity.Equals(entity));

                if (trackedEntity == null)
                {
                    _dbSet.Attach(entity);
                }
            }

            _dbSet.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
