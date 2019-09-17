using ShoppingCart.Areas.Admin.Models.ViewModel.Dashboard;
using ShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        // GET: Admin/Dashboard
        public ActionResult Index()
        {
            DashboardVM model = new DashboardVM();

            using(Db db = new Db()) {
                model.userCount = db.Users.Count();
                model.categoryCount = db.Categories.Count();
                model.productCount = db.Products.Count();
                model.orderCount = db.Orders.Count();
            }

            return View(model);
        }
    }
}