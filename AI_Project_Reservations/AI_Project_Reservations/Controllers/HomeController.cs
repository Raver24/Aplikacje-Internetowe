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
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult IndexTeacher()
        {
            return View();
        }
    }
}
