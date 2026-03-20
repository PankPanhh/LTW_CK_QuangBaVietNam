using System;
using System.Collections.Generic;
using System.Web.Mvc;
using LTW_CK_QuangBaVietNam.Models;

namespace LTW_CK_QuangBaVietNam.Helpers
{
    /// <summary>
    /// Extension methods for managing breadcrumb navigation
    /// </summary>
    public static class BreadcrumbHelper
    {
        /// <summary>
        /// Set breadcrumbs for the current page
        /// </summary>
        public static void SetBreadcrumbs(this Controller controller, List<BreadcrumbItem> breadcrumbs)
        {
            if (controller != null)
            {
                controller.ViewBag.Breadcrumbs = breadcrumbs;
            }
        }

        /// <summary>
        /// Add a breadcrumb item to the current breadcrumb list
        /// </summary>
        public static void AddBreadcrumb(this Controller controller, string name, string url = null)
        {
            if (controller != null)
            {
                List<BreadcrumbItem> breadcrumbs = controller.ViewBag.Breadcrumbs ?? new List<BreadcrumbItem>();
                breadcrumbs.Add(new BreadcrumbItem(name, url));
                controller.ViewBag.Breadcrumbs = breadcrumbs;
            }
        }

        /// <summary>
        /// Get current breadcrumbs
        /// </summary>
        public static List<BreadcrumbItem> GetBreadcrumbs(this Controller controller)
        {
            if (controller != null && controller.ViewBag.Breadcrumbs != null)
            {
                return controller.ViewBag.Breadcrumbs;
            }
            return new List<BreadcrumbItem>();
        }

        /// <summary>
        /// Clear all breadcrumbs
        /// </summary>
        public static void ClearBreadcrumbs(this Controller controller)
        {
            if (controller != null)
            {
                controller.ViewBag.Breadcrumbs = new List<BreadcrumbItem>();
            }
        }
    }
}
