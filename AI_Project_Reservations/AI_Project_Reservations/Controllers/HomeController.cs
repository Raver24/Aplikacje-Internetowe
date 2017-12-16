using AI_Project_Reservations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AI_Project_Reservations.Controllers
{
    public class HomeController : Controller
    {
        [Authorize]
        [HttpGet]
        public ActionResult IndexStudent()
        {
            if (LoggedOnUser.loggedOnUserID == -1 || TempData["loggedOnUserId"] != null)
            {
                LoggedOnUser.loggedOnUserID = (int)TempData["loggedOnUserId"];
            }
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

        [Authorize]
        [HttpGet]
        public ActionResult IndexTeacher()
        {
            if (LoggedOnUser.loggedOnUserID == -1 || TempData["loggedOnUserId"] != null)
            {
                LoggedOnUser.loggedOnUserID = (int)TempData["loggedOnUserId"];
            }
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
    }
}
