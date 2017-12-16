using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AI_Project_Reservations.Models
{
    [MetadataType(typeof(ReservationMetaData))]
    public partial class Reservation
    {
        public DateTime reservationDate { get; set; }
        public DateTime startTime { get; set; }
        public DateTime endTime { get; set; }
    }

    public class ReservationMetaData
    {

        [Display(Name = "Room")]
        [Required(ErrorMessage = "Room is required")]
        public Room Room { get; set; }

        [Display(Name = "Reservation Date")]
        [Required(ErrorMessage = "Reservation Date is required")]
        [DataType(DataType.Date)]
        public DateTime reservationDate { get; set; }

        [Display(Name = "Start time")]
        [Required(ErrorMessage = "Start time is required")]
        [DataType(DataType.Time)]
        public DateTime startTime { get; set; }

        [Display(Name = "End time")]
        [Required(ErrorMessage = "End time is required")]
        [DataType(DataType.Time)]
        public DateTime endTime { get; set; }

        [Display(Name = "Description")]
        public string description { get; set; }

        [Display(Name = "Subject")]
        [Required(ErrorMessage = "Subject is required")]
        public Subject Subject { get; set; }
    }
}
