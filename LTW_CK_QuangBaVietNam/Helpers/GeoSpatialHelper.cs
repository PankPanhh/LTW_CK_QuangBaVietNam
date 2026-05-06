using System;
using System.Collections.Generic;
using System.Linq;

namespace LTW_CK_QuangBaVietNam.Helpers
{
    /// <summary>
    /// Hỗ trợ các phép tính địa lý không gian cho bản đồ du lịch
    /// </summary>
    public static class GeoSpatialHelper
    {
        /// <summary>
        /// Tính toán khoảng cách giữa hai điểm bằng công thức Haversine
        /// </summary>
        /// <param name="lat1">Vĩ độ điểm 1</param>
        /// <param name="lon1">Kinh độ điểm 1</param>
        /// <param name="lat2">Vĩ độ điểm 2</param>
        /// <param name="lon2">Kinh độ điểm 2</param>
        /// <returns>Khoảng cách tính bằng km</returns>
        public static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Asin(Math.Sqrt(a));
            return R * c;
        }

        /// <summary>
        /// Chuyển đổi độ sang radian
        /// </summary>
        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        /// <summary>
        /// Tính toán khung giới hạn (bounding box) cho một tâm và bán kính
        /// Hữu ích cho việc tìm kiếm các địa điểm trong một vùng
        /// </summary>
        public static (double minLat, double maxLat, double minLon, double maxLon) GetBoundingBox(
            double centerLat, 
            double centerLon, 
            double radiusKm)
        {
            const double latChange = 0.008983; // 1 km ở xích đạo ≈ 0.008983 độ
            const double lonChange = 0.008983; // Điều chỉnh dựa trên vĩ độ
            
            var latOffset = radiusKm * latChange;
            var lonOffset = radiusKm * lonChange / Math.Cos(DegreesToRadians(centerLat));
            
            return (
                centerLat - latOffset,
                centerLat + latOffset,
                centerLon - lonOffset,
                centerLon + lonOffset
            );
        }

        /// <summary>
        /// Kiểm tra xem một điểm có nằm trong khung giới hạn không
        /// </summary>
        public static bool IsPointInBounds(
            double lat, 
            double lon,
            double minLat,
            double maxLat,
            double minLon,
            double maxLon)
        {
            return lat >= minLat && lat <= maxLat && lon >= minLon && lon <= maxLon;
        }

        /// <summary>
        /// Sắp xếp một danh sách các điểm theo khoảng cách từ một vị trí
        /// </summary>
        public static List<T> SortByDistance<T>(
            IEnumerable<T> items,
            double centerLat,
            double centerLon,
            Func<T, double> getLatitude,
            Func<T, double> getLongitude) where T : class
        {
            return items
                .OrderBy(item => CalculateHaversineDistance(
                    centerLat,
                    centerLon,
                    getLatitude(item),
                    getLongitude(item)))
                .ToList();
        }

        /// <summary>
        /// Chọn tuyến tham quan tối ưu bằng thuật toán greedy
        /// Bắt đầu từ vị trí ban đầu, lần lượt chọn địa điểm gần nhất chưa được ghé qua
        /// </summary>
        public static List<T> SelectOptimalRoute<T>(
            List<T> locations,
            double startLat,
            double startLon,
            int maxPoints,
            Func<T, double> getLatitude,
            Func<T, double> getLongitude) where T : class
        {
            var selected = new List<T>();
            var remaining = locations.ToList();

            double currentLat = startLat;
            double currentLon = startLon;

            for (int i = 0; i < maxPoints && remaining.Count > 0; i++)
            {
                // Tìm điểm gần nhất từ vị trí hiện tại
                var nearest = remaining
                    .OrderBy(item => CalculateHaversineDistance(
                        currentLat,
                        currentLon,
                        getLatitude(item),
                        getLongitude(item)))
                    .First();

                selected.Add(nearest);
                remaining.Remove(nearest);

                // Cập nhật vị trí hiện tại
                currentLat = getLatitude(nearest);
                currentLon = getLongitude(nearest);
            }

            return selected;
        }

        /// <summary>
        /// Tính tổng quãng đường của một tuyến đi
        /// </summary>
        public static double CalculateTotalDistance<T>(
            List<T> route,
            double startLat,
            double startLon,
            Func<T, double> getLatitude,
            Func<T, double> getLongitude) where T : class
        {
            double totalDistance = 0;
            double currentLat = startLat;
            double currentLon = startLon;

            foreach (var item in route)
            {
                var itemLat = getLatitude(item);
                var itemLon = getLongitude(item);

                totalDistance += CalculateHaversineDistance(currentLat, currentLon, itemLat, itemLon);

                currentLat = itemLat;
                currentLon = itemLon;
            }

            return totalDistance;
        }
    }
}
