using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Web.Mvc;
using System.Web.Security;
using LTW_CK_QuangBaVietNam.Helpers;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("??ng nh?p", "/Account/Login", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            ViewBag.ReturnUrl = returnUrl;
            return View("~/Views/Home/Login.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string email, string password, bool rememberMe = false, string returnUrl = null)
        {
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            password = (password ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                TempData["AuthError"] = "Vui lòng nh?p ??y ?? email và m?t kh?u.";
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }

            string fullName;
            string storedHash;
            string storedSalt;

            using (var connection = new SqlConnection(GetConnectionString()))
            using (var command = new SqlCommand(@"SELECT TOP 1 FullName, PasswordHash, PasswordSalt
                                                  FROM dbo.Users
                                                  WHERE Email = @Email AND IsActive = 1", connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        TempData["AuthError"] = "Email ho?c m?t kh?u không chính xác.";
                        return RedirectToAction("Login", new { returnUrl = returnUrl });
                    }

                    fullName = reader["FullName"].ToString();
                    storedHash = reader["PasswordHash"].ToString();
                    storedSalt = reader["PasswordSalt"].ToString();
                }
            }

            if (!VerifyPassword(password, storedSalt, storedHash))
            {
                TempData["AuthError"] = "Email ho?c m?t kh?u không chính xác.";
                return RedirectToAction("Login", new { returnUrl = returnUrl });
            }

            using (var connection = new SqlConnection(GetConnectionString()))
            using (var updateCommand = new SqlCommand(@"UPDATE dbo.Users
                                                        SET LastLoginAt = GETDATE(),
                                                            UpdatedAt = GETDATE()
                                                        WHERE Email = @Email", connection))
            {
                updateCommand.Parameters.AddWithValue("@Email", email);
                connection.Open();
                updateCommand.ExecuteNonQuery();
            }

            FormsAuthentication.SetAuthCookie(email, rememberMe);
            Session["FullName"] = fullName;

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Profile", "Home");
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("??ng ký", "/Account/Register", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View("~/Views/Home/Register.cshtml");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string fullName, string email, string password, string confirmPassword)
        {
            fullName = (fullName ?? string.Empty).Trim();
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            password = (password ?? string.Empty).Trim();
            confirmPassword = (confirmPassword ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                TempData["AuthError"] = "Vui lòng ?i?n ??y ?? t?t c? các tr??ng.";
                return RedirectToAction("Register");
            }

            if (password.Length < 6)
            {
                TempData["AuthError"] = "M?t kh?u ph?i có ít nh?t 6 ký t?.";
                return RedirectToAction("Register");
            }

            if (!password.Equals(confirmPassword, StringComparison.Ordinal))
            {
                TempData["AuthError"] = "M?t kh?u xác nh?n không kh?p.";
                return RedirectToAction("Register");
            }

            using (var connection = new SqlConnection(GetConnectionString()))
            using (var existsCommand = new SqlCommand("SELECT COUNT(1) FROM dbo.Users WHERE Email = @Email", connection))
            {
                existsCommand.Parameters.AddWithValue("@Email", email);
                connection.Open();

                var exists = Convert.ToInt32(existsCommand.ExecuteScalar()) > 0;
                if (exists)
                {
                    TempData["AuthError"] = "Email ?ã t?n t?i.";
                    return RedirectToAction("Register");
                }
            }

            var salt = GenerateSalt();
            var hash = HashPassword(password, salt);

            using (var connection = new SqlConnection(GetConnectionString()))
            using (var insertCommand = new SqlCommand(@"INSERT INTO dbo.Users (FullName, Email, PasswordHash, PasswordSalt, IsActive, CreatedAt)
                                                        VALUES (@FullName, @Email, @PasswordHash, @PasswordSalt, 1, GETDATE())", connection))
            {
                insertCommand.Parameters.AddWithValue("@FullName", fullName);
                insertCommand.Parameters.AddWithValue("@Email", email);
                insertCommand.Parameters.AddWithValue("@PasswordHash", hash);
                insertCommand.Parameters.AddWithValue("@PasswordSalt", salt);
                connection.Open();
                insertCommand.ExecuteNonQuery();
            }

            TempData["AuthSuccess"] = "??ng ký thành công. Vui lòng ??ng nh?p.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Remove("FullName");
            return RedirectToAction("Index", "Home");
        }

        private static string HashPassword(string password, string saltBase64)
        {
            var salt = Convert.FromBase64String(saltBase64);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                var hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(hash);
            }
        }

        private static bool VerifyPassword(string inputPassword, string storedSalt, string storedHash)
        {
            var inputHash = HashPassword(inputPassword, storedSalt);
            return string.Equals(inputHash, storedHash, StringComparison.Ordinal);
        }

        private static string GenerateSalt()
        {
            var salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            return Convert.ToBase64String(salt);
        }

        private static string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
    }
}
