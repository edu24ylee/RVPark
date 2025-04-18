using ApplicationCore.Models;
using System.Threading.Tasks;

namespace ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        IGenericRepository<Park> Park { get; }
        IGenericRepository<LotType> LotType { get; }
        IGenericRepository<Lot> Lot { get; }
        IGenericRepository<Reservation> Reservation { get; }
        IGenericRepository<Guest> Guest { get; }
        IGenericRepository<RV> RV { get; }
        IGenericRepository<User> User { get; }
        IGenericRepository<Employee> Employee { get; }
        IGenericRepository<ReservationReport> ReservationReport { get; }
        IGenericRepository<Fee> Fee { get; }
        IGenericRepository<FeeType> FeeType { get; }
        IGenericRepository<Policy> Policy { get; }
        IGenericRepository<DodAffiliation> DodAffiliation { get; }

        int Commit();
        Task<int> CommitAsync();
    }
}
