using ApplicationCore.Models;
using ApplicationCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        public IGenericRepository<Park> Park { get; }
        public IGenericRepository<LotType> LotType { get; }
        public IGenericRepository<Lot> Lot { get; }
        public IGenericRepository<Reservation> Reservation { get; }
        public IGenericRepository<ReservationUpdateModel> ReservationUpdateModel { get; }
        public IGenericRepository<Guest> Guest { get; }
        public IGenericRepository<RV> RV { get; }
        public IGenericRepository<User> User { get; }
        public IGenericRepository<Employee> Employee { get; }
        public IGenericRepository<ReservationReport> ReservationReport { get; }
        public IGenericRepository<Fee> Fee { get; }
        public IGenericRepository<FeeType> FeeType { get; }
        public IGenericRepository<Policy> Policy { get; }
        public IGenericRepository<FinancialReport> FinancialReport { get; }
        public IGenericRepository<DodAffiliation> DodAffiliation { get; }

        int Commit();
        Task<int> CommitAsync();    
    }
}
