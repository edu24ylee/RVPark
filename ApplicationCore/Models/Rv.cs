using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApplicationCore.Models;

namespace ApplicationCore.Models
{
    public class RV
    {
        [Key]
        public int RvID { get; set; }

        [Required]
        public int GuestID { get; set; }

        [ForeignKey("GuestID")]
        public Guest Guest { get; set; }

        [Required]
        public string LicensePlate { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        public string Description { get; set; }

        public int Length { get; set; }

        public static RV GetGuestRV(List<RV> rvList, int guestId)
        {
            return rvList.FirstOrDefault(rv => rv.GuestID == guestId);
        }
    }
}
