using ApplicationCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
 
    public class Guest
    {
 
        [Key]
        public int GuestId { get; set; }

        [Required]
        public int UserId { get; set; }
 
        [ForeignKey("UserId")]
        public User User { get; set; }
 
        [Required]
        public int DodId { get; set; }
        public decimal Balance { get; set; } = 0;

        public ICollection<Reservation> Reservations { get; set; }
 
        public ICollection<Rv> Rvs { get; set; }
        public DodAffiliation DodAffiliation { get; set; }


        public Reservation MakeReservation(Lot lot, Rv rv, int duration, DateTime startDate)
        {
            return new Reservation
            {
                GuestId = this.GuestId,
                RvId = rv.RvId,
                LotId = lot.Id,
                Duration = duration,
                StartDate = startDate,
                EndDate = startDate.AddDays(duration),
                Status = "Active"
            };
        }
 
        public void CancelReservation(Reservation reservation)
        {
            reservation.CancelReservation();
        }
 
        public List<Reservation> ViewReservationHistory()
        {
            return Reservations?.ToList() ?? new List<Reservation>();
        }
    }
}
