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
        public string roomName { get; set; }
        public string subjectName { get; set; }

        public void SetRoomName()
        {
            string name = null;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                name = de.Room.Where(x => x.Id.Equals(this.roomId)).FirstOrDefault().Name;
            }
            if (name != null)
            {
                this.roomName = name;
            }
        }
        public void SetSubjectName()
        {
            string name = null;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                name = de.Subject.Where(x => x.Id.Equals(this.subjectId)).FirstOrDefault().Name;
            }
            if (name != null)
            {
                this.subjectName = name;
            }
        }
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

        [Display(Name = "Room")]
        public string roomName { get; set; }

        [Display(Name = "Subject")]
        public string subjectName { get; set; }
    }
}
