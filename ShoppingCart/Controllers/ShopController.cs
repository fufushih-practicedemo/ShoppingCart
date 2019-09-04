using PagedList;
using ShoppingCart.Models.Data;
using ShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listProductVM;

            // Set page number
            var pageNum = page ?? 1;

            using (Db db = new Db()) {
                listProductVM = db.Products.ToArray()
                    .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                    .Select(x => new ProductVM(x))
                    .ToList();

                // Populate categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                ViewBag.SelectCat = catId.ToString();

            }

            var onePageOfProduct = listProductVM.ToPagedList(pageNum, 5);
            ViewBag.OnePageOfProduct = onePageOfProduct;

            return View(listProductVM);
        }

        // GET: /shop/category/name
        public ActionResult Category(string name)
        {
            // Declare a list of ProductVm
            List<ProductVM> ProductVMList;

            using (Db db = new Db()) {
                // Get category id
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;

                // Init the list
                ProductVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();

                // Get category name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }

            // Return view with list
            return View(ProductVMList);
        }

        // GET: /shop/product-details/id
        public ActionResult ProductDetails(int id)
        {

            ProductVM model;

            using (Db db = new Db()) {
                ProductDTO dto = db.Products.Find(id);

                if (dto == null) {
                    return Content("That product does not exist");
                }

                model = new ProductVM(dto);

            }

            return View("ProductDetails", model);
        }

        
    }
}