using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    [NotMapped]
    public class ReservationUpdateModel
    {
        [Required]
        public Reservation Reservation { get; set; } = new Reservation();

        [Required]
        public RV Rv { get; set; } = new RV();

        [Required]
        public string GuestName { get; set; } = string.Empty;

        public List<Lot> AvailableLots { get; set; } = new List<Lot>();
        public List<LotType> LotTypes { get; set; } = new List<LotType>();

        public decimal OriginalTotal { get; set; } = 0m;

        public List<FeeType> ManualFeeOptions { get; set; } = new List<FeeType>();
        public int? ManualFeeTypeId { get; set; }

        public int Duration
        {
            get => (Reservation.EndDate - Reservation.StartDate).Days;
            set => Reservation.Duration = value;
        }
    }
}
