using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class ProfileController : Controller
    {
        private class UserProfileExtraData
        {
            public DateTime? NgaySinh { get; set; }
            public string TieuSu { get; set; }
            public string ThanhPho { get; set; }
            public string QuocGia { get; set; }
        }

        private readonly DataClasses1DataContext db = new DataClasses1DataContext(GetConnectionString());

        private static string GetConnectionString()
        {
            var appConnection = ConfigurationManager.ConnectionStrings["QL_DL_LTWConnectionString"];
            if (appConnection != null && !string.IsNullOrWhiteSpace(appConnection.ConnectionString))
            {
                return appConnection.ConnectionString;
            }

            var defaultConnection = ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (defaultConnection != null && !string.IsNullOrWhiteSpace(defaultConnection.ConnectionString))
            {
                return defaultConnection.ConnectionString;
            }

            throw new ConfigurationErrorsException("Missing connection string. Please add 'QL_DL_LTWConnectionString' (or 'DefaultConnection') in Web.config.");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult Update(string fullName, string email, string phone, string avatarUrl, string birthDate, string bio, string city, string country)
        {
            Response.ContentEncoding = System.Text.Encoding.UTF8;

            var sessionUser = Session["nguoiDung"] as NguoiDung;
            if (sessionUser == null)
            {
                Response.StatusCode = 401;
                return Json(new { success = false, message = "Phiên ??ng nh?p ?ã h?t h?n. Vui lòng ??ng nh?p l?i." });
            }

            string normalizedName = (fullName ?? string.Empty).Trim();
            string normalizedEmail = (email ?? string.Empty).Trim();
            string normalizedPhone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            string normalizedAvatar = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
            string normalizedBio = string.IsNullOrWhiteSpace(bio) ? null : bio.Trim();
            string normalizedCity = string.IsNullOrWhiteSpace(city) ? null : city.Trim();
            string normalizedCountry = string.IsNullOrWhiteSpace(country) ? null : country.Trim();

            DateTime? normalizedBirthDate = null;
            if (!string.IsNullOrWhiteSpace(birthDate))
            {
                DateTime parsedBirthDate;
                if (!DateTime.TryParseExact(birthDate.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedBirthDate))
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Ngày sinh không h?p l?." });
                }
                normalizedBirthDate = parsedBirthDate.Date;
            }

            if (string.IsNullOrWhiteSpace(normalizedName) || string.IsNullOrWhiteSpace(normalizedEmail))
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "H? tên và email là b?t bu?c." });
            }

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(normalizedEmail);
                if (!string.Equals(mailAddress.Address, normalizedEmail, StringComparison.OrdinalIgnoreCase))
                {
                    Response.StatusCode = 400;
                    return Json(new { success = false, message = "Email không h?p l?." });
                }
            }
            catch
            {
                Response.StatusCode = 400;
                return Json(new { success = false, message = "Email không h?p l?." });
            }

            var user = db.NguoiDungs.FirstOrDefault(x => x.MaNguoiDung == sessionUser.MaNguoiDung && x.TrangThai);
            if (user == null)
            {
                Response.StatusCode = 404;
                return Json(new { success = false, message = "Không tìm th?y ng??i dùng." });
            }

            bool emailExists = db.NguoiDungs.Any(x => x.Email == normalizedEmail && x.MaNguoiDung != user.MaNguoiDung);
            if (emailExists)
            {
                Response.StatusCode = 409;
                return Json(new { success = false, message = "Email ?ã ???c s? d?ng." });
            }

            user.HoTen = normalizedName;
            user.Email = normalizedEmail;
            user.SoDienThoai = normalizedPhone;
            user.AnhDaiDien = normalizedAvatar;
            user.NgayCapNhat = DateTime.Now;

            db.SubmitChanges();

            try
            {
                db.ExecuteCommand(
                    "UPDATE NguoiDung SET NgaySinh = {0}, TieuSu = {1}, ThanhPho = {2}, QuocGia = {3}, NgayCapNhat = {4} WHERE MaNguoiDung = {5}",
                    normalizedBirthDate,
                    normalizedBio,
                    normalizedCity,
                    normalizedCountry,
                    user.NgayCapNhat,
                    user.MaNguoiDung);
            }
            catch
            {
                Response.StatusCode = 500;
                return Json(new
                {
                    success = false,
                    message = "C? s? d? li?u ch?a có các c?t h? s? m? r?ng. Hãy ch?y script c?p nh?t schema r?i th? l?i."
                });
            }

            var extra = db.ExecuteQuery<UserProfileExtraData>(
                "SELECT NgaySinh, TieuSu, ThanhPho, QuocGia FROM NguoiDung WHERE MaNguoiDung = {0}",
                user.MaNguoiDung).FirstOrDefault();

            Session["nguoiDung"] = user;
            Session["khach"] = user;

            return Json(new
            {
                success = true,
                message = "C\u1EADp nh\u1EADt th\u00F4ng tin th\u00E0nh c\u00F4ng.",
                data = new
                {
                    fullName = user.HoTen,
                    email = user.Email,
                    phone = user.SoDienThoai,
                    avatarUrl = user.AnhDaiDien,
                    birthDate = (extra != null && extra.NgaySinh.HasValue) ? extra.NgaySinh.Value.ToString("yyyy-MM-dd") : string.Empty,
                    bio = extra != null ? (extra.TieuSu ?? string.Empty) : string.Empty,
                    city = extra != null ? (extra.ThanhPho ?? string.Empty) : string.Empty,
                    country = extra != null ? (extra.QuocGia ?? string.Empty) : string.Empty,
                    updatedAt = user.NgayCapNhat.HasValue ? user.NgayCapNhat.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty
                }
            });
        }
    }
}
