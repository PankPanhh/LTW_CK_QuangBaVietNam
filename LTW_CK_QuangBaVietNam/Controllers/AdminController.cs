using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class AdminController : Controller
    {

        // GET: /Admin/
        DataClasses1DataContext db =
        new DataClasses1DataContext(
            ConfigurationManager
            .ConnectionStrings["CK_QBVNConnectionString"]
            .ConnectionString
        );
        public ActionResult Index()
        {
            ViewBag.Title = "Dashboard";
            return View();
        }

        // GET: /Admin/DiaDiem
        public ActionResult DiaDiem(string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            filter = (filter ?? "all").ToLower().Trim();
            q = (q ?? "").Trim();
            vung = (vung ?? "all").Trim(); 

            
            var list = (from dd in db.DiaDiems
                        join dm in db.DanhMucs on dd.MaDanhMuc equals dm.MaDanhMuc into gj
                        from dm in gj.DefaultIfEmpty()
                        select new LTW_CK_QuangBaVietNam.Models.DiaDiemRowVM
                        {
                            MaDiaDiem = dd.MaDiaDiem,
                            TenDiaDiem = dd.TenDiaDiem,
                            GioMoCua = dd.GioMoCua,
                            MaDanhMuc = dd.MaDanhMuc,
                            TenDanhMuc = (dm != null ? dm.TenDanhMuc : "(Chưa có)"),
                            VungMien = dd.VungMien,
                            GiaVe = dd.GiaVe,
                            TrangThai = dd.TrangThai,
                            NgayDang = dd.NgayDang
                        }).ToList();

            
            if (filter == "showing") list = list.Where(x => x.TrangThai).ToList();
            else if (filter == "hidden") list = list.Where(x => !x.TrangThai).ToList();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var qNorm = NormalizeText(q);
                list = list.Where(x => NormalizeText(x.TenDiaDiem).Contains(qNorm)).ToList();
            }

           
            if (!string.IsNullOrWhiteSpace(vung) && vung.ToLower() != "all")
            {
                
                var vNorm = NormalizeText(vung); 
                list = list.Where(x => NormalizeText(x.VungMien).Contains(vNorm)).ToList();
            }

            // 4) Lọc danh mục
            if (danhMuc.HasValue)
                list = list.Where(x => x.MaDanhMuc == danhMuc.Value).ToList();

            list = list.OrderByDescending(x => x.NgayDang).ToList();

            ViewBag.Filter = filter;
            ViewBag.Q = q;
            ViewBag.Vung = vung;       
            ViewBag.DanhMuc = danhMuc;
            ViewBag.DanhMucList = db.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList();

            return View(list);
        }

        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            input = input.Trim().ToLowerInvariant();
            var formD = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int i = 0; i < formD.Length; i++)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(formD[i]);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(formD[i]);
            }

            return sb.ToString()
                     .Normalize(NormalizationForm.FormC)
                     .Replace("đ", "d");
        }
        public ActionResult TaoDiaDiem()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThaiDiaDiem(int id, string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd != null)
            {
                dd.TrangThai = !dd.TrangThai;
                db.SubmitChanges();
            }
            return RedirectToAction("DiaDiem", new { filter = filter, q = q, vung = vung, danhMuc = danhMuc });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaDiaDiem(int id, string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd != null)
            {
                dd.TrangThai = false; 
                db.SubmitChanges();
            }

            return RedirectToAction("DiaDiem", new { filter = filter, q = q, vung = vung, danhMuc = danhMuc });
        }

        // GET: /Admin/DanhMuc
        public ActionResult DanhMuc()
        {
            ViewBag.Title = "Quản lý danh mục";
            var list = db.DanhMucs.OrderBy(x => x.MaDanhMuc).ToList();
            return View(list);
        }

        [HttpPost]
      
        public ActionResult ThemDanhMuc(string tenDanhMuc, string moTa)
        {
            tenDanhMuc = (tenDanhMuc ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            if (string.IsNullOrEmpty(tenDanhMuc))
                return RedirectToAction("DanhMuc");

            db.DanhMucs.InsertOnSubmit(new DanhMuc
            {
                TenDanhMuc = tenDanhMuc,
                MoTa = moTa
            });
            db.SubmitChanges();

            return RedirectToAction("DanhMuc");
        }

        [HttpPost]
        public ActionResult SuaDanhMuc(int id, string tenDanhMuc, string moTa)
        {
            tenDanhMuc = (tenDanhMuc ?? "").Trim();
            moTa = string.IsNullOrWhiteSpace(moTa) ? null : moTa.Trim();

            var dm = db.DanhMucs.SingleOrDefault(x => x.MaDanhMuc == id);
            if (dm == null) return HttpNotFound();

            dm.TenDanhMuc = tenDanhMuc;
            dm.MoTa = moTa;
            db.SubmitChanges();

            return RedirectToAction("DanhMuc");
        }

        [HttpPost]
        public ActionResult XoaDanhMuc(int id)
        {
            var dm = db.DanhMucs.SingleOrDefault(x => x.MaDanhMuc == id);
            if (dm != null)
            {
                db.DanhMucs.DeleteOnSubmit(dm);
                db.SubmitChanges();
            }

            return RedirectToAction("DanhMuc");
        }

        // QUẢN LÝ NGƯỜI DÙNG
        public ActionResult NguoiDung(string filter = "all")
        {
            ViewBag.Title = "Quản lý người dùng";
            ViewBag.Filter = filter;

            var ds = db.NguoiDungs.AsQueryable();

            if (filter == "active")
            {
                ds = ds.Where(x => x.TrangThai == true);
            }
            else if (filter == "locked")
            {
                ds = ds.Where(x => x.TrangThai == false);
            }

            return View(ds.ToList());
        }

        // KIỂM DUYỆT ĐÁNH GIÁ
        public ActionResult DanhGia(string filter = "all")
        {
            ViewBag.Title = "Kiểm duyệt đánh giá";
            filter = (filter ?? "all").ToLower();
            ViewBag.Filter = filter;

            var opt = new DataLoadOptions();
            opt.LoadWith<DanhGia>(x => x.NguoiDung);
            opt.LoadWith<DanhGia>(x => x.DiaDiem);
            db.LoadOptions = opt;

            var q = db.DanhGias.AsQueryable();

            if (filter == "pending")
                q = q.Where(x => x.TrangThaiKiemDuyet == false);

            var list = q.OrderByDescending(x => x.NgayGui).ToList();

            return View(list);
        }

        [HttpPost]
        public ActionResult DuyetDanhGia(int id, string filter = "all")
        {
            var dg = db.DanhGias.SingleOrDefault(x => x.MaDanhGia == id);

            if (dg != null)
            {
                dg.TrangThaiKiemDuyet = true;
                db.SubmitChanges();

                //CapNhatDiemTB(dg.MaDiaDiem);
            }

            return RedirectToAction("DanhGia", new { filter });
        }

        [HttpPost]
        public ActionResult XoaDanhGia(int id, string filter = "all")
        {
            var dg = db.DanhGias.SingleOrDefault(x => x.MaDanhGia == id);

            if (dg != null)
            {
                int maDiaDiem = dg.MaDiaDiem;

                db.DanhGias.DeleteOnSubmit(dg);
                db.SubmitChanges();

                //CapNhatDiemTB(maDiaDiem);
            }

            return RedirectToAction("DanhGia", new { filter });
        }

        [HttpPost]
        public ActionResult KhoaNguoiDung(int userId)
        {
            
            return RedirectToAction("DanhGia");
        }

        // BLOG
        public ActionResult Blog()
        {
            ViewBag.Title = "Quản lý bài viết blog";
            return View();
        }

        public ActionResult BanDo()
        {
            ViewBag.Title = "Bản đồ & vị trí";
            return View(); 
        }

        [HttpGet]
        public ActionResult CapNhatToaDo(int? id)
        {
            ViewBag.Title = "Cập nhật tọa độ";

            // Không có id -> quay về trang bản đồ
            if (id == null)
                return RedirectToAction("BanDo");

            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id.Value);

            if (dd == null)
                return HttpNotFound();

            return View(dd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhatToaDo(int id, decimal kinhDo, decimal viDo)
        {
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);

            if (dd == null)
                return HttpNotFound();

            dd.KinhDo = kinhDo;
            dd.ViDo = viDo;

            db.SubmitChanges();

            TempData["msg"] = "Cập nhật tọa độ thành công";
            return RedirectToAction("BanDo");
        }

        // ===== BANNER =====
        public ActionResult Banner(string filter = "all")
        {
            ViewBag.Title = "Quản lý banner trang chủ";

            filter = (filter ?? "all").ToLower();
            ViewBag.Filter = filter;

            var q = db.Banners.AsQueryable();

            if (filter == "showing")
            {
                q = q.Where(x => x.TrangThai == true);
            }
            else if (filter == "hidden")
            {
                q = q.Where(x => x.TrangThai == false);
            }

            var list = q.OrderBy(x => x.ThuTuHienThi).ToList();

            return View(list);
        }

        [HttpGet]
        public ActionResult TaoBanner()
        {
            ViewBag.Title = "Thêm banner";
            return View();
        }

        [HttpPost]
        public ActionResult TaoBanner(string hinhAnh, string lienKet, int thuTuHienThi)
        {
            var banner = new Banner
            {
                HinhAnh = hinhAnh,
                LienKet = lienKet,
                ThuTuHienThi = thuTuHienThi
            };

            db.Banners.InsertOnSubmit(banner);
            db.SubmitChanges();

            return RedirectToAction("Banner");
        }

        [HttpGet]
        public ActionResult SuaBanner(int id)
        {
            ViewBag.Title = "Sửa banner";

            var b = db.Banners
                      .SingleOrDefault(x => x.MaBanner == id);

            if (b == null)
                return HttpNotFound();

            return View(b);
        }

        [HttpPost]
      
        public ActionResult SuaBanner(int id, string hinhAnh, string lienKet, int thuTuHienThi)
        {
            var b = db.Banners
                      .SingleOrDefault(x => x.MaBanner == id);

            if (b == null)
                return HttpNotFound();

            b.HinhAnh = hinhAnh;
            b.LienKet = lienKet;
            b.ThuTuHienThi = thuTuHienThi;

            db.SubmitChanges();

            return RedirectToAction("Banner");
        }

        [HttpPost]
       
        public ActionResult XoaBanner(int id)
        {
            var b = db.Banners
                      .SingleOrDefault(x => x.MaBanner == id);

            if (b != null)
            {
                db.Banners.DeleteOnSubmit(b);
                db.SubmitChanges();
            }

            return RedirectToAction("Banner");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuThuTuBanner(int[] id, int[] thuTu)
        {
            if (id != null &&
                thuTu != null &&
                id.Length == thuTu.Length)
            {
                for (int i = 0; i < id.Length; i++)
                {
                    var b = db.Banners
                              .SingleOrDefault(x => x.MaBanner == id[i]);

                    if (b != null)
                        b.ThuTuHienThi = thuTu[i];
                }

                db.SubmitChanges();
            }

            return RedirectToAction("Banner");
        }

        // Nội dung trang chủ
        public ActionResult NoiDungTrangChu()
        {
            ViewBag.Title = "Nội dung trang chủ";

            var vm = new NoiDungTrangChuVM
            {
                DiaDiems = db.DiaDiems
                             .OrderBy(x => x.TenDiaDiem)
                             .ToList(),

                Banners = db.Banners
                            .OrderBy(x => x.ThuTuHienThi)
                            .ToList()
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LuuNoiBat(int[] featuredIds, int[] allIds, int[] thuTuNoiBat)
        {
            featuredIds = featuredIds ?? new int[0];

            if (allIds != null &&
                thuTuNoiBat != null &&
                allIds.Length == thuTuNoiBat.Length)
            {
                for (int i = 0; i < allIds.Length; i++)
                {
                    int id = allIds[i];

                    var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
                    if (dd == null) continue;

                    bool isFeatured = featuredIds.Contains(id);

                    dd.NoiBat = isFeatured;
                    dd.ThuTuNoiBat = isFeatured
                        ? (int?)thuTuNoiBat[i]
                        : null;
                }

                db.SubmitChanges();
            }

            return RedirectToAction("NoiDungTrangChu");
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult LuuThuTuBanner(int[] bannerIds, int[] thuTuHienThi)
        //{
        //    if (bannerIds != null &&
        //        thuTuHienThi != null &&
        //        bannerIds.Length == thuTuHienThi.Length)
        //    {
        //        for (int i = 0; i < bannerIds.Length; i++)
        //        {
        //            var b = db.Banners
        //                      .SingleOrDefault(x => x.MaBanner == bannerIds[i]);

        //            if (b != null)
        //                b.ThuTuHienThi = thuTuHienThi[i];
        //        }

        //        db.SubmitChanges();
        //    }

        //    return RedirectToAction("NoiDungTrangChu");
        //}

        // THỐNG KÊ
        public ActionResult ThongKe()
        {
            ViewBag.Title = "Thống kê & báo cáo";
            return View();
        }


    }
}
