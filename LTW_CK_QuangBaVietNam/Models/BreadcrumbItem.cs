namespace LTW_CK_QuangBaVietNam.Models
{
    public class BreadcrumbItem
    {
        public BreadcrumbItem() { }

        public BreadcrumbItem(string title, string url = null, bool isActive = false)
        {
            Title = title;
            Url = url;
            IsActive = isActive;
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public bool IsActive { get; set; }
    }
}
