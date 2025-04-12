using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        // By using ReadOnly ApplicationDbContext, you can have access to only
        // querying capabilities of DbContext. UnitOfWork writes
        // (commits) to the PHYSICAL tables (not internal object).
        private readonly ApplicationDbContext _dbContext;

        public GenericRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;

        }

        public void Add(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(T entity)
        {
            _dbContext.Set<T>().Remove(entity);
            _dbContext.SaveChanges();
        }

        public void Delete(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
            _dbContext.SaveChanges();
        }

        public virtual T Get(Expression<Func<T, bool>> predicate, bool trackChanges = false, string? includes = null)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includes)) // If other objects to include (join)
            {
                foreach (var includeProperty in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(includeProperty);
                }
            }

            if (trackChanges) // If is false, we do not want EF tracking changes
            {
                queryable = queryable.AsNoTracking();
            }

            return queryable.FirstOrDefault(predicate);
        }


        public virtual async Task<T> GetAsync(Expression<Func<T, bool>> predicate, bool trackChanges = false, string? includes = null)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includes)) // If other objects to include (join)
            {
                var includeProperties = includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var includeProperty in includeProperties)
                {
                    queryable = queryable.Include(includeProperty);
                }
            }

            if (trackChanges) // If is false, we do not want EF tracking changes
            {
                queryable = queryable.AsNoTracking();
            }

            return await queryable.FirstOrDefaultAsync(predicate);
        }
        // The virtual keyword is used to modify a method, property, indexer, or
        // and allows for it to be overridden in a derived class.
        public virtual T GetById(int? id)
        {
            return _dbContext.Set<T>().Find(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, int>>? orderBy = null, string? includes = null)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var includeProperty in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(includeProperty);
                }
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (orderBy != null)
            {
                queryable = queryable.OrderBy(orderBy);
            }

            return await queryable.ToListAsync();
        }
        public async Task ReloadAsync(T entity, string? includes = null)
        {
            var entry = _dbContext.Entry(entity);

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var include in includes.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    if (include.Contains('.'))
                    {
                        var parts = include.Split('.');
                        var firstLevel = entry.Reference(parts[0]);
                        await firstLevel.LoadAsync();

                        var nestedEntity = firstLevel.CurrentValue;
                        if (nestedEntity != null)
                        {
                            var nestedEntry = _dbContext.Entry(nestedEntity);
                            await nestedEntry.Reference(parts[1]).LoadAsync();
                        }
                    }
                    else
                    {
                        await entry.Reference(include).LoadAsync();
                    }
                }
            }
            else
            {
                await entry.ReloadAsync();
            }
        }

        public void Update(T entity)
        {
            var entry = _dbContext.Entry(entity);
            var key = _dbContext.Model.FindEntityType(typeof(T))?.FindPrimaryKey()?.Properties.FirstOrDefault();

            if (key == null)
                throw new InvalidOperationException("No primary key defined for this entity.");

            var keyValue = key.PropertyInfo.GetValue(entity);

            var local = _dbContext.Set<T>().Local
                .FirstOrDefault(e =>
                    key.PropertyInfo.GetValue(e)?.Equals(keyValue) == true);

            if (local != null)
            {
                _dbContext.Entry(local).State = EntityState.Detached;
            }

            _dbContext.Entry(entity).State = EntityState.Modified;
            _dbContext.SaveChanges();
        }



        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? predicate = null, Expression<Func<T, int>>? orderBy = null, string? includes = null)
        {
            IQueryable<T> queryable = _dbContext.Set<T>();

            if (!string.IsNullOrEmpty(includes))
            {
                foreach (var includeProperty in includes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    queryable = queryable.Include(includeProperty);
                }
            }

            if (predicate != null)
            {
                queryable = queryable.Where(predicate);
            }

            if (orderBy != null)
            {
                queryable = queryable.OrderBy(orderBy);
            }

            return queryable.ToList();
        }
    }
}
