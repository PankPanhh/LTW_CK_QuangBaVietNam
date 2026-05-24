using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class ItineraryController : Controller
    {
        private static string GetConnectionString()
        {
            var c = System.Configuration.ConfigurationManager.ConnectionStrings["QBConnectionString"];
            if (c != null && !string.IsNullOrWhiteSpace(c.ConnectionString)) return c.ConnectionString;
            var d = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"];
            if (d != null && !string.IsNullOrWhiteSpace(d.ConnectionString)) return d.ConnectionString;
            throw new System.Configuration.ConfigurationErrorsException("Missing connection string.");
        }

        private DataClasses1DataContext db = new DataClasses1DataContext(GetConnectionString());

        // ── Helpers ─────────────────────────────────────────────────────────────
        private NguoiDung CurrentUser => Session["nguoiDung"] as NguoiDung;
        private bool IsLoggedIn => CurrentUser != null;

        private HashSet<int> GetLikedItinerariesSession()
        {
            if (CurrentUser == null) return new HashSet<int>();
            string sessionKey = "LikedItineraries_" + CurrentUser.MaNguoiDung;
            var liked = Session[sessionKey] as HashSet<int>;
            if (liked == null)
            {
                liked = new HashSet<int>();
                Session[sessionKey] = liked;
            }
            return liked;
        }

        /// <summary>
        /// Tính khoảng cách đường chim bay (km) theo công thức Haversine.
        /// </summary>
        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = (lat2 - lat1) * Math.PI / 180;
            var dLon = (lon2 - lon1) * Math.PI / 180;
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                  + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180)
                  * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        /// <summary>
        /// Ước tính khoảng cách đường bộ thực tế bằng cách nhân đường chim bay
        /// với hệ số bù địa hình (winding factor = 1.4).
        /// Hệ số này phù hợp địa hình đồi núi Việt Nam (Đà Lạt, Sapa, Tây Bắc...).
        /// Với địa hình đồng bằng thực tế ~1.2–1.3, núi cao ~1.5–1.7.
        /// </summary>
        private double RoadDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double WindingFactor = 1.4;
            return Haversine(lat1, lon1, lat2, lon2) * WindingFactor;
        }

        private string GetTransportIcon(double km)
        {
            if (km < 2) return "🚶";
            if (km <= 30) return "🛵";
            if (km <= 200) return "🚗";
            return "✈️";
        }

        private string GetTransportLabel(double km)
        {
            if (km < 2) return "Đi bộ";
            if (km <= 30) return "Xe máy";
            if (km <= 200) return "Ô tô";
            return "Máy bay";
        }

        private string GetAirportInfo(double km)
        {
            if (km > 150)
                return $"Khoảng cách {km:F0} km — đề xuất đặt vé máy bay (~{(int)(km / 700 * 60 + 30)} phút bay)";
            return "";
        }

        private object BuildPlaceObj(ChiTietLichTrinh c, double distToNext, string icon, string transport, string airport)
        {
            var dd = c.DiaDiem;
            return new
            {
                idChiTiet = c.MaChiTiet,
                maDiaDiem = c.MaDiaDiem,
                tenDiaDiem = dd?.TenDiaDiem ?? "",
                diaChi = dd?.DiaChiChiTiet ?? "",
                tinhThanh = dd?.TinhThanh ?? "",
                anh = dd?.AnhDiaDiems.FirstOrDefault()?.DuongDanAnh ?? "",
                viDo = dd?.ViDo,
                kinhDo = dd?.KinhDo,
                gioBatDau = c.GioBatDau.ToString(@"hh\:mm"),
                gioKetThuc = c.GioKetThuc.HasValue ? c.GioKetThuc.Value.ToString(@"hh\:mm") : "",
                ghiChu = c.GhiChu ?? "",
                thuTu = c.ThuTu,
                distanceToNext = Math.Round(distToNext, 1),
                transportIcon = icon,
                transportLabel = transport,
                airportInfo = airport
            };
        }

        // ── API: Tạo lịch trình ─────────────────────────────────────────────────
        [HttpPost]
        public ActionResult Create(string tenLichTrinh, string moTa, string ngayBatDau, string ngayKetThuc, string trangThai, string anhBia)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            if (string.IsNullOrWhiteSpace(tenLichTrinh)) return Json(new { success = false, message = "Tên lịch trình không được để trống" });

            if (!DateTime.TryParseExact(ngayBatDau, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime startDate) &&
                !DateTime.TryParse(ngayBatDau, out startDate))
                return Json(new { success = false, message = "Ngày bắt đầu không hợp lệ" });

            if (!DateTime.TryParseExact(ngayKetThuc, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime endDate) &&
                !DateTime.TryParse(ngayKetThuc, out endDate))
                return Json(new { success = false, message = "Ngày kết thúc không hợp lệ" });

            if (endDate < startDate) return Json(new { success = false, message = "Ngày kết thúc phải lớn hơn ngày bắt đầu" });

            int soNgay = (endDate - startDate).Days + 1;

            var lt = new LichTrinh
            {
                TenLichTrinh = tenLichTrinh.Trim(),
                MoTa = moTa?.Trim() ?? "",
                NgayBatDau = startDate,
                NgayKetThuc = endDate,
                TrangThai = trangThai ?? "private",
                MaNguoiDung = CurrentUser.MaNguoiDung,
                SoNgay = soNgay,
                AnhBia = anhBia,
                NgayTao = DateTime.Now,
                LuotXem = 0,
                LuotLike = 0
            };

            db.LichTrinhs.InsertOnSubmit(lt);
            db.SubmitChanges();

            var dayIds = new List<object>();
            for (int i = 1; i <= soNgay; i++)
            {
                var ngay = new NgayLichTrinh
                {
                    MaLichTrinh = lt.MaLichTrinh,
                    ThuTuNgay = i,
                    TieuDe = $"Ngày {i} — {startDate.AddDays(i - 1):dd/MM}"
                };
                db.NgayLichTrinhs.InsertOnSubmit(ngay);
                db.SubmitChanges(); // Save immediately to get the generated MaNgay
                dayIds.Add(new { dayNum = i, dayId = ngay.MaNgay });
            }

            return Json(new { success = true, id = lt.MaLichTrinh, soNgay, dayIds, message = "Đã tạo lịch trình thành công!" });
        }

        // ── API: Thêm ngày ──────────────────────────────────────────────────────
        [HttpPost]
        public ActionResult AddDay(int idLichTrinh, string tenKhuVuc = "")
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Lịch trình không tồn tại" });
            if (lt.MaNguoiDung != CurrentUser.MaNguoiDung) return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa" });

            lt.SoNgay = (lt.SoNgay ?? 0) + 1;
            int num = lt.SoNgay.Value;
            var newDate = lt.NgayBatDau?.AddDays(num - 1);
            string dateStr = newDate.HasValue ? newDate.Value.ToString("dd/MM") : "";

            // Build tiêu đề: "Ngày 1 — Phú Quốc" hoặc "Ngày 1 — 25/12"
            string tieuDeBase = !string.IsNullOrWhiteSpace(tenKhuVuc)
                ? $"Ngày {num} — {tenKhuVuc.Trim()}"
                : $"Ngày {num}" + (dateStr != "" ? $" — {dateStr}" : "");

            var day = new NgayLichTrinh
            {
                MaLichTrinh = lt.MaLichTrinh,
                ThuTuNgay = num,
                TieuDe = tieuDeBase
            };
            db.NgayLichTrinhs.InsertOnSubmit(day);
            db.SubmitChanges();

            return Json(new { success = true, dayId = day.MaNgay, thuTu = num, tieuDe = day.TieuDe, tenKhuVuc = tenKhuVuc?.Trim() ?? "", message = "Đã thêm ngày" });
        }

        // ── API: Cập nhật khu vực chính của ngày ────────────────────────────
        [HttpPost]
        public ActionResult UpdateDayRegion(int idNgay, string tenKhuVuc)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == idNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });

            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ngay.MaLichTrinh);
            if (lt == null || lt.MaNguoiDung != CurrentUser.MaNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa" });

            // Cập nhật TieuDe: giữ "Ngày X" + thay phần sau dấu " — "
            string prefix = $"Ngày {ngay.ThuTuNgay}";
            ngay.TieuDe = !string.IsNullOrWhiteSpace(tenKhuVuc)
                ? $"{prefix} — {tenKhuVuc.Trim()}"
                : prefix;

            db.SubmitChanges();
            return Json(new { success = true, tieuDe = ngay.TieuDe, message = "Đã cập nhật khu vực" });
        }

        // ── API: Thêm địa điểm vào ngày ─────────────────────────────────────────
        [HttpPost]
        public ActionResult AddPlace(int idNgay, int idDiaDiem, string gioBatDau, string gioKetThuc, string ghiChu)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == idNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });

            if (!TimeSpan.TryParse(gioBatDau, out TimeSpan tsBD))
                return Json(new { success = false, message = "Giờ bắt đầu không hợp lệ" });

            TimeSpan? tsKT = null;
            if (!string.IsNullOrWhiteSpace(gioKetThuc) && TimeSpan.TryParse(gioKetThuc, out TimeSpan tsKTParsed))
                tsKT = tsKTParsed;

            var list = db.ChiTietLichTrinhs.Where(x => x.MaNgay == idNgay).ToList();
            if (list.Count >= 10) return Json(new { success = false, message = "Một ngày không thể có quá 10 địa điểm" });
            if (list.Any(c => c.GioBatDau == tsBD)) return Json(new { success = false, warning = "Khung giờ này đã có hoạt động khác, vui lòng chọn giờ khác" });

            var ct = new ChiTietLichTrinh
            {
                MaNgay = idNgay,
                MaDiaDiem = idDiaDiem,
                GioBatDau = tsBD,
                GioKetThuc = tsKT,
                GhiChu = ghiChu?.Trim(),
                ThuTu = list.Count + 1
            };
            db.ChiTietLichTrinhs.InsertOnSubmit(ct);
            db.SubmitChanges();

            // Tính khoảng cách với điểm trước
            double dist = 0; string icon = ""; string transport = ""; string airport = ""; string warnDist = "";
            var prev = list.OrderByDescending(x => x.GioBatDau).FirstOrDefault(x => x.GioBatDau <= tsBD);
            var cur = db.DiaDiems.FirstOrDefault(x => x.MaDiaDiem == idDiaDiem);

            if (prev?.DiaDiem != null && cur != null && prev.DiaDiem.ViDo.HasValue && cur.ViDo.HasValue)
            {
                dist = RoadDistance((double)prev.DiaDiem.ViDo.Value, (double)prev.DiaDiem.KinhDo.Value,
                                   (double)cur.ViDo.Value, (double)cur.KinhDo.Value);
                icon = GetTransportIcon(dist);
                transport = GetTransportLabel(dist);
                airport = GetAirportInfo(dist);
                var timeDiff = (tsBD - prev.GioBatDau).TotalHours;
                if (timeDiff > 0 && dist / timeDiff > 500)
                    warnDist = $"Khoảng cách {dist:F0} km quá xa để di chuyển trong {timeDiff:F1} giờ";
            }

            return Json(new
            {
                success = true,
                id = ct.MaChiTiet,
                message = "Thêm địa điểm thành công",
                distance = Math.Round(dist, 1),
                transportIcon = icon,
                transportLabel = transport,
                airportInfo = airport,
                warningDist = warnDist
            });
        }

        // ── API: Xóa địa điểm ───────────────────────────────────────────────────
        [HttpPost]
        public ActionResult DeletePlace(int idChiTiet)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var ct = db.ChiTietLichTrinhs.FirstOrDefault(x => x.MaChiTiet == idChiTiet);
            if (ct == null) return Json(new { success = false, message = "Không tìm thấy" });
            db.ChiTietLichTrinhs.DeleteOnSubmit(ct);
            db.SubmitChanges();
            return Json(new { success = true, message = "Đã xóa địa điểm" });
        }

        // ── API: Cập nhật trạng thái (public/private) ───────────────────────────
        [HttpPost]
        public ActionResult UpdateStatus(int idLichTrinh, string status)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Lịch trình không tồn tại" });
            if (lt.MaNguoiDung != CurrentUser.MaNguoiDung) return Json(new { success = false, message = "Không có quyền" });
            lt.TrangThai = status;
            db.SubmitChanges();
            return Json(new { success = true, message = status == "public" ? "Đã công khai lịch trình" : "Đã chuyển về riêng tư" });
        }

        // ── API: Lấy timeline một ngày ──────────────────────────────────────────
        [HttpGet]
        public ActionResult GetTimeline(int idNgay)
        {
            var items = db.ChiTietLichTrinhs
                .Where(x => x.MaNgay == idNgay)
                .OrderBy(x => x.GioBatDau)
                .ToList();

            var result = new List<object>();
            for (int i = 0; i < items.Count; i++)
            {
                double dist = 0; string icon = ""; string transport = ""; string airport = "";
                if (i < items.Count - 1)
                {
                    var a = items[i].DiaDiem; var b = items[i + 1].DiaDiem;
                    if (a?.ViDo.HasValue == true && b?.ViDo.HasValue == true)
                    {
                        dist = RoadDistance((double)a.ViDo.Value, (double)a.KinhDo.Value, (double)b.ViDo.Value, (double)b.KinhDo.Value);
                        icon = GetTransportIcon(dist); transport = GetTransportLabel(dist); airport = GetAirportInfo(dist);
                    }
                }
                result.Add(BuildPlaceObj(items[i], dist, icon, transport, airport));
            }
            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lấy chi tiết toàn lịch trình ──────────────────────────────────
        [HttpGet]
        public ActionResult GetDetail(int idLichTrinh)
        {
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Không tìm thấy lịch trình" }, JsonRequestBehavior.AllowGet);

            bool isOwner = IsLoggedIn && lt.MaNguoiDung == CurrentUser.MaNguoiDung;
            if (lt.TrangThai == "private" && !isOwner)
                return Json(new { success = false, message = "Lịch trình này là riêng tư" }, JsonRequestBehavior.AllowGet);

            var days = db.NgayLichTrinhs
                .Where(x => x.MaLichTrinh == idLichTrinh)
                .OrderBy(x => x.ThuTuNgay)
                .ToList()
                .Select(d =>
                {
                    var items = db.ChiTietLichTrinhs.Where(c => c.MaNgay == d.MaNgay).OrderBy(c => c.GioBatDau).ToList();
                    var placeList = new List<object>();
                    for (int i = 0; i < items.Count; i++)
                    {
                        double dist = 0; string icon = ""; string tr = ""; string ap = "";
                        if (i < items.Count - 1)
                        {
                            var a = items[i].DiaDiem; var b = items[i + 1].DiaDiem;
                            if (a?.ViDo.HasValue == true && b?.ViDo.HasValue == true)
                            {
                                dist = RoadDistance((double)a.ViDo.Value, (double)a.KinhDo.Value, (double)b.ViDo.Value, (double)b.KinhDo.Value);
                                icon = GetTransportIcon(dist); tr = GetTransportLabel(dist); ap = GetAirportInfo(dist);
                            }
                        }
                        placeList.Add(BuildPlaceObj(items[i], dist, icon, tr, ap));
                    }
                    // Trích xuất tenKhuVuc từ TieuDe (phần sau " — ")
                    string tenKhuVucDay = "";
                    if (!string.IsNullOrEmpty(d.TieuDe) && d.TieuDe.Contains(" — "))
                    {
                        var parts = d.TieuDe.Split(new string[] { " — " }, 2, System.StringSplitOptions.None);
                        if (parts.Length == 2) tenKhuVucDay = parts[1];
                    }
                    return new { idNgay = d.MaNgay, tieuDe = d.TieuDe, thuTu = d.ThuTuNgay, tenKhuVuc = tenKhuVucDay, places = placeList };
                }).ToList();

            int totalPlaces = days.Sum(d => d.places.Count);
            double totalKm = days.SelectMany(d => d.places)
                .Where(p => (double)((dynamic)p).distanceToNext > 0)
                .Sum(p => (double)((dynamic)p).distanceToNext);

            bool isLiked = false;
            if (IsLoggedIn)
            {
                var likedSet = GetLikedItinerariesSession();
                isLiked = likedSet.Contains(lt.MaLichTrinh);
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    idLichTrinh = lt.MaLichTrinh,
                    tenLichTrinh = lt.TenLichTrinh,
                    moTa = lt.MoTa ?? "",
                    anhBia = lt.AnhBia ?? "",
                    ngayBatDau = lt.NgayBatDau?.ToString("yyyy-MM-dd") ?? "",
                    ngayKetThuc = lt.NgayKetThuc?.ToString("yyyy-MM-dd") ?? "",
                    trangThai = lt.TrangThai,
                    soNgay = lt.SoNgay ?? days.Count,
                    sodiaDiem = totalPlaces,
                    tongKm = Math.Round(totalKm, 1),
                    nguoiTao = lt.NguoiDung?.HoTen ?? "",
                    maNguoiTao = lt.MaNguoiDung,
                    isOwner,
                    luotLike = lt.LuotLike ?? 0,
                    isLiked = isLiked,
                    days
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Tìm kiếm địa điểm (cho modal thêm) ─────────────────────────────
        [HttpGet]
        public ActionResult SearchPlaces(string q, int? maDanhMuc)
        {
            var query = db.DiaDiems.Where(d => d.TrangThai == true);
            if (!string.IsNullOrWhiteSpace(q))
                query = query.Where(d => d.TenDiaDiem.Contains(q) || (d.TinhThanh != null && d.TinhThanh.Contains(q)));
            if (maDanhMuc.HasValue)
                query = query.Where(d => d.MaDanhMuc == maDanhMuc.Value);

            var result = query.Take(20).ToList().Select(d => new
            {
                id = d.MaDiaDiem,
                ten = d.TenDiaDiem,
                tinhThanh = d.TinhThanh ?? "",
                diaChi = d.DiaChiChiTiet ?? "",
                anh = d.AnhDiaDiems.FirstOrDefault()?.DuongDanAnh ?? "",
                viDo = d.ViDo,
                kinhDo = d.KinhDo,
                maDanhMuc = d.MaDanhMuc
            });
            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lấy danh mục ───────────────────────────────────────────────────
        [HttpGet]
        public ActionResult GetCategories()
        {
            var cats = db.DanhMucs.Select(c => new { id = c.MaDanhMuc, ten = c.TenDanhMuc }).ToList();
            return Json(new { success = true, data = cats }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Clone lịch trình ────────────────────────────────────────────────
        [HttpPost]
        public ActionResult Clone(int idLichTrinh)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var src = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (src == null || (src.TrangThai == "private" && src.MaNguoiDung != CurrentUser.MaNguoiDung))
                return Json(new { success = false, message = "Không tìm thấy lịch trình" });

            var clone = new LichTrinh
            {
                TenLichTrinh = "[Bản sao] " + src.TenLichTrinh,
                MoTa = src.MoTa,
                NgayBatDau = src.NgayBatDau,
                NgayKetThuc = src.NgayKetThuc,
                TrangThai = "private",
                MaNguoiDung = CurrentUser.MaNguoiDung,
                SoNgay = src.SoNgay,
                AnhBia = src.AnhBia,
                NgayTao = DateTime.Now,
                LuotXem = 0,
                LuotLike = 0
            };
            db.LichTrinhs.InsertOnSubmit(clone);
            db.SubmitChanges();

            foreach (var day in src.NgayLichTrinhs.OrderBy(d => d.ThuTuNgay))
            {
                var newDay = new NgayLichTrinh
                {
                    MaLichTrinh = clone.MaLichTrinh,
                    ThuTuNgay = day.ThuTuNgay,
                    TieuDe = day.TieuDe
                };
                db.NgayLichTrinhs.InsertOnSubmit(newDay);
                db.SubmitChanges();
                foreach (var ct in day.ChiTietLichTrinhs)
                {
                    db.ChiTietLichTrinhs.InsertOnSubmit(new ChiTietLichTrinh
                    {
                        MaNgay = newDay.MaNgay,
                        MaDiaDiem = ct.MaDiaDiem,
                        GioBatDau = ct.GioBatDau,
                        GioKetThuc = ct.GioKetThuc,
                        GhiChu = ct.GhiChu,
                        ThuTu = ct.ThuTu
                    });
                }
                db.SubmitChanges();
            }
            return Json(new { success = true, id = clone.MaLichTrinh, message = "Đã nhân bản lịch trình" });
        }
        // ── API: Danh sách Lịch trình của tôi ──────────────────────────────────
        [HttpGet]
        public ActionResult GetMySchedules()
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

            var list = db.LichTrinhs.Where(x => x.MaNguoiDung == CurrentUser.MaNguoiDung)
                        .OrderByDescending(x => x.NgayTao)
                        .ToList()
                        .Select(x => new {
                            id = x.MaLichTrinh,
                            title = x.TenLichTrinh,
                            days = x.SoNgay,
                            description = x.MoTa ?? "",
                            image = string.IsNullOrEmpty(x.AnhBia) ? "https://images.unsplash.com/photo-1528127269322-539801943592?w=600" : x.AnhBia,
                            status = x.TrangThai
                        }).ToList();

            return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Khám phá lịch trình cộng đồng ─────────────────────────────────
        [HttpGet]
        public ActionResult GetPublicSchedules(string filter = "all", string sort = "latest")
        {
            var query = db.LichTrinhs.Where(x => x.TrangThai == "public");

            if (filter == "3days") query = query.Where(x => x.SoNgay == 3);
            else if (filter == "5days") query = query.Where(x => x.SoNgay == 5);
            else if (filter == "7days") query = query.Where(x => x.SoNgay >= 7);

            var list = query.OrderByDescending(x => x.NgayTao).ToList();

            var likedSet = GetLikedItinerariesSession();
            var result = list.Select(lt => (object)new
            {
                id = lt.MaLichTrinh,
                title = lt.TenLichTrinh,
                days = lt.SoNgay,
                description = lt.MoTa ?? "",
                image = string.IsNullOrEmpty(lt.AnhBia) ? "https://images.unsplash.com/photo-1540611025311-01df3cef54b5?w=600" : lt.AnhBia,
                author = lt.NguoiDung?.HoTen ?? "Người dùng",
                authorInitial = (lt.NguoiDung?.HoTen ?? "N").Substring(0, 1).ToUpper(),
                likes = lt.LuotLike ?? 0,
                isLiked = IsLoggedIn && likedSet.Contains(lt.MaLichTrinh),
                ngayTao = lt.NgayTao
            }).ToList();

            if (sort == "popular") result = result.OrderByDescending(x => (int)((dynamic)x).likes).ToList();
            else result = result.OrderByDescending(x => (DateTime?)((dynamic)x).ngayTao).ToList();

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lịch trình đã lưu ─────────────────────────────────────────────
        [HttpGet]
        public ActionResult GetSavedSchedules()
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

            var result = new List<object>();
            using (var conn = new System.Data.SqlClient.SqlConnection(GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT l.MaLichTrinh, l.TenLichTrinh, l.SoNgay, l.MoTa, l.AnhBia, nd.HoTen
                    FROM LuuLichTrinh ll
                    JOIN LichTrinh l ON ll.MaLichTrinh = l.MaLichTrinh
                    JOIN NguoiDung nd ON l.MaNguoiDung = nd.MaNguoiDung
                    WHERE ll.MaNguoiDung = @u
                    ORDER BY ll.NgayLuu DESC";

                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@u", CurrentUser.MaNguoiDung);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new
                            {
                                id = Convert.ToInt32(reader["MaLichTrinh"]),
                                title = reader["TenLichTrinh"].ToString(),
                                days = Convert.ToInt32(reader["SoNgay"]),
                                description = reader["MoTa"].ToString(),
                                image = string.IsNullOrEmpty(reader["AnhBia"].ToString()) ? "https://images.unsplash.com/photo-1583417319070-4a69db38a482?w=600" : reader["AnhBia"].ToString(),
                                author = reader["HoTen"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lưu / Hủy lưu lịch trình ──────────────────────────────────────
        [HttpPost]
        public ActionResult ToggleSave(int idLichTrinh)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập để lưu lịch trình." });

            bool isSaved = false;
            using (var conn = new System.Data.SqlClient.SqlConnection(GetConnectionString()))
            {
                conn.Open();

                // Check if already saved
                int count = 0;
                using (var cmd = new System.Data.SqlClient.SqlCommand("SELECT COUNT(1) FROM LuuLichTrinh WHERE MaNguoiDung = @u AND MaLichTrinh = @lt", conn))
                {
                    cmd.Parameters.AddWithValue("@u", CurrentUser.MaNguoiDung);
                    cmd.Parameters.AddWithValue("@lt", idLichTrinh);
                    count = Convert.ToInt32(cmd.ExecuteScalar());
                }

                if (count > 0)
                {
                    // Remove save
                    using (var cmd = new System.Data.SqlClient.SqlCommand("DELETE FROM LuuLichTrinh WHERE MaNguoiDung = @u AND MaLichTrinh = @lt; UPDATE LichTrinh SET LuotLuu = ISNULL(LuotLuu,0) - 1 WHERE MaLichTrinh = @lt;", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", CurrentUser.MaNguoiDung);
                        cmd.Parameters.AddWithValue("@lt", idLichTrinh);
                        cmd.ExecuteNonQuery();
                    }
                    isSaved = false;
                }
                else
                {
                    // Add save
                    using (var cmd = new System.Data.SqlClient.SqlCommand("INSERT INTO LuuLichTrinh (MaNguoiDung, MaLichTrinh) VALUES (@u, @lt); UPDATE LichTrinh SET LuotLuu = ISNULL(LuotLuu,0) + 1 WHERE MaLichTrinh = @lt;", conn))
                    {
                        cmd.Parameters.AddWithValue("@u", CurrentUser.MaNguoiDung);
                        cmd.Parameters.AddWithValue("@lt", idLichTrinh);
                        cmd.ExecuteNonQuery();
                    }
                    isSaved = true;
                }
            }

            return Json(new { success = true, isSaved = isSaved, message = isSaved ? "Đã lưu lịch trình vào bộ sưu tập." : "Đã bỏ lịch trình." });
        }

        // ── API: Lấy danh sách khu vực (TinhThanh) có địa điểm ──────────────
        [HttpGet]
        public ActionResult GetRegions()
        {
            var regions = db.DiaDiems
                .Where(d => d.TrangThai == true && d.TinhThanh != null && d.TinhThanh != "")
                .GroupBy(d => d.TinhThanh)
                .Select(g => new
                {
                    name = g.Key,
                    count = g.Count(),
                    anh = g.SelectMany(d => d.AnhDiaDiems).Select(a => a.DuongDanAnh).FirstOrDefault()
                })
                .OrderByDescending(x => x.count)
                .ToList();

            return Json(new { success = true, data = regions }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lấy địa điểm nổi bật theo khu vực ─────────────────────────
        [HttpGet]
        public ActionResult GetPlacesByRegion(string region, int limit = 20)
        {
            if (string.IsNullOrWhiteSpace(region))
                return Json(new { success = false, message = "Vui lòng chọn khu vực" }, JsonRequestBehavior.AllowGet);

            var places = db.DiaDiems
                .Where(d => d.TrangThai == true && d.TinhThanh != null && d.TinhThanh.Contains(region))
                .OrderByDescending(d => d.DiemDanhGiaTB)
                .ThenByDescending(d => d.LuotXem)
                .Take(limit)
                .ToList()
                .Select(d => new
                {
                    id = d.MaDiaDiem,
                    ten = d.TenDiaDiem,
                    tinhThanh = d.TinhThanh ?? "",
                    diaChi = d.DiaChiChiTiet ?? "",
                    moTa = d.MoTaNgan ?? "",
                    anh = d.AnhDiaDiems.Where(a => a.LaAnhChinh == true).Select(a => a.DuongDanAnh).FirstOrDefault()
                          ?? d.AnhDiaDiems.Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                    viDo = d.ViDo,
                    kinhDo = d.KinhDo,
                    danhMuc = d.DanhMuc?.TenDanhMuc ?? "",
                    diemDanhGia = d.DiemDanhGiaTB
                })
                .ToList();

            return Json(new { success = true, data = places }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Nearby cho Schedule (trả thêm khoảng cách) ─────────────────
        [HttpGet]
        public ActionResult GetNearbyForSchedule(int maDiaDiem, int limit = 6)
        {
            var current = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == maDiaDiem);
            if (current == null)
                return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);

            var nearbyPlaces = db.DiaDiems
                .Where(d => d.TrangThai == true && d.MaDiaDiem != maDiaDiem
                    && d.TinhThanh == current.TinhThanh)
                .OrderByDescending(d => d.DiemDanhGiaTB)
                .Take(limit)
                .ToList()
                .Select(d =>
                {
                    double dist = 0;
                    if (current.ViDo.HasValue && d.ViDo.HasValue)
                        dist = RoadDistance((double)current.ViDo, (double)current.KinhDo, (double)d.ViDo, (double)d.KinhDo);

                    return new
                    {
                        id = d.MaDiaDiem,
                        ten = d.TenDiaDiem,
                        tinhThanh = d.TinhThanh ?? "",
                        anh = d.AnhDiaDiems.Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        khoangCach = Math.Round(dist, 1),
                        viDo = d.ViDo,
                        kinhDo = d.KinhDo
                    };
                })
                .OrderBy(x => x.khoangCach)
                .ToList();

            return Json(new { success = true, data = nearbyPlaces }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Xóa lịch trình ─────────────────────────────────────────────
        [HttpPost]
        public ActionResult DeleteSchedule(int idLichTrinh)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Không tìm thấy" });
            if (lt.MaNguoiDung != CurrentUser.MaNguoiDung) return Json(new { success = false, message = "Không có quyền" });

            // Delete details -> days -> schedule
            var days = db.NgayLichTrinhs.Where(d => d.MaLichTrinh == idLichTrinh).ToList();
            foreach (var day in days)
            {
                var details = db.ChiTietLichTrinhs.Where(c => c.MaNgay == day.MaNgay).ToList();
                db.ChiTietLichTrinhs.DeleteAllOnSubmit(details);
            }
            db.NgayLichTrinhs.DeleteAllOnSubmit(days);
            db.LichTrinhs.DeleteOnSubmit(lt);
            db.SubmitChanges();

            return Json(new { success = true, message = "Đã xóa lịch trình" });
        }

        // ── API: Thêm địa điểm với tự động gán giờ ─────────────────────────
        [HttpPost]
        public ActionResult AddPlaceAuto(int idNgay, int idDiaDiem, string ghiChu)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == idNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });

            var list = db.ChiTietLichTrinhs.Where(x => x.MaNgay == idNgay).OrderBy(x => x.GioBatDau).ToList();
            if (list.Count >= 10) return Json(new { success = false, message = "Tối đa 10 địa điểm/ngày" });

            // Auto-assign time: start 08:00, each next +2h30m
            TimeSpan startTime = new TimeSpan(8, 0, 0);
            if (list.Count > 0)
            {
                var lastEnd = list.Last().GioKetThuc ?? list.Last().GioBatDau.Add(new TimeSpan(2, 0, 0));
                startTime = lastEnd.Add(new TimeSpan(0, 30, 0)); // 30 min gap
                if (startTime >= new TimeSpan(23, 0, 0)) startTime = new TimeSpan(22, 0, 0);
            }
            TimeSpan endTime = startTime.Add(new TimeSpan(2, 0, 0));

            var ct = new ChiTietLichTrinh
            {
                MaNgay = idNgay,
                MaDiaDiem = idDiaDiem,
                GioBatDau = startTime,
                GioKetThuc = endTime,
                GhiChu = ghiChu?.Trim(),
                ThuTu = list.Count + 1
            };
            db.ChiTietLichTrinhs.InsertOnSubmit(ct);
            db.SubmitChanges();

            // Calculate distance to previous
            double dist = 0; string icon = ""; string transport = ""; string airport = "";
            if (list.Count > 0)
            {
                var prev = list.Last().DiaDiem;
                var cur = db.DiaDiems.FirstOrDefault(x => x.MaDiaDiem == idDiaDiem);
                if (prev?.ViDo.HasValue == true && cur?.ViDo.HasValue == true)
                {
                    dist = RoadDistance((double)prev.ViDo, (double)prev.KinhDo, (double)cur.ViDo, (double)cur.KinhDo);
                    icon = GetTransportIcon(dist);
                    transport = GetTransportLabel(dist);
                    airport = GetAirportInfo(dist);
                }
            }

            return Json(new
            {
                success = true,
                id = ct.MaChiTiet,
                message = "Đã thêm địa điểm",
                gioBatDau = startTime.ToString(@"hh\:mm"),
                gioKetThuc = endTime.ToString(@"hh\:mm"),
                distance = Math.Round(dist, 1),
                transportIcon = icon,
                transportLabel = transport,
                airportInfo = airport
            });
        }

        // ── API: Cập nhật giờ địa điểm ──────────────────────────────────────
        [HttpPost]
        public ActionResult UpdateTime(int idChiTiet, string gioBatDau, string gioKetThuc)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });
            var ct = db.ChiTietLichTrinhs.FirstOrDefault(x => x.MaChiTiet == idChiTiet);
            if (ct == null) return Json(new { success = false, message = "Không tìm thấy" });

            // Verify ownership
            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == ct.MaNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ngay.MaLichTrinh);
            if (lt == null || lt.MaNguoiDung != CurrentUser.MaNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa" });

            if (!TimeSpan.TryParse(gioBatDau, out TimeSpan tsBD))
                return Json(new { success = false, message = "Giờ bắt đầu không hợp lệ" });

            ct.GioBatDau = tsBD;
            if (!string.IsNullOrWhiteSpace(gioKetThuc) && TimeSpan.TryParse(gioKetThuc, out TimeSpan tsKT))
                ct.GioKetThuc = tsKT;

            db.SubmitChanges();
            return Json(new { success = true, message = "Đã cập nhật giờ thành công" });
        }

        // ── API: Sắp xếp lại thứ tự địa điểm (sau drag-drop) ──────────────
        [HttpPost]
        public ActionResult ReorderPlaces(int idNgay, string orderJson)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == idNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ngay.MaLichTrinh);
            if (lt == null || lt.MaNguoiDung != CurrentUser.MaNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền" });

            // orderJson: "[{id:1,thuTu:1},{id:2,thuTu:2}]"
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var orders = serializer.Deserialize<List<Dictionary<string, int>>>(orderJson);

            foreach (var item in orders)
            {
                int id = item["id"];
                int thuTu = item["thuTu"];
                var ct = db.ChiTietLichTrinhs.FirstOrDefault(x => x.MaChiTiet == id && x.MaNgay == idNgay);
                if (ct != null) ct.ThuTu = thuTu;
            }
            db.SubmitChanges();
            return Json(new { success = true, message = "Đã cập nhật thứ tự" });
        }

        // ── API: Tối ưu tuyến đường (nearest neighbor) ──────────────────────
        [HttpPost]
        public ActionResult OptimizeRoute(int idNgay)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" });

            var ngay = db.NgayLichTrinhs.FirstOrDefault(x => x.MaNgay == idNgay);
            if (ngay == null) return Json(new { success = false, message = "Ngày không tồn tại" });
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == ngay.MaLichTrinh);
            if (lt == null || lt.MaNguoiDung != CurrentUser.MaNguoiDung)
                return Json(new { success = false, message = "Bạn không có quyền" });

            var places = db.ChiTietLichTrinhs.Where(x => x.MaNgay == idNgay).ToList();
            if (places.Count < 2) return Json(new { success = true, message = "Không đủ địa điểm để tối ưu" });

            // Nearest neighbor algorithm
            var remaining = places.ToList();
            var sorted = new List<ChiTietLichTrinh>();
            var current = remaining.First();
            remaining.Remove(current);
            sorted.Add(current);

            while (remaining.Count > 0)
            {
                var cur = current.DiaDiem;
                ChiTietLichTrinh nearest = null;
                double minDist = double.MaxValue;

                foreach (var candidate in remaining)
                {
                    var cand = candidate.DiaDiem;
                    if (cur?.ViDo.HasValue == true && cand?.ViDo.HasValue == true)
                    {
                        double d = Haversine((double)cur.ViDo, (double)cur.KinhDo,
                                            (double)cand.ViDo, (double)cand.KinhDo);
                        if (d < minDist) { minDist = d; nearest = candidate; }
                    }
                    else if (nearest == null) nearest = candidate;
                }

                if (nearest == null) nearest = remaining.First();
                remaining.Remove(nearest);
                sorted.Add(nearest);
                current = nearest;
            }

            // Update order + reassign times starting 08:00
            TimeSpan t = new TimeSpan(8, 0, 0);
            for (int i = 0; i < sorted.Count; i++)
            {
                sorted[i].ThuTu = i + 1;
                sorted[i].GioBatDau = t;
                sorted[i].GioKetThuc = t.Add(new TimeSpan(2, 0, 0));
                t = sorted[i].GioKetThuc.Value.Add(new TimeSpan(0, 30, 0));
                if (t >= new TimeSpan(23, 0, 0)) t = new TimeSpan(22, 0, 0);
            }
            db.SubmitChanges();

            return Json(new { success = true, message = "Đã tối ưu tuyến đường thành công!" });
        }

        // ── API: Thông tin chuyến bay (giả lập) ─────────────────────────────
        [HttpGet]
        public ActionResult GetFlightInfo(int fromPlaceId, int toPlaceId)
        {
            var from = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == fromPlaceId);
            var to = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == toPlaceId);

            if (from == null || to == null)
                return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);

            double dist = 0;
            if (from.ViDo.HasValue && to.ViDo.HasValue)
                dist = RoadDistance((double)from.ViDo, (double)from.KinhDo, (double)to.ViDo, (double)to.KinhDo);

            // Airport lookup by province
            var airportMap = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "Hồ Chí Minh", "Tân Sơn Nhất (SGN)" }, { "TP.HCM", "Tân Sơn Nhất (SGN)" },
                { "Hà Nội", "Nội Bài (HAN)" }, { "Đà Nẵng", "Đà Nẵng (DAD)" },
                { "Phú Quốc", "Phú Quốc (PQC)" }, { "Nha Trang", "Cam Ranh (CXR)" },
                { "Khánh Hòa", "Cam Ranh (CXR)" }, { "Đà Lạt", "Liên Khương (DLI)" },
                { "Lâm Đồng", "Liên Khương (DLI)" }, { "Cần Thơ", "Cần Thơ (VCA)" },
                { "Huế", "Phú Bài (HUI)" }, { "Thừa Thiên Huế", "Phú Bài (HUI)" },
                { "Hải Phòng", "Cát Bi (HPH)" }, { "Quy Nhơn", "Phù Cát (UIH)" },
                { "Bình Định", "Phù Cát (UIH)" }, { "Pleiku", "Pleiku (PXU)" },
                { "Gia Lai", "Pleiku (PXU)" }, { "Buôn Ma Thuột", "Buôn Ma Thuột (BMV)" }
            };

            string fromAirport = "Sân bay quốc tế";
            string toAirport = "Sân bay quốc tế";
            foreach (var kv in airportMap)
                if (!string.IsNullOrEmpty(from.TinhThanh) && from.TinhThanh.Contains(kv.Key)) { fromAirport = kv.Value; break; }
            foreach (var kv in airportMap)
                if (!string.IsNullOrEmpty(to.TinhThanh) && to.TinhThanh.Contains(kv.Key)) { toAirport = kv.Value; break; }

            int flightMinutes = (int)(dist / 700 * 60) + 30;

            return Json(new
            {
                success = true,
                data = new
                {
                    fromPlace = from.TenDiaDiem,
                    toPlace = to.TenDiaDiem,
                    fromAirport,
                    toAirport,
                    distance = Math.Round(dist, 0),
                    flightDuration = $"{flightMinutes / 60}h{flightMinutes % 60:D2}m",
                    estimatedCost = dist > 500 ? "1.500.000 – 3.500.000 đ" : "800.000 – 2.000.000 đ",
                    note = dist > 150 ? $"Khoảng cách {dist:F0} km — nên đặt vé máy bay" : ""
                }
            }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Like / Unlike lịch trình ────────────────────────────────────
        [HttpPost]
        public ActionResult ToggleLike(int idLichTrinh)
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập để thích lịch trình" });

            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Không tìm thấy lịch trình" });

            var likedSet = GetLikedItinerariesSession();
            bool isLiked;
            if (likedSet.Contains(idLichTrinh))
            {
                likedSet.Remove(idLichTrinh);
                lt.LuotLike = Math.Max(0, (lt.LuotLike ?? 0) - 1);
                isLiked = false;
            }
            else
            {
                likedSet.Add(idLichTrinh);
                lt.LuotLike = (lt.LuotLike ?? 0) + 1;
                isLiked = true;
            }

            try
            {
                db.SubmitChanges();
                return Json(new { success = true, isLiked = isLiked, likes = lt.LuotLike ?? 0, message = isLiked ? "Đã thích lịch trình" : "Đã bỏ thích" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi cập nhật: " + ex.Message });
            }
        }

        // ── API: Lấy danh sách lịch trình của tôi (để thêm địa điểm) ────────
        [HttpGet]
        public ActionResult GetMyScheduleList()
        {
            if (!IsLoggedIn) return Json(new { success = false, message = "Vui lòng đăng nhập" }, JsonRequestBehavior.AllowGet);

            var list = db.LichTrinhs.Where(x => x.MaNguoiDung == CurrentUser.MaNguoiDung)
                        .OrderByDescending(x => x.NgayTao)
                        .Select(x => new
                        {
                            id = x.MaLichTrinh,
                            title = x.TenLichTrinh,
                            days = x.SoNgay,
                            status = x.TrangThai,
                            ngayList = x.NgayLichTrinhs.OrderBy(n => n.ThuTuNgay).Select(n => new { n.MaNgay, n.TieuDe, n.ThuTuNgay }).ToList()
                        }).ToList();

            return Json(new { success = true, data = list }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Lấy danh sách lịch trình cộng đồng theo filter tag ─────────
        [HttpGet]
        public ActionResult FilterByTag(string tag = "all", string sort = "latest")
        {
            var query = db.LichTrinhs.Where(x => x.TrangThai == "public");

            // Tag filter — filter by MoTa contains keyword or TenLichTrinh
            if (!string.IsNullOrEmpty(tag) && tag != "all")
            {
                query = query.Where(x =>
                    (x.MoTa != null && x.MoTa.Contains(tag)) ||
                    (x.TenLichTrinh != null && x.TenLichTrinh.Contains(tag)));
            }

            var list = query.OrderByDescending(x => x.NgayTao).ToList();

            var likedSet = GetLikedItinerariesSession();
            var result = list.Select(lt => (object)new
            {
                id = lt.MaLichTrinh,
                title = lt.TenLichTrinh,
                days = lt.SoNgay,
                description = lt.MoTa ?? "",
                image = string.IsNullOrEmpty(lt.AnhBia) ? "https://images.unsplash.com/photo-1540611025311-01df3cef54b5?w=600" : lt.AnhBia,
                author = lt.NguoiDung?.HoTen ?? "Người dùng",
                authorInitial = (lt.NguoiDung?.HoTen ?? "N").Substring(0, 1).ToUpper(),
                likes = lt.LuotLike ?? 0,
                isLiked = IsLoggedIn && likedSet.Contains(lt.MaLichTrinh),
                ngayTao = lt.NgayTao
            }).ToList();

            if (sort == "popular") result = result.OrderByDescending(x => (int)((dynamic)x).likes).ToList();
            else result = result.OrderByDescending(x => (DateTime?)((dynamic)x).ngayTao).ToList();

            return Json(new { success = true, data = result }, JsonRequestBehavior.AllowGet);
        }

        // ── API: Chia sẻ lịch trình (trả về link) ───────────────────────────
        [HttpGet]
        public ActionResult GetShareLink(int idLichTrinh)
        {
            var lt = db.LichTrinhs.FirstOrDefault(x => x.MaLichTrinh == idLichTrinh);
            if (lt == null) return Json(new { success = false, message = "Không tìm thấy lịch trình" }, JsonRequestBehavior.AllowGet);

            string baseUrl = Request.Url.GetLeftPart(UriPartial.Authority);
            string link = $"{baseUrl}/Home/DetailSchedule?id={idLichTrinh}";
            return Json(new { success = true, link, title = lt.TenLichTrinh }, JsonRequestBehavior.AllowGet);
        }

        // ── View: Trang chi tiết lịch trình (public URL) ─────────────────────
        [HttpGet]
        public ActionResult DetailSchedule(int id)
        {
            ViewBag.ScheduleId = id;
            return View();
        }
    }
}
