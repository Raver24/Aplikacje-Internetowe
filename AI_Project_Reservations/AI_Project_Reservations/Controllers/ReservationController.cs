using AI_Project_Reservations.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
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
            ViewBag.getRooms = allRooms;

            var getSubjects = entity.Subject.ToList();
            SelectList allSubjects = new SelectList(getSubjects, "Id", "Name");
            ViewBag.getSubjects = allSubjects;
            return View();
        }

        [HttpPost]
        public ActionResult AddReservation(Reservation model)
        {
            string message = "";
            bool status;
            Reservation res = new Reservation();
            res.dateFrom = model.reservationDate.Add(model.startTime.TimeOfDay);
            res.dateTo = model.reservationDate.Add(model.endTime.TimeOfDay);
            res.description = model.description;
            res.roomId = model.roomId;
            res.subjectId = model.subjectId;
            res.teacherId = (int)TempData["loggedOnUserId"];
            if (!ModelState.IsValid)
            {
                status = false;
                message = "Wrong data inserted!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }
            if (res.dateFrom.CompareTo(res.dateTo) > 0)
            {
                status = false;
                message = "Start time must be before end time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }
            if (!isRoomFree(res))
            {
                status = false;
                message = "Selected room is reserved at this time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }
            if (!isTeacherFree(res))
            {
                status = false;
                message = "You have classes at this time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }
            
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                res.Room = de.Room.Where(x => x.Id.Equals(res.roomId)).FirstOrDefault();
                res.Subject = de.Subject.Where(x => x.Id.Equals(res.subjectId)).FirstOrDefault();
                try
                {
                    de.Reservation.Add(res);
                    de.SaveChanges();
                }
                catch (DbEntityValidationException dbEx)
                {
                    message = dbEx.Message;
                    status = false;
                }
            }
            status = true;
            message = "Reservation Created";
            TempData["resCreationMessage"] = message;
            TempData["resCreationStatus"] = status;
            return RedirectToAction("IndexTeacher", "Home");
        }

        [NonAction]
        public bool isTeacherFree(Reservation reservation)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                var v = de.Reservation.Where(a => a.teacherId.Equals(reservation.teacherId)).ToList();
                foreach (Reservation reserv in v)
                {
                    int SScompare = reservation.dateFrom.CompareTo(reserv.dateFrom);
                    int EScompare = reservation.dateTo.CompareTo(reserv.dateFrom);
                    int SEcompare = reservation.dateFrom.CompareTo(reserv.dateTo);
                    if (SScompare < 0 && EScompare <= 0)
                    {
                        return true;
                    }
                    if (SEcompare >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        [NonAction]
        public bool isRoomFree(Reservation reservation)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                var v = de.Reservation.Where(a => a.roomId.Equals(reservation.roomId)).ToList();
                foreach (Reservation reserv in v)
                {
                    int SScompare = reservation.dateFrom.CompareTo(reserv.dateFrom);
                    int EScompare = reservation.dateTo.CompareTo(reserv.dateFrom);
                    int SEcompare = reservation.dateFrom.CompareTo(reserv.dateTo);
                    if (SScompare < 0 && EScompare <= 0)
                    {
                        return true;
                    }
                    if (SEcompare >= 0)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}