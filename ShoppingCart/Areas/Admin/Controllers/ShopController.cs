using PagedList;
using ShoppingCart.Areas.Admin.Models.ViewModel.Shop;
using ShoppingCart.Models.Data;
using ShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace ShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ShopController : Controller
    {
        #region Category

        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {
            List<CategoryVM> categoryVMList;

            using(Db db= new Db()) {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();
            }

            return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            string id;

            using(Db db= new Db()) {
                if(db.Categories.Any(x => x.Name == catName)) {
                    return "titletaken";
                }

                CategoryDTO dto = new CategoryDTO();

                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;

                db.Categories.Add(dto);
                db.SaveChanges();

                id = dto.Id.ToString();
            }

            return id;
        }

        // POST: Admin/Shop/ReorderCategories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using(Db db = new Db()) {
                int count = 1;

                CategoryDTO dto;

                foreach(var catid in id) {
                    dto = db.Categories.Find(catid);
                    dto.Sorting = count;

                    db.SaveChanges();
                    count++;
                }
            }
        }

        // GET: Admin/Shop/DeleteCategory/id
        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db()) {

                CategoryDTO dto = db.Categories.Find(id);
                db.Categories.Remove(dto);

                db.SaveChanges();
            }

            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName, int id)
        {
            using(Db db = new Db()) {
                if(db.Categories.Any(x => x.Name == newCatName)) {
                    return "titletaken";
                }

                CategoryDTO dto = db.Categories.Find(id);

                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ", "-").ToLower();

                db.SaveChanges();
            }

            return "ok";
        }

        #endregion

        #region Product

        // GET: Admin/Shop/AddProduct
        public ActionResult AddProduct()
        {
            ProductVM model = new ProductVM();

            using(Db db = new Db()) {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
            }


            return View(model);
        }

        // POST: Admin/Shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model, HttpPostedFileBase file)
        {
            if (!ModelState.IsValid) {
                using(Db db = new Db()) {

                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }
            }

            // Make sure product name is unique
            using (Db db = new Db()) {
                if(db.Categories.Any(x => x.Name == model.Name)) {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "The product name is taken!!");
                    return View(model);
                }
                
            }

            // Declare product id
            int id;

            using(Db db = new Db()) {
                ProductDTO product = new ProductDTO();

                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ", "-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO cat = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                model.CategoryName = cat.Name;

                db.Products.Add(product);
                db.SaveChanges();

                // Get id
                id = product.Id;
            }

            // set tempdata message
            TempData["SM"] = "You add a product";

            # region Upload Image
            
            // Create a dir
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            var pathString1 = Path.Combine(originalDirectory.ToString(), "Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
                Directory.CreateDirectory(pathString1);
            if (!Directory.Exists(pathString2))
                Directory.CreateDirectory(pathString2);
            if (!Directory.Exists(pathString3))
                Directory.CreateDirectory(pathString3);
            if (!Directory.Exists(pathString4))
                Directory.CreateDirectory(pathString4);
            if (!Directory.Exists(pathString5))
                Directory.CreateDirectory(pathString5);

            // Check if a file was uploaded
            if (file != null && file.ContentLength > 0) {
                string ext = file.ContentType.ToLower();

                if(ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/png" && 
                    ext != "image/x-png") {
                    
                    using(Db db = new Db()) {
                        model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                        ModelState.AddModelError("", "The image was not uploaded");
                        return View(model);
                    }
                }

                string imgName = file.FileName;

                using(Db db = new Db()) {
                    ProductDTO product = db.Products.Find(id);
                    product.ImageName = imgName;

                    db.SaveChanges();
                }

                // Set original and thumb image paths
                var path = string.Format("{0}\\{1}", pathString2, imgName);
                var path2 = string.Format("{0}\\{1}", pathString3, imgName);

                file.SaveAs(path);

                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }
    
            #endregion


            return RedirectToAction("AddProduct");
        }

        // GET: Admin/Shop/Products
        public ActionResult Products(int? page, int? catId)
        {
            List<ProductVM> listProductVM;

            // Set page number
            var pageNum = page ?? 1;

            using(Db db = new Db()) {
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

        // GET: Admin/Shop/EditProduct/id
        public ActionResult EditProduct(int id)
        {
            ProductVM model;

            using(Db db = new Db()) {
                ProductDTO dto = db.Products.Find(id);

                if(dto == null) {
                    return Content("That product does not exist");
                }

                model = new ProductVM(dto);
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

                // Get all gallery images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                    .Select(fn => Path.GetFileName(fn));

            }

            return View(model);
        }

        // GET: Admin/Shop/DeleteProduct/id
        public ActionResult DeleteProduct(int id)
        {
            // Delete product from DB
            using (Db db = new Db()) {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);

                db.SaveChanges();
            }

            // Delete product folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

            string pathString = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());

            if (Directory.Exists(pathString)) {
                foreach(var info in new DirectoryInfo(pathString).GetFileSystemInfos("*", SearchOption.AllDirectories)) {
                    info.Attributes = FileAttributes.Normal;
                }
                Directory.Delete(pathString, true);
            }

            // Redirect
            return RedirectToAction("Products");
        }

        // GET: Admin/Shop/Orders
        public ActionResult Orders()
        {
            List<OrderForAdminVM> orderForAdmin = new List<OrderForAdminVM>();

            using(Db db = new Db()) {
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                foreach(var order in orders) {

                    // Init product dict
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    // Get account
                    UserDTO user = db.Users.Where(x => x.Id == order.UserId).FirstOrDefault();
                    string userAccount = user.Account;

                    // Loop through list of OrderDetailDTO
                    foreach(var orderDetail in orderDetailsList) {
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetail.ProductId).FirstOrDefault();

                        decimal price = product.Price;
                        string productName = product.Name;

                        productAndQty.Add(productName, orderDetail.Quantity);

                        total += orderDetail.Quantity * price;
                    }

                    // Add to order for admin vm list
                    orderForAdmin.Add(new OrderForAdminVM()
                    {
                        OrderNumber = order.OrderId,
                        UserName = userAccount,
                        Total = total,
                        ProductAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(orderForAdmin);
        }

        #endregion
    }
}