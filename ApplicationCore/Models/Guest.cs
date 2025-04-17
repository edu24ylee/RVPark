using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
    public class Guest
    {
        [Key]
        public int GuestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        [Required]
        public User User { get; set; } = null!;

        public int? DodId { get; set; }

        [ForeignKey(nameof(DodId))]
        public DodAffiliation? DodAffiliation { get; set; }

        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public ICollection<RV> RVs { get; set; } = new List<RV>();

        public Reservation MakeReservation(Lot lot, RV rv, int duration, DateTime startDate)
            => new Reservation
            {
                GuestId = this.GuestId,
                RvId = rv.RvId,
                LotId = lot.Id,
                Duration = duration,
                StartDate = startDate,
                EndDate = startDate.AddDays(duration),
                Status = "Active"
            };

        public void CancelReservation(Reservation reservation)
            => reservation.CancelReservation();

        public List<Reservation> ViewReservationHistory()
            => Reservations.ToList();
    }
}
