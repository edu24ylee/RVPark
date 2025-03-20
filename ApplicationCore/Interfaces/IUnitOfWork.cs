using ApplicationCore.Models;
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
        int Commit();
        Task<int> CommitAsync();    
    }
}
