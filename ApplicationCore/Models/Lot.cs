using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    public class Lot
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Location { get; set; }
        [Required] 
        public double Length { get; set; }
        [Required]
        public double Width { get; set; }
        public int HeightLimit { get; set; }
        [Required]
        public bool IsAvailable { get; set; }
        public string Description { get; set; }

        public int LotTypeId { get; set; }
        [ForeignKey("LotTypeId")]
        public virtual LotType LotType { get; set; }
        public string Image { get; set; }
    }
}
