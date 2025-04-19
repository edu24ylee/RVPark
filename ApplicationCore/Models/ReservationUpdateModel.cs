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

        public Rv Rv { get; set; }

        public string GuestName { get; set; }

        public List<Lot> AvailableLots { get; set; }

        public List<LotType> LotTypes { get; set; }

        public decimal OriginalTotal { get; set; }
        public List<ManualFeeOptionViewModel>? ManualFeeOptions { get; set; }
        public int? ManualFeeTypeId { get; set; }

        public int Duration
        {
            get => (Reservation.EndDate - Reservation.StartDate).Days;
            set => Reservation.Duration = value;
        }
    }
}
