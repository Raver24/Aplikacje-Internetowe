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
            List<Reservation> reservations;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                reservations = de.Reservation.ToList();
                if (de.User.Where(x => x.Id.Equals(LoggedOnUser.loggedOnUserID)).FirstOrDefault().isTeacher)
                {
                    ViewBag.type = "teacher";
                }
                else
                {
                    ViewBag.type = "student";
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult AddReservation(Reservation model)
        {
            string message = "";
            bool status = false; ;

            #region Create new object of reservation and assign properties

            Reservation res = new Reservation();
            res.dateFrom = model.reservationDate.Add(model.startTime.TimeOfDay);
            res.dateTo = model.reservationDate.Add(model.endTime.TimeOfDay);
            if (model.description == null)
            {
                res.description = "No description";
            }
            else
            {
                res.description = model.description;
            }
            res.roomId = model.roomId;
            res.subjectId = model.subjectId;
            res.teacherId = LoggedOnUser.loggedOnUserID;

            #endregion

            #region Check if Room and Subject were selected

            if (model.roomId == 0 || model.subjectId == 0)
            {
                message = "Please provide correct room and subject!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");
            }

            #endregion

            #region Check if selected date is future

            if (res.dateFrom.CompareTo(DateTime.Now) <= 0)
            {
                message = "You must select future date!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");
            }

            #endregion

            #region Check if start hours are before end hours

            if (res.dateFrom.CompareTo(res.dateTo) > 0)
            {
                message = "Start time must be before end time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }

            #endregion

            #region Check if room is free at selected time

            if (!isRoomFree(res))
            {
                message = "Selected room is reserved at this time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }

            #endregion

            #region Check if user(teacher) does not have any other classes at this time

            if (!isTeacherFree(res))
            {
                message = "You have classes at this time!";
                TempData["resCreationMessage"] = message;
                TempData["resCreationStatus"] = status;
                return RedirectToAction("IndexTeacher", "Home");

            }

            #endregion


            #region Add reservation to db

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
                    return RedirectToAction("IndexTeacher", "Home");
                }
            }

            #endregion

            status = true;
            message = "Reservation succesfully created";
            TempData["resCreationMessage"] = message;
            TempData["resCreationStatus"] = status;
            return RedirectToAction("IndexTeacher", "Home");
        }


        [HttpGet]
        public ActionResult ViewReservations()
        {
            List<Reservation> reservations;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                reservations = de.Reservation.ToList();
                foreach (Reservation item in reservations)
                {
                    item.SetRoomName();
                    item.SetSubjectName();
                    item.SetAddress();
                }
                if (de.User.Where(x => x.Id.Equals(LoggedOnUser.loggedOnUserID)).FirstOrDefault().isTeacher)
                {
                    ViewBag.type = "teacher";
                    ViewBag.teacherId = LoggedOnUser.loggedOnUserID;
                }
                else
                {
                    ViewBag.type = "student";
                }
            }
            List<Reservation> sortedRes = reservations.OrderBy(o => o.dateFrom).ToList();
            
            return View(sortedRes);
        }

        [HttpGet]
        public ActionResult Edit(int id = 0)
        {
            ai_databaseEntities entity = new ai_databaseEntities();
            Reservation editedReservation = null;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                editedReservation = de.Reservation.Where(x => x.Id.Equals(id)).FirstOrDefault();
            }
            #region set Reservation Date and time

            editedReservation.reservationDate = editedReservation.dateFrom;
            DateTime start = new DateTime(editedReservation.dateFrom.Year, editedReservation.dateFrom.Month, editedReservation.dateFrom.Day, editedReservation.dateFrom.Hour, editedReservation.dateFrom.Minute, 0);
            DateTime end = new DateTime(editedReservation.dateTo.Year, editedReservation.dateTo.Month, editedReservation.dateTo.Day, editedReservation.dateTo.Hour, editedReservation.dateTo.Minute, 0);
            editedReservation.startTime = start;
            editedReservation.endTime = end;

            #endregion
            var getAllRoomsList = entity.Room.ToList();
            SelectList allRooms = new SelectList(getAllRoomsList, "Id", "Name");
            ViewBag.getRooms = allRooms;

            var getSubjects = entity.Subject.ToList();
            SelectList allSubjects = new SelectList(getSubjects, "Id", "Name");
            ViewBag.getSubjects = allSubjects;
            
            return View(editedReservation);
        }

        [HttpPost]
        public ActionResult Edit(Reservation model)
        {
            string message = "";
            bool status = false;

            #region Create new object of reservation and assign properties

            model.dateFrom = model.reservationDate.Add(model.startTime.TimeOfDay);
            model.dateTo = model.reservationDate.Add(model.endTime.TimeOfDay);
            if (model.description == null)
            {
                model.description = "No description";
            }
            model.roomId = model.roomId;
            model.subjectId = model.subjectId;
            model.teacherId = LoggedOnUser.loggedOnUserID;

            #endregion

            #region Check if Room and Subject were selected

            if (model.roomId == 0 || model.subjectId == 0)
            {
                message = "Please provide correct room and subject!";
                TempData["resEditMessage"] = message;
                TempData["resEditStatus"] = status;
                return RedirectToAction("ViewReservations", "Reservation");
            }

            #endregion

            #region Check if selected date is future

            if (model.dateFrom.CompareTo(DateTime.Now) <= 0)
            {
                message = "You must select future date!";
                TempData["resEditMessage"] = message;
                TempData["resEditStatus"] = status;
                return RedirectToAction("ViewReservations", "Reservation");
            }

            #endregion

            #region Check if start hours are before end hours

            if (model.dateFrom.CompareTo(model.dateTo) > 0)
            {
                message = "Start time must be before end time!";
                TempData["resEditMessage"] = message;
                TempData["resEditStatus"] = status;
                return RedirectToAction("ViewReservations", "Reservation");

            }

            #endregion

            #region Check if room is free at selected time

            if (!isRoomFree(model))
            {
                message = "Selected room is reserved at this time!";
                TempData["resEditMessage"] = message;
                TempData["resEditStatus"] = status;
                return RedirectToAction("ViewReservations", "Reservation");

            }

            #endregion

            #region Check if user(teacher) does not have any other classes at this time

            if (!isTeacherFree(model))
            {
                message = "You have classes at this time!";
                TempData["resEditMessage"] = message;
                TempData["resEditStatus"] = status;
                return RedirectToAction("ViewReservations", "Reservation");

            }

            #endregion


            #region Edit reservation

            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                model.Room = de.Room.Where(x => x.Id.Equals(model.roomId)).FirstOrDefault();
                model.Subject = de.Subject.Where(x => x.Id.Equals(model.subjectId)).FirstOrDefault();
                de.Entry(model).State = System.Data.Entity.EntityState.Modified;
                de.SaveChanges();
                
            }

            #endregion

            status = true;
            message = "Reservation succesfully edited";
            TempData["resEditMessage"] = message;
            TempData["resEditStatus"] = status;

            return RedirectToAction("ViewReservations", "Reservation");
        }

        [HttpGet]
        public ActionResult Delete()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Delete(Reservation model)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                Reservation delRes = de.Reservation.Where(x => x.Id.Equals(model.Id)).FirstOrDefault();
                if (delRes != null)
                {
                    de.Reservation.Remove(delRes);
                    de.SaveChanges();
                }
            }
            return RedirectToAction("ViewReservations", "Reservation");
        }
        [NonAction]
        public bool isTeacherFree(Reservation reservation)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                var v = de.Reservation.Where(a => a.teacherId.Equals(reservation.teacherId)).ToList();
                foreach (Reservation reserv in v)
                {
                    if (reserv.Id != reservation.Id)
                    {
                        bool result = false;
                        int SScompare = reservation.dateFrom.CompareTo(reserv.dateFrom);
                        int EScompare = reservation.dateTo.CompareTo(reserv.dateFrom);
                        int SEcompare = reservation.dateFrom.CompareTo(reserv.dateTo);
                        if (SScompare < 0 && EScompare <= 0)
                        {
                            result = true;
                        }
                        if (SEcompare >= 0)
                        {
                            result = true;
                        }
                        if (!result)
                        {
                            return result;
                        }
                    }
                    
                }
                return true;
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
                    if (reserv.Id != reservation.Id)
                    {
                        bool result = false;
                        int SScompare = reservation.dateFrom.CompareTo(reserv.dateFrom);
                        int EScompare = reservation.dateTo.CompareTo(reserv.dateFrom);
                        int SEcompare = reservation.dateFrom.CompareTo(reserv.dateTo);
                        if (SScompare < 0 && EScompare <= 0)
                        {
                            result = true;
                        }
                        if (SEcompare >= 0)
                        {
                            result = true;
                        }
                        if (!result)
                        {
                            return result;
                        }
                    }
                    
                }
                return true;
            }
        }
    }
}