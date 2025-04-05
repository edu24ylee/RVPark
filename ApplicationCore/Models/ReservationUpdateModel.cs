using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
 
    public class ReservationUpdateModel
    {
 
        public Reservation Reservation { get; set; }
 
        public RV Rv { get; set; }
 
        public string GuestName { get; set; }
 
        public List<Lot> AvailableLots { get; set; }

        public List<LotType> LotTypes { get; set; }
 
        public decimal OriginalTotal { get; set; }
    }
}
