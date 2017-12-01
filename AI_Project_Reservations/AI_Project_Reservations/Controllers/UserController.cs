using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AI_Project_Reservations.Models;
using System.Net.Mail;
using System.Net;
using System.Web.Security;

namespace AI_Project_Reservations.Controllers
{
    public class UserController : Controller
    {
        //Registration Action
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }


        //Registration POST Action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = String.Empty;

            //Model validation
            if (ModelState.IsValid)
            {
                #region  Login is already used

                var loginExists = IsLoginUsed(user.login);
                if (loginExists)
                {
                    ModelState.AddModelError("LoginExists", "Login already exist");
                    return View(user);
                }

                #endregion

                #region  Email is already used

                var emailExists = IsEmailUsed(user.email);
                if (emailExists)
                {
                    ModelState.AddModelError("EmailExists", "Email already exist");
                    return View(user);
                }

                #endregion

                #region Generate Activation Code

                user.ActivationCode = Guid.NewGuid();

                #endregion

                #region Password Hashing

                user.password = Crypto.Hash(user.password);
                user.ConfirmPassword = Crypto.Hash(user.ConfirmPassword);

                #endregion

                user.isEmailVerified = false;
                user.isTeacher = false;

                #region Save data to database

                using (ai_databaseEntities de = new ai_databaseEntities())
                {
                    de.User.Add(user);
                    de.SaveChanges();

                    #region Send email to user

                    SendVerificationLinkEmail(user.email, user.ActivationCode.ToString());
                    message = "Registration successfully done. Account activation link has been sent to your email:" + user.email;
                    Status = true;
                    #endregion
                }

                #endregion
            }
            else
            {
                message = "Invalid request";
            }
            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        #region Verify Account

        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool status = false;
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                de.Configuration.ValidateOnSaveEnabled = false;
                var v = de.User.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.isEmailVerified = true;
                    de.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = true;
            return View();
        }

        #endregion

        #region Login

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        #endregion

        #region Login POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin model, string ReturnUrl = "")
        {
            string message = "";
            using (ai_databaseEntities dc = new ai_databaseEntities())
            {
                var v = dc.User.Where(a => a.login == model.login).FirstOrDefault();
                if (v != null)
                {
                    if (!v.isEmailVerified)
                    {
                        ViewBag.Message = "Please verify your email first";
                        return View();
                    }

                    if (string.Compare(Crypto.Hash(model.password), v.password) == 0)
                    {
                        int timeout = model.rememberMe ? 525600 : 20;
                        var ticket = new FormsAuthenticationTicket(model.login, model.rememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            if (v.isTeacher)
                            {
                                return RedirectToAction("IndexTeacher", "Home");
                            }
                            else
                            {
                                return RedirectToAction("IndexStudent", "Home");
                            }
                            
                        }
                    }
                    else
                    {
                        message = "Invalid credential provided";
                    }
                }
                else
                {
                    message = "Invalid credential provided";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        #endregion

        #region Logout

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        #endregion


        [NonAction]
        public bool IsLoginUsed(string login)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                var v = de.User.Where(a => a.login == login).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public bool IsEmailUsed(string email)
        {
            using (ai_databaseEntities de = new ai_databaseEntities())
            {
                var v = de.User.Where(a => a.email == email).FirstOrDefault();
                return v != null;
            }
        }
        [NonAction]
        public void SendVerificationLinkEmail(string email, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("reserveclasses@gmail.com", "Class Reservations");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "makeyourreservation";
            string subject = "Your account is succesfully created";
            string body = "<br/><br/>We are excited to tell you that your account is Successfully created. Please click on the below link to verify your account" +
                "<br/><br/><a href='" + link + "'>" + link + "</a> ";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true 
            })

            smtp.Send(message);
        }
    }
}