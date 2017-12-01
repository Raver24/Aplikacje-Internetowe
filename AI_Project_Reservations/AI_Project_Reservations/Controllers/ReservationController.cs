using AI_Project_Reservations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AI_Project_Reservations.Controllers
{
    public class ReservationController : Controller
    {
        [HttpGet]
        public ActionResult AddReservation()
        {
            ai_databaseEntities entity = new ai_databaseEntities();
            var getAllRoomsList = entity.Room.ToList();
            SelectList allRooms = new SelectList(getAllRoomsList, "Id", "Name");
            ViewBag.roomsList = allRooms;

            var getSubjects = entity.Subject.ToList();
            SelectList allSubjects = new SelectList(getSubjects, "Id", "Name");
            ViewBag.subjectsList = allSubjects;
            return View();
        }

        [HttpPost]
        public ActionResult AddReservation(Reservation model)
        {
            return View();
        }
    }
}