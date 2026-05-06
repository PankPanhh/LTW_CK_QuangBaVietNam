using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Controllers
{
    public class MapController : Controller
    {
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

            throw new ConfigurationErrorsException("Missing connection string.");
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetLocations(int? maDanhMuc = null, string categoryName = null)
        {
            try
            {
                var query = db.DiaDiems.Where(d => d.TrangThai == true);

                if (maDanhMuc.HasValue && maDanhMuc > 0)
                {
                    query = query.Where(d => d.MaDanhMuc == maDanhMuc.Value);
                }
                else if (!string.IsNullOrEmpty(categoryName))
                {
                    query = query.Where(d => d.DanhMuc.TenDanhMuc.Contains(categoryName));
                }

                var locations = query
                    .OrderByDescending(d => d.NgayDang)
                    .Select(d => new
                    {
                        id = d.MaDiaDiem,
                        name = d.TenDiaDiem,
                        region = d.VungMien,
                        coordinates = new[] { (double?)d.ViDo, (double?)d.KinhDo },
                        categoryId = d.MaDanhMuc,
                        categoryName = d.DanhMuc.TenDanhMuc,
                        category = d.DanhMuc.TenDanhMuc.ToLower(),
                        image = d.AnhDiaDiems.Where(a => a.LaAnhChinh == true).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        description = d.MoTaNgan,
                        address = d.DiaChiChiTiet,
                        rating = d.DiemDanhGiaTB ?? 0,
                        badge = d.DanhMuc.TenDanhMuc,
                        phone = d.SoDienThoai,
                        email = d.Email,
                        website = d.Website,
                        openingHours = d.GioMoCua,
                        price = d.GiaVe
                    })
                    .ToList();

                return Json(new { success = true, data = locations }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetNearbyLocations(double latitude, double longitude, int radiusKm = 50, int limit = 10)
        {
            try
            {
                var locations = db.DiaDiems
                    .Where(d => d.TrangThai == true && d.ViDo != null && d.KinhDo != null)
                    .AsEnumerable()
                    .Select(d => new
                    {
                        location = d,
                        distance = CalculateHaversineDistance(
                            latitude,
                            longitude,
                            (double)d.ViDo,
                            (double)d.KinhDo
                        )
                    })
                    .Where(x => x.distance <= radiusKm)
                    .OrderBy(x => x.distance)
                    .Take(limit)
                    .Select(x => new
                    {
                        id = x.location.MaDiaDiem,
                        name = x.location.TenDiaDiem,
                        region = x.location.VungMien,
                        coordinates = new[] { (double?)x.location.ViDo, (double?)x.location.KinhDo },
                        category = x.location.DanhMuc.TenDanhMuc.ToLower(),
                        distance = Math.Round(x.distance, 2),
                        image = x.location.AnhDiaDiems.Where(a => a.LaAnhChinh == true).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        description = x.location.MoTaNgan,
                        address = x.location.DiaChiChiTiet,
                        rating = x.location.DiemDanhGiaTB ?? 0,
                        badge = x.location.DanhMuc.TenDanhMuc
                    })
                    .ToList();

                return Json(new { success = true, data = locations }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetLocationDetail(int id)
        {
            try
            {
                var location = db.DiaDiems
                    .FirstOrDefault(d => d.MaDiaDiem == id && d.TrangThai == true);

                if (location == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy địa điểm" }, JsonRequestBehavior.AllowGet);
                }

                var detail = new
                {
                    id = location.MaDiaDiem,
                    name = location.TenDiaDiem,
                    region = location.VungMien,
                    coordinates = new[] { (double?)location.ViDo, (double?)location.KinhDo },
                    category = location.DanhMuc.TenDanhMuc,
                    images = location.AnhDiaDiems.Select(a => new { path = a.DuongDanAnh, isMain = a.LaAnhChinh ?? false }).ToList(),
                    description = location.MoTaChiTiet,
                    shortDescription = location.MoTaNgan,
                    address = location.DiaChiChiTiet,
                    rating = location.DiemDanhGiaTB ?? 0,
                    views = location.LuotXem ?? 0,
                    phone = location.SoDienThoai,
                    email = location.Email,
                    website = location.Website,
                    openingHours = location.GioMoCua,
                    price = location.GiaVe,
                    experiences = location.TraiNghiems.Select(t => new
                    {
                        id = t.MaTraiNghiem,
                        title = t.TieuDe,
                        type = t.LoaiTraiNghiem,
                        description = t.MoTa,
                        icon = t.Icon
                    }).ToList()
                };

                return Json(new { success = true, data = detail }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult SearchLocations(string query, string categoryName = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { success = false, message = "Vui lòng nhập từ khóa tìm kiếm" }, JsonRequestBehavior.AllowGet);
                }

                var searchTerm = query.ToLower();
                var result = db.DiaDiems
                    .Where(d => d.TrangThai == true)
                    .AsEnumerable()
                    .Where(d =>
                        d.TenDiaDiem.ToLower().Contains(searchTerm) ||
                        d.VungMien.ToLower().Contains(searchTerm) ||
                        d.MoTaNgan.ToLower().Contains(searchTerm) ||
                        d.DiaChiChiTiet.ToLower().Contains(searchTerm)
                    );

                if (!string.IsNullOrEmpty(categoryName))
                {
                    result = result.Where(d => d.DanhMuc.TenDanhMuc.ToLower().Contains(categoryName.ToLower()));
                }

                var locations = result
                    .Select(d => new
                    {
                        id = d.MaDiaDiem,
                        name = d.TenDiaDiem,
                        region = d.VungMien,
                        coordinates = new[] { (double?)d.ViDo, (double?)d.KinhDo },
                        category = d.DanhMuc.TenDanhMuc.ToLower(),
                        image = d.AnhDiaDiems.Where(a => a.LaAnhChinh == true).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        description = d.MoTaNgan,
                        address = d.DiaChiChiTiet,
                        rating = d.DiemDanhGiaTB ?? 0,
                        badge = d.DanhMuc.TenDanhMuc
                    })
                    .ToList();

                return Json(new { success = true, data = locations }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public JsonResult GetCategories()
        {
            try
            {
                var categories = db.DanhMucs
                    .Select(c => new
                    {
                        id = c.MaDanhMuc,
                        name = c.TenDanhMuc,
                        description = c.MoTa,
                        count = c.DiaDiems.Count(d => d.TrangThai == true)
                    })
                    .ToList();

                return Json(new { success = true, data = categories }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        [HttpGet]
        public JsonResult SuggestRoute(double latitude, double longitude)
        {
            try
            {
                const double TRANSPORT_THRESHOLD_KM = 100;

                var locations = db.DiaDiems
                    .Where(d => d.TrangThai == true && d.ViDo != null && d.KinhDo != null)
                    .AsEnumerable()
                    .Select(d => new
                    {
                        location = d,
                        distance = CalculateHaversineDistance(
                            latitude,
                            longitude,
                            (double)d.ViDo,
                            (double)d.KinhDo
                        )
                    })
                    .OrderBy(x => x.distance)
                    .Select(x => new
                    {
                        id = x.location.MaDiaDiem,
                        name = x.location.TenDiaDiem,
                        region = x.location.VungMien,
                        coordinates = new[] { (double?)x.location.ViDo, (double?)x.location.KinhDo },
                        category = x.location.DanhMuc.TenDanhMuc.ToLower(),
                        distanceKm = Math.Round(x.distance, 2),
                        transportType = x.distance < TRANSPORT_THRESHOLD_KM ? "motorbike" : "airplane",
                        transportIcon = x.distance < TRANSPORT_THRESHOLD_KM ? "🛵" : "✈️",
                        image = x.location.AnhDiaDiems.Where(a => a.LaAnhChinh == true).Select(a => a.DuongDanAnh).FirstOrDefault() ?? "",
                        description = x.location.MoTaNgan,
                        address = x.location.DiaChiChiTiet,
                        rating = x.location.DiemDanhGiaTB ?? 0,
                        badge = x.location.DanhMuc.TenDanhMuc
                    })
                    .ToList();

                return Json(new { success = true, data = locations }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// API MỚI: Xử lý logic Routing tập trung tại Backend
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetDirections(double fromLat, double fromLng, int toId)
        {
            try
            {
                var destination = db.DiaDiems.FirstOrDefault(d => d.MaDiaDiem == toId);
                if (destination == null || destination.ViDo == null || destination.KinhDo == null)
                {
                    return Json(new { success = false, message = "Địa điểm không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }

                double destLat = (double)destination.ViDo;
                double destLng = (double)destination.KinhDo;
                double distance = Math.Round(CalculateHaversineDistance(fromLat, fromLng, destLat, destLng), 1);

                // Dưới 100km -> Đi đường bộ (Gọi thẳng OSRM từ server)
                if (distance <= 100)
                {
                    string osrmUrl = $"https://router.project-osrm.org/route/v1/driving/{fromLng},{fromLat};{destLng},{destLat}?overview=full&geometries=geojson&steps=true";

                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("User-Agent", "RobinsVilla/1.0");
                        var response = await client.GetStringAsync(osrmUrl);

                        // Trả về JSON thô của OSRM bọc trong metadata của hệ thống
                        string jsonResult = $"{{\"success\": true, \"type\": \"road\", \"distanceKm\": {distance}, \"destinationName\": \"{destination.TenDiaDiem}\", \"osrmData\": {response}}}";
                        return Content(jsonResult, "application/json");
                    }
                }
                else
                {
                    // Trên 100km -> Kịch bản bay (Giả lập tính toán sân bay trung chuyển)
                    // Trong thực tế, bạn sẽ truy vấn bảng SanBay dựa trên tọa độ
                    var flightData = new
                    {
                        success = true,
                        type = "flight",
                        distanceKm = distance,
                        destinationName = destination.TenDiaDiem,
                        fromAirport = "Sân bay khu vực của bạn",
                        toAirport = $"Sân bay gần {destination.VungMien}",
                        estimatedTime = $"{Math.Round(distance / 500, 1)} giờ", // Ước lượng 500km/h
                        destCoords = new[] { destLat, destLng },
                        segments = new[]
                        {
                            new { text = "🚗 Di chuyển từ vị trí của bạn đến sân bay đi", icon = "bi-car-front" },
                            new { text = $"✈️ Chuyến bay đến khu vực {destination.VungMien}", icon = "bi-airplane" },
                            new { text = $"🚕 Di chuyển từ sân bay đến {destination.TenDiaDiem}", icon = "bi-geo-alt" }
                        }
                    };
                    return Json(flightData, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}