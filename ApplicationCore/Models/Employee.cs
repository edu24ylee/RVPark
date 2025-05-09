﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApplicationCore.Models
{
  
    public class Employee
    {

        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
        public bool IsArchived { get; set; } = false;
    }
}
