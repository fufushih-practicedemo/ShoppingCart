using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShoppingCart.Models.ViewModels.User
{
    public class OrderForUserVM
    {
        public int OrderNumber { get; set; }
        public decimal Total { get; set; }
        public Dictionary<string, int> ProductAndQty { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}