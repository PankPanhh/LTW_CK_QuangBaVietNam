using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Helpers;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            // Set breadcrumbs for Index page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Index", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Intro()
        {
            // Intro page doesn't need breadcrumbs
            return View();
        }

        public ActionResult Createschedule()
        {
            // Set breadcrumbs for Create Schedule page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Khám phá", "/Home/Index"),
                new BreadcrumbItem("Tạo lịch trình", "/Home/Createschedule", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Login()
        {
            // Set breadcrumbs for Login page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Đăng nhập", "/Home/Login", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Register()
        {
            // Set breadcrumbs for Register page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Đăng ký", "/Home/Register", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Profile()
        {
            // Set breadcrumbs for Profile page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Hồ sơ cá nhân", "/Home/Profile", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

        public ActionResult Map()
        {
            // Set breadcrumbs for Map page
            var breadcrumbs = new List<BreadcrumbItem>
            {
                new BreadcrumbItem("Bản đồ", "/Home/Map", isActive: true)
            };
            this.SetBreadcrumbs(breadcrumbs);

            return View();
        }

    }
}
