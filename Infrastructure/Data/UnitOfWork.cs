using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _dbContext;
    }
    public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        private IGenericRepository<FILLER> _FILLER;
        public IGenericRepository<FILLLER> Category
        {
            get
            {
                if (_FILLLER == null)
                {
                    _FILLLER = new GenericRepository<FILLLER>(_dbContext);
                }
                return _FILLLER;
            }
        }
        public int Commit()
        {
            return _dbContext.SaveChanges();
        }
        public async Task<int> CommitAsync()
        {
            return await _dbContext.SaveChangesAsync();
        }
        //additional method added for garbage disposal
        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
