using LTW_CK_QuangBaVietNam.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.Data.Linq.SqlClient;
using System.Globalization;
using System.IO;
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

        private void LoadDanhMuc()
        {
            ViewBag.DanhMucList = db.DanhMucs.OrderBy(x => x.TenDanhMuc).ToList();
        }

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

                        join a in db.AnhDiaDiems.Where(x => x.LaAnhChinh.GetValueOrDefault())
    on dd.MaDiaDiem equals a.MaDiaDiem into ga
                        from a in ga.DefaultIfEmpty()
                       

                        select new DiaDiemRowVM
                        {
                            MaDiaDiem = dd.MaDiaDiem,
                            TenDiaDiem = dd.TenDiaDiem,
                            MoTaNgan = dd.MoTaNgan,
                            GioMoCua = dd.GioMoCua,
                            GiaVe = dd.GiaVe,
                            VungMien = dd.VungMien,
                            TinhThanh = dd.TinhThanh,
                            MaDanhMuc = dd.MaDanhMuc,
                            TenDanhMuc = (dm != null ? dm.TenDanhMuc : "(Chưa có)"),
                            AnhChinh = (a != null ? a.DuongDanAnh : null),
                            TrangThai = dd.TrangThai ?? true
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

            list = list.OrderBy(x => x.MaDiaDiem).ToList();

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

        private string SaveUploadImage(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLower();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            if (!allowed.Contains(ext))
                throw new Exception("Chỉ cho phép ảnh: jpg, jpeg, png, webp, gif.");

            var folder = Server.MapPath("~/Content/uploads/diadiem");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid().ToString("N") + ext;
            var fullPath = Path.Combine(folder, fileName);

            file.SaveAs(fullPath);

            return "/Content/uploads/diadiem/" + fileName; 
        }

        [HttpGet]
        public ActionResult TaoDiaDiem()
        {
            ViewBag.Title = "Thêm địa điểm";
            LoadDanhMuc();

            var vm = new DiaDiemRowVM
            {
                GiaVe = 0,
                TrangThai = true,
                LaDiemChinh = false
            };

            return View(vm); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TaoDiaDiem(DiaDiemRowVM vm)
        {
            ViewBag.Title = "Thêm địa điểm";
            LoadDanhMuc();

            vm.TenDiaDiem = (vm.TenDiaDiem ?? "").Trim();
            vm.Slug = (vm.Slug ?? "").Trim();

            if (string.IsNullOrWhiteSpace(vm.TenDiaDiem))
                ModelState.AddModelError("TenDiaDiem", "Vui lòng nhập tên địa điểm.");

            if (string.IsNullOrWhiteSpace(vm.Slug))
                ModelState.AddModelError("Slug", "Vui lòng nhập slug.");

            if (!vm.MaDanhMuc.HasValue)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            if (!ModelState.IsValid) return View(vm);

            var dd = new DiaDiem
            {
                TenDiaDiem = vm.TenDiaDiem,
                Slug = vm.Slug,
                MoTaNgan = string.IsNullOrWhiteSpace(vm.MoTaNgan) ? null : vm.MoTaNgan.Trim(),
                MoTaChiTiet = string.IsNullOrWhiteSpace(vm.MoTaChiTiet) ? null : vm.MoTaChiTiet.Trim(),
                MaDanhMuc = vm.MaDanhMuc.Value,
                TinhThanh = string.IsNullOrWhiteSpace(vm.TinhThanh)
    ? null
    : vm.TinhThanh.Trim(),
                GiaVe = vm.GiaVe ?? 0,
                GioMoCua = string.IsNullOrWhiteSpace(vm.GioMoCua) ? null : vm.GioMoCua.Trim(),
                VungMien = string.IsNullOrWhiteSpace(vm.VungMien) ? null : vm.VungMien.Trim(),

                KinhDo = vm.KinhDo,
                ViDo = vm.ViDo,
                DiaChiChiTiet = string.IsNullOrWhiteSpace(vm.DiaChiChiTiet) ? null : vm.DiaChiChiTiet.Trim(),
                SoDienThoai = string.IsNullOrWhiteSpace(vm.SoDienThoai) ? null : vm.SoDienThoai.Trim(),
                Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim(),
                Website = string.IsNullOrWhiteSpace(vm.Website) ? null : vm.Website.Trim(),
                LaDiemChinh = vm.LaDiemChinh,
                TrangThai = vm.TrangThai,
                LuotXem = 0,
                DiemDanhGiaTB = 0,
                NgayDang = DateTime.Now
            };

            db.DiaDiems.InsertOnSubmit(dd);
            db.SubmitChanges(); 

            
            if (vm.AnhChinhFile != null && vm.AnhChinhFile.ContentLength > 0)
            {
                var url = SaveUploadImage(vm.AnhChinhFile);
                db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                {
                    MaDiaDiem = dd.MaDiaDiem,
                    DuongDanAnh = url,
                    LaAnhChinh = true
                });
            }

            if (vm.AnhPhuFiles != null)
            {
                foreach (var f in vm.AnhPhuFiles)
                {
                    if (f == null || f.ContentLength == 0) continue;

                    var url = SaveUploadImage(f);
                    db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                    {
                        MaDiaDiem = dd.MaDiaDiem,
                        DuongDanAnh = url,
                        LaAnhChinh = false
                    });
                }
            }

            db.SubmitChanges();

            TempData["Success"] = "Đã thêm địa điểm.";
            return RedirectToAction("DiaDiem");
        }

        [HttpGet]
        public ActionResult SuaDiaDiem(int id)
        {
            ViewBag.Title = "Sửa địa điểm";
            LoadDanhMuc();

            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd == null) return HttpNotFound();

            var vm = new DiaDiemRowVM
            {
                MaDiaDiem = dd.MaDiaDiem,
                TenDiaDiem = dd.TenDiaDiem,
                Slug = dd.Slug,
                MoTaNgan = dd.MoTaNgan,
                MoTaChiTiet = dd.MoTaChiTiet,
                MaDanhMuc = dd.MaDanhMuc,
                GiaVe = dd.GiaVe,
                GioMoCua = dd.GioMoCua,
                VungMien = dd.VungMien,
                KinhDo = dd.KinhDo,
                ViDo = dd.ViDo,
                DiaChiChiTiet = dd.DiaChiChiTiet,
                SoDienThoai = dd.SoDienThoai,
                Email = dd.Email,
                Website = dd.Website,
                TrangThai = dd.TrangThai ?? true,
                NgayDang = dd.NgayDang,
                TinhThanh = dd.TinhThanh,
                LaDiemChinh = dd.LaDiemChinh ?? false
            };
            ViewBag.Images = db.AnhDiaDiems
    .Where(x => x.MaDiaDiem == id)
    .OrderByDescending(x => x.LaAnhChinh)
    .ToList();


            return View(vm); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaDiaDiem(DiaDiemRowVM vm)
        {
            ViewBag.Title = "Sửa địa điểm";
            LoadDanhMuc();

            vm.TenDiaDiem = (vm.TenDiaDiem ?? "").Trim();
            vm.Slug = (vm.Slug ?? "").Trim();

            if (string.IsNullOrWhiteSpace(vm.TenDiaDiem))
                ModelState.AddModelError("TenDiaDiem", "Vui lòng nhập tên địa điểm.");

            if (string.IsNullOrWhiteSpace(vm.Slug))
                ModelState.AddModelError("Slug", "Vui lòng nhập slug.");

            if (!vm.MaDanhMuc.HasValue)
                ModelState.AddModelError("MaDanhMuc", "Vui lòng chọn danh mục.");

            if (!ModelState.IsValid) return View(vm);

            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == vm.MaDiaDiem);
            if (dd == null) return HttpNotFound();

            dd.TenDiaDiem = vm.TenDiaDiem;
            dd.Slug = vm.Slug;
            dd.MoTaNgan = string.IsNullOrWhiteSpace(vm.MoTaNgan) ? null : vm.MoTaNgan.Trim();
            dd.MoTaChiTiet = string.IsNullOrWhiteSpace(vm.MoTaChiTiet) ? null : vm.MoTaChiTiet.Trim();
            dd.MaDanhMuc = vm.MaDanhMuc.Value;

            dd.GiaVe = vm.GiaVe ?? 0;
            dd.GioMoCua = string.IsNullOrWhiteSpace(vm.GioMoCua) ? null : vm.GioMoCua.Trim();
            dd.VungMien = string.IsNullOrWhiteSpace(vm.VungMien) ? null : vm.VungMien.Trim();
            dd.TinhThanh = string.IsNullOrWhiteSpace(vm.TinhThanh)
    ? null
    : vm.TinhThanh.Trim();
            dd.KinhDo = vm.KinhDo;
            dd.ViDo = vm.ViDo;
            dd.DiaChiChiTiet = string.IsNullOrWhiteSpace(vm.DiaChiChiTiet) ? null : vm.DiaChiChiTiet.Trim();
            dd.SoDienThoai = string.IsNullOrWhiteSpace(vm.SoDienThoai) ? null : vm.SoDienThoai.Trim();
            dd.Email = string.IsNullOrWhiteSpace(vm.Email) ? null : vm.Email.Trim();
            dd.Website = string.IsNullOrWhiteSpace(vm.Website) ? null : vm.Website.Trim();
            dd.TrangThai = vm.TrangThai;
            dd.LaDiemChinh = vm.LaDiemChinh;

            if (vm.AnhChinhFile != null && vm.AnhChinhFile.ContentLength > 0)
            {
                db.ExecuteCommand("UPDATE AnhDiaDiem SET LaAnhChinh = 0 WHERE MaDiaDiem = {0}", dd.MaDiaDiem);

                var url = SaveUploadImage(vm.AnhChinhFile);
                db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                {
                    MaDiaDiem = dd.MaDiaDiem,
                    DuongDanAnh = url,
                    LaAnhChinh = true
                });
            }

            
            if (vm.AnhPhuFiles != null)
            {
                foreach (var f in vm.AnhPhuFiles)
                {
                    if (f == null || f.ContentLength == 0) continue;

                    var url = SaveUploadImage(f);
                    db.AnhDiaDiems.InsertOnSubmit(new AnhDiaDiem
                    {
                        MaDiaDiem = dd.MaDiaDiem,
                        DuongDanAnh = url,
                        LaAnhChinh = false
                    });
                }
            }

            db.SubmitChanges();

            TempData["Success"] = "Đã cập nhật địa điểm.";
            return RedirectToAction("DiaDiem");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DoiTrangThaiDiaDiem(int id, string filter = "all", string q = "", string vung = "all", int? danhMuc = null)
        {
            var dd = db.DiaDiems.SingleOrDefault(x => x.MaDiaDiem == id);
            if (dd != null)
            {
                var current = dd.TrangThai ?? true;
                dd.TrangThai = !current;
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

        //// KIỂM DUYỆT ĐÁNH GIÁ
        //public ActionResult DanhGia(string filter = "all")
        //{
        //    ViewBag.Title = "Kiểm duyệt đánh giá";
        //    filter = (filter ?? "all").ToLower();
        //    ViewBag.Filter = filter;

        //    var opt = new DataLoadOptions();
        //    opt.LoadWith<DanhGia>(x => x.NguoiDung);
        //    opt.LoadWith<DanhGia>(x => x.DiaDiem);
        //    db.LoadOptions = opt;

        //    var q = db.DanhGias.AsQueryable();

        //    if (filter == "pending")
        //        q = q.Where(x => x.TrangThaiKiemDuyet == false);

        //    var list = q.OrderByDescending(x => x.NgayGui).ToList();

        //    return View(list);
        //}

        //[HttpPost]
        //public ActionResult DuyetDanhGia(int id, string filter = "all")
        //{
        //    var dg = db.DanhGias.SingleOrDefault(x => x.MaDanhGia == id);

        //    if (dg != null)
        //    {
        //        dg.TrangThaiKiemDuyet = true;
        //        db.SubmitChanges();

        //        //CapNhatDiemTB(dg.MaDiaDiem);
        //    }

        //    return RedirectToAction("DanhGia", new { filter });
        //}

        //[HttpPost]
        //public ActionResult XoaDanhGia(int id, string filter = "all")
        //{
        //    var dg = db.DanhGias.SingleOrDefault(x => x.MaDanhGia == id);

        //    if (dg != null)
        //    {
        //        int maDiaDiem = dg.MaDiaDiem;

        //        db.DanhGias.DeleteOnSubmit(dg);
        //        db.SubmitChanges();

        //        //CapNhatDiemTB(maDiaDiem);
        //    }

        //    return RedirectToAction("DanhGia", new { filter });
        //}

        //[HttpPost]
        //public ActionResult KhoaNguoiDung(int userId)
        //{
            
        //    return RedirectToAction("DanhGia");
        //}

        //// BLOG
        //public ActionResult Blog()
        //{
        //    ViewBag.Title = "Quản lý bài viết blog";
        //    return View();
        //}

        //public ActionResult BanDo()
        //{
        //    ViewBag.Title = "Bản đồ & vị trí";
        //    return View(); 
        //}


        // THỐNG KÊ
        public ActionResult ThongKe()
        {
            ViewBag.Title = "Thống kê & báo cáo";
            return View();
        }


    }
}
