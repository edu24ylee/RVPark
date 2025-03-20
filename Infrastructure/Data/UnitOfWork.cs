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
        public UnitOfWork(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IGenericRepository<Park> _Park;
        private IGenericRepository<LotType> _LotType;
        private IGenericRepository<Lot> _Lot;

        public IGenericRepository<Park> Park
        {
            get
            {
                if (_Park == null)
                {
                    _Park = new GenericRepository<Park>(_dbContext);
                }
                return _Park;
            }
        }
        public IGenericRepository<LotType> LotType
        {
            get
            {
                if (_LotType == null)
                {
                    _LotType = new GenericRepository<LotType>(_dbContext);
                }
                return _LotType;
            }
        }

        public IGenericRepository<Lot> Lot
        {
            get
            {
                if (_Lot == null)
                {
                    _Lot = new GenericRepository<Lot>(_dbContext);
                }
                return _Lot;
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
