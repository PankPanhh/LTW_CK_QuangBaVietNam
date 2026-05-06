using System;

namespace LTW_CK_QuangBaVietNam.Models
{
    /// <summary>
    /// Represents a breadcrumb item in the navigation trail
    /// </summary>
    public class BreadcrumbItem
    {
        /// <summary>
        /// Display name of the breadcrumb item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL to navigate when clicked (null if this is the current page)
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether this is the current/active page
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Constructor for creating a breadcrumb item
        /// </summary>
        public BreadcrumbItem(string name, string url = null, bool isActive = false)
        {
            Name = name;
            Url = url;
            IsActive = isActive;
        }

        public class FavoritePlaceVM
        {
            public int MaDiaDiem { get; set; }
            public string TenDiaDiem { get; set; }
            public string VungMien { get; set; }
            public string AnhChinh { get; set; }
            public DateTime NgayLuu { get; set; }
        }
    }
}
