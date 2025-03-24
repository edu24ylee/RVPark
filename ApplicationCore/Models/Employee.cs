using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationCore.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    namespace ApplicationCore.Models
    {
        public class Employee
        {
            [Key]
            public int EmployeeID { get; set; }

            [Required]
            public int Role { get; set; }

            [Required]
            public int UserID { get; set; }

            [ForeignKey("UserID")]
            public virtual User User { get; set; }

            public Employee(User user, int role)
            {
                User = user;
                UserID = user.UserID;
                Role = role;
            }
        }
    }
}
