using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingCart.Areas.Admin.Models.ViewModel.Dashboard
{
    public class DashboardVM
    {
        public int userCount { get; set; }
        public int productCount { get; set; }
        public int categoryCount { get; set; }
        public int orderCount { get; set; }
    }
}