using ApplicationCore.Interfaces;
using ApplicationCore.Models;
using ApplicationCore;
using Infrastructure.Services;
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
        private IGenericRepository<Reservation> _Reservation;
        private IGenericRepository<RV> _RV;
        private IGenericRepository<Guest> _Guest;
        private IGenericRepository<User> _User;
        private IGenericRepository<Employee> _Employee;



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
        public IGenericRepository<Reservation> Reservation
        {
            get
            {
                if (_Reservation == null)
                {
                    _Reservation = new GenericRepository<Reservation>(_dbContext);
                }
                return _Reservation;
            }
        }
        public IGenericRepository<RV> RV
        {
            get
            {
                if (_RV == null)
                {
                    _RV = new GenericRepository<RV>(_dbContext);
                }
                return _RV;
            }
        }
        public IGenericRepository<Guest> Guest
        {
            get
            {
                if (_Guest == null)
                {
                    _Guest = new GenericRepository<Guest>(_dbContext);
                }
                return _Guest;
            }
        }
        public IGenericRepository<User> User
        {
            get
            {
                if (_User == null)
                {
                    _User = new GenericRepository<User>(_dbContext);
                }
                return _User;
            }
        }
        public IGenericRepository<Employee> Employee
        {
            get
            {
                if (_Employee == null)
                {
                    _Employee = new GenericRepository<Employee>(_dbContext);
                }
                return _Employee;
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
