using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingCart.Areas.Admin.Models.ViewModel.Shop
{
    public class OrderForAdminVM
    {
        public int OrderNumber { get; set; }
        public string UserName { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductAndQty { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}