using LTW_CK_QuangBaVietNam.Helpers;
using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class LoginController : Controller
    {
        private readonly DataClasses1DataContext db = new DataClasses1DataContext(
            ConfigurationManager.ConnectionStrings["QBConnectionString"].ConnectionString);
       
        //DataClasses1DataContext db = new DataClasses1DataContext();

        public ActionResult Index()
        {
            return RedirectToAction("Login", "Home");
        }

        public ActionResult DangNhap()
        {
            return View("~/Views/Home/Login.cshtml");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DangNhap(string txtTDN, string txtPass)
        //{
        //    if (string.IsNullOrWhiteSpace(txtTDN) || string.IsNullOrWhiteSpace(txtPass))
        //    {
        //        TempData["LoginError"] = "Vui lòng nhập đầy đủ email và mật khẩu.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    NguoiDung nguoiDung = db.NguoiDungs.FirstOrDefault(t =>
        //        t.Email == txtTDN &&
        //        t.MatKhauHash == txtPass &&
        //        t.TrangThai);

        //    if (nguoiDung == null)
        //    {
        //        TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    Session["nguoiDung"] = nguoiDung;
        //    Session["khach"] = nguoiDung;

        //    return RedirectToAction("Index", "Home");
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DangKy(string txtHoTen, string txtEmail, string txtPass, string txtConfirmPass)
        //{
        //    if (string.IsNullOrWhiteSpace(txtHoTen) || string.IsNullOrWhiteSpace(txtEmail) || string.IsNullOrWhiteSpace(txtPass) || string.IsNullOrWhiteSpace(txtConfirmPass))
        //    {
        //        TempData["RegisterError"] = "Vui lòng di?n d?y d? t?t c? các tru?ng.";
        //        return RedirectToAction("Register", "Home");
        //    }

        //    if (txtPass.Length < 6)
        //    {
        //        TempData["RegisterError"] = "M?t kh?u ph?i có ít nh?t 6 ký t?.";
        //        return RedirectToAction("Register", "Home");
        //    }

        //    if (txtPass != txtConfirmPass)
        //    {
        //        TempData["RegisterError"] = "M?t kh?u xác nh?n không kh?p.";
        //        return RedirectToAction("Register", "Home");
        //    }

        //    bool daTonTaiEmail = db.NguoiDungs.Any(t => t.Email == txtEmail);
        //    if (daTonTaiEmail)
        //    {
        //        TempData["RegisterError"] = "Email dã du?c s? d?ng.";
        //        return RedirectToAction("Register", "Home");
        //    }

        //    var nguoiDungMoi = new NguoiDung
        //    {
        //        HoTen = txtHoTen.Trim(),
        //        Email = txtEmail.Trim(),
        //        MatKhauHash = txtPass,
        //        VaiTro = 0,
        //        TrangThai = true,
        //        NgayTao = DateTime.Now
        //    };

        //    db.NguoiDungs.InsertOnSubmit(nguoiDungMoi);
        //    db.SubmitChanges();

        //    TempData["RegisterSuccess"] = "Ðang ký thành công. Vui lòng dang nh?p.";
        //    return RedirectToAction("Login", "Home");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangNhap(string txtTDN, string txtPass, string returnUrl)
        {
            if (string.IsNullOrWhiteSpace(txtTDN) || string.IsNullOrWhiteSpace(txtPass))
            {
                TempData["LoginError"] = "Vui lòng nhập đầy đủ email và mật khẩu.";
                return RedirectToAction("Login", "Home", new { returnUrl = returnUrl });
            }

            string email = txtTDN.Trim();

            var nguoiDung = db.NguoiDungs.FirstOrDefault(x => x.Email == email && x.TrangThai);
            if (nguoiDung == null)
            {
                TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
                return RedirectToAction("Login", "Home", new { returnUrl = returnUrl });
            }

            bool isValid = PasswordHasher.VerifyPassword(txtPass, nguoiDung.MatKhauHash);
            if (!isValid)
            {
                TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
                return RedirectToAction("Login", "Home", new { returnUrl = returnUrl });
            }

            // Upgrade mật khẩu nếu trước đó lưu plain text
            if (!PasswordHasher.IsHashedFormat(nguoiDung.MatKhauHash))
            {
                nguoiDung.MatKhauHash = PasswordHasher.HashPassword(txtPass);
                nguoiDung.NgayCapNhat = DateTime.Now;
                db.SubmitChanges();
            }

            Session["nguoiDung"] = nguoiDung;
            Session["khach"] = nguoiDung;

            // 1) Ưu tiên quay lại trang được yêu cầu (Admin/...)
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // 2) Không có returnUrl thì redirect theo VaiTro
            if (nguoiDung.VaiTro == 1) 
                return RedirectToAction("Index", "Admin");

            return RedirectToAction("Index", "Home");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult DangNhap(string txtTDN, string txtPass)
        //{
        //    if (string.IsNullOrWhiteSpace(txtTDN) || string.IsNullOrWhiteSpace(txtPass))
        //    {
        //        TempData["LoginError"] = "Vui lòng nhập đầy đủ email và mật khẩu.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    string email = txtTDN.Trim();

        //    var nguoiDung = db.NguoiDungs
        //                     .FirstOrDefault(x => x.Email == email && x.TrangThai);

        //    if (nguoiDung == null)
        //    {
        //        TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    // So sánh password (hash)
        //    bool isValid = PasswordHasher.VerifyPassword(txtPass, nguoiDung.MatKhauHash);

        //    if (!isValid)
        //    {
        //        TempData["LoginError"] = "Email hoặc mật khẩu không đúng.";
        //        return RedirectToAction("Login", "Home");
        //    }

        //    // Nếu mật khẩu cũ chưa hash -> tự động upgrade
        //    if (!PasswordHasher.IsHashedFormat(nguoiDung.MatKhauHash))
        //    {
        //        nguoiDung.MatKhauHash = PasswordHasher.HashPassword(txtPass);
        //        nguoiDung.NgayCapNhat = DateTime.Now;
        //        db.SubmitChanges();
        //    }

        //    Session["nguoiDung"] = nguoiDung;
        //    Session["khach"] = nguoiDung;

        //    return RedirectToAction("Index", "Home");
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DangKy(string txtHoTen, string txtEmail, string txtPass, string txtConfirmPass)
        {
            if (string.IsNullOrWhiteSpace(txtHoTen) ||
                string.IsNullOrWhiteSpace(txtEmail) ||
                string.IsNullOrWhiteSpace(txtPass) ||
                string.IsNullOrWhiteSpace(txtConfirmPass))
            {
                TempData["RegisterError"] = "Vui lòng nhập đầy đủ tất cả các trường.";
                return RedirectToAction("Register", "Home");
            }

            if (txtPass.Length < 6)
            {
                TempData["RegisterError"] = "Mật khẩu phải có ít nhất 6 ký tự.";
                return RedirectToAction("Register", "Home");
            }

            if (txtPass != txtConfirmPass)
            {
                TempData["RegisterError"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction("Register", "Home");
            }

            string email = txtEmail.Trim();

            bool daTonTai = db.NguoiDungs.Any(x => x.Email == email);
            if (daTonTai)
            {
                TempData["RegisterError"] = "Email đã được sử dụng.";
                return RedirectToAction("Register", "Home");
            }

            var nguoiDungMoi = new NguoiDung
            {
                HoTen = txtHoTen.Trim(),
                Email = email,
                MatKhauHash = PasswordHasher.HashPassword(txtPass), // 🔥 HASH
                VaiTro = 2, // 1: Admin, 2: User
                TrangThai = true,
                NgayTao = DateTime.Now
            };

            db.NguoiDungs.InsertOnSubmit(nguoiDungMoi);
            db.SubmitChanges();

            TempData["RegisterSuccess"] = "Đăng ký thành công. Vui lòng đăng nhập.";
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public ActionResult Logout()
        {
            Session["nguoiDung"] = null;
            Session["khach"] = null;
            Session.Clear();
            return RedirectToAction("Login", "Home");
        }
    }
}
