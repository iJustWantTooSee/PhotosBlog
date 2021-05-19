using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace PhotoBlogs.Views.Manage
{
    public static class ManageNavPages
    {
        public static String ActivePageKey => "ActivePage";

        public static String Index => "Index";

        public static String ChangePassword => "ChangePassword";

        public static String IndexNavClass(ViewContext viewContext) => ManageNavPages.PageNavClass(viewContext, ManageNavPages.Index);

        public static String ChangePasswordNavClass(ViewContext viewContext) => ManageNavPages.PageNavClass(viewContext, ManageNavPages.ChangePassword);

        public static String PageNavClass(ViewContext viewContext, String page)
        {
            var activePage = viewContext.ViewData[ManageNavPages.ActivePageKey] as String;
            return String.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

        public static void AddActivePage(this ViewDataDictionary viewData, String activePage) => viewData[ManageNavPages.ActivePageKey] = activePage;
    }
}
