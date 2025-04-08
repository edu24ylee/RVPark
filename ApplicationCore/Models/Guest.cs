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
        public int GuestID { get; set; }

        [Required]
        public int UserID { get; set; }
 
        [ForeignKey("UserID")]
        public User User { get; set; }
 
        [Required]
        public int DodId { get; set; }
 
        public ICollection<Reservation> Reservations { get; set; }
 
        public ICollection<RV> RVs { get; set; }
        public DodAffiliation DodAffiliation { get; set; }


        public Reservation MakeReservation(Lot lot, RV rv, int duration, DateTime startDate)
        {
            return new Reservation
            {
                GuestId = this.GuestID,
                RvId = rv.RvID,
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
