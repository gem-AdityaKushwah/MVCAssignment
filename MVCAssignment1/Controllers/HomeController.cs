using MVCAssignment1.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static MVCAssignment1.Models.Login;

namespace MVCAssignment1.Controllers
{

    public class HomeController : Controller
    {
        private MVCAssignmentDBContext db = new MVCAssignmentDBContext();

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login model,string returnUrl)
        {
            if (ModelState.IsValid)
            {
                // Check if user exists with the given email
                var user = db.UserDetails.FirstOrDefault(u => u.EmailAddress == model.Email);
                if (user != null)
                {
                    // Retrieve the hashed password from the PasswordDetails table
                    var passwordDetail = db.PasswordDetails.FirstOrDefault(pd => pd.UserID == user.UserID);
                    if (passwordDetail != null)
                    {
                        // Hash the entered password
                        var hashedPassword = HashPassword(model.Password);

                        // Compare the hashed password with the stored password
                        if (passwordDetail.Password == hashedPassword)
                        {
                            FormsAuthentication.SetAuthCookie(user.EmailAddress, false);

                            return Redirect(!string.IsNullOrEmpty(returnUrl) ? returnUrl : Url.Action("UserList", "Home"));
                        }
                        else
                        {
                            ViewBag.Message="Invalid password.";
                        }
                    }
                    else
                    {
                        ViewBag.Message= "No password found for this user.";
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid email.");
                }
            }

            // If we got this far, something failed; redisplay the form
            return View(model);
        }




        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Registration(UserDetail model)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (db.UserDetails.Any(u => u.EmailAddress == model.EmailAddress))
                {
                    ModelState.AddModelError("EmailAddress", "Email already registered.");
                    return View(model);
                }

                // Save the user details
                db.UserDetails.Add(model);
                db.SaveChanges();

                var activationLink = Url.Action("CreatePassword", "Home", new { email = model.EmailAddress }, protocol: Request.Url.Scheme);
                 SendEmail(model.EmailAddress, activationLink);

                ViewBag.Message = "Registration successful! Please check your email to verify your account.";
                return View();
            }

            return View(model);
        }


        public ActionResult UserList(string searchTerm = "")
        {
            var users = db.UserDetails.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = users.Where(u => u.FirstName.Contains(searchTerm) ||
                                          u.LastName.Contains(searchTerm) ||
                                          u.EmailAddress.Contains(searchTerm));
            }

            return View(users.ToList()); // Make sure you are passing a List or IEnumerable<UserDetail>
        }

        public ActionResult SearchUsers(string searchTerm)
        {
            // This could be case insensitive depending on your requirement.
            var users = db.UserDetails
                .Where(u => u.FirstName.Contains(searchTerm) ||
                            u.LastName.Contains(searchTerm) ||
                            u.EmailAddress.Contains(searchTerm))
                .ToList();

            return PartialView("_UserListPartial", users); // Return the updated user list
        }



        public ActionResult ShowUserDetails(int id)
        {
            var user = db.UserDetails.FirstOrDefault(u => u.UserID == id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return PartialView("_UserListPartial", user); // Ensure the view returns the correct data
        }



        [HttpGet]
        public ActionResult Edit(int id)
        {
            var user = db.UserDetails.Find(id); // Find user by ID
            if (user == null)
            {
                return HttpNotFound(); // Return 404 if user not found
            }

            return View(user); // Pass the user data to the Edit view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserDetail model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified; // Mark the entity as modified
                db.SaveChanges(); // Save changes to the database
                return RedirectToAction("UserList"); // Redirect back to the User List
            }

            return View(model); // If validation fails, redisplay the form
        }


        private void SendEmail(string email, string activationLink)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(email);
                mail.Subject = "Create Password";
                mail.Body = $"Please click this link to create Password: <a href='{activationLink}'>Create Password</a>";
                mail.IsBodyHtml = true;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new System.Net.NetworkCredential("adityakushwaha6889@gmail.com", "rmzdpjzrzuvxnjyk")
                };

                smtp.Send(mail);
            }
            catch (Exception ex)
            {
                // Log the exception or display it for debugging
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        [HttpGet]
        public ActionResult CreatePassword(string email)
        {
            var model = new CreatePasswordViewModel
            {
                Email = email // Set the email if provided
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePassword(CreatePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Retrieve the user from the database using the provided email
                var user = db.UserDetails.FirstOrDefault(u => u.EmailAddress == model.Email);

                if (user == null)
                {
                    ModelState.AddModelError("", "Invalid user."); // Add an error if the user is not found
                    return View(model);
                }

                // Check if the password meets your validation requirements
                if (!IsPasswordValid(model.Password))
                {
                    ModelState.AddModelError("Password", "Password does not meet complexity requirements."); // Add an error if password is invalid
                    return View(model);
                }

                // Check if the password and confirm password match
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
                    return View(model);
                }

                // Hash the password before saving it to the database
                var hashedPassword = HashPassword(model.Password);

                // Retrieve the existing password detail
                var passwordDetail = db.PasswordDetails.FirstOrDefault(pd => pd.UserID == user.UserID);

                if (passwordDetail != null)
                {
                    // Update the existing password
                    passwordDetail.Password = hashedPassword;
                    db.Entry(passwordDetail).State = EntityState.Modified; // Mark the entity as modified
                }
                else
                {
                    // If no existing password, create a new entry
                    passwordDetail = new PasswordDetails
                    {
                        UserID = user.UserID,
                        Password = hashedPassword
                    };
                    db.PasswordDetails.Add(passwordDetail);
                }

                db.SaveChanges(); // Save changes to the database

                ViewBag.Message = "Password created/updated successfully! You can now log in with the new password.";
            }

            // If model state is invalid, return the same view with validation messages
            return View(model);
        }


        // Optional: Password complexity validation method
        private bool IsPasswordValid(string password)
        {
            // Implement your password validation logic here
            // For example, check for length, special characters, etc.
            return password.Length >= 6; // Simple length check
        }


        // Function to hash the password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }


        public ActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Forgot Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the email exists in the database
                var user = db.UserDetails.FirstOrDefault(u => u.EmailAddress == model.EmailAddress);
                if (user != null)
                {
                    var activationLink = Url.Action("CreatePassword", "Home", new { email = model.EmailAddress }, protocol: Request.Url.Scheme);
                    SendEmail(model.EmailAddress, activationLink); // Call your email sending function

                    ViewBag.Message = "Password reset link has been sent to your email address.";
                }
                else
                {
                    ModelState.AddModelError("", "Email address not found.");
                }
            }

            return View(model);
        }
    }
    
}