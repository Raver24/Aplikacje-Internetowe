using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace AI_Project_Reservations.Models
{
    public class UserLogin
    {
        [Display(Name = "Login")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Login required")]
        public string login { get; set; }

        [Display(Name = "Password")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "Password required")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Display(Name = "Remember Me")]
        public bool rememberMe { get; set; }
    }
}