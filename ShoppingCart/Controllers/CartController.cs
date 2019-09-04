using ShoppingCart.Models.Data;
using ShoppingCart.Models.ViewModels.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace ShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            // Init cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            // Check if cart is empty
            if(cart.Count == 0 || Session["cart"] == null) {
                ViewBag.Message = "Your cart is empty!";
                return View();
            }

            // Calculate total and save to viewbag
            decimal total = 0m;

            foreach(var item in cart) {
                total += item.Total;
            }

            ViewBag.GrandTotal = total;

            return View(cart);
        }

        public ActionResult CartPartial()
        {
            // Init
            CartVM model = new CartVM();
            int qty = 0;
            decimal price = 0m;

            // Check for cart session
            if(Session["cart"] != null) {
                var list = (List<CartVM>)Session["cart"];

                foreach(var item in list) {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                    model.Quantity = qty;
                    model.Price = price;
                }
            } else {
                model.Quantity = 0;
                model.Price = 0m;
            }

            return PartialView(model);
        }

        // 
        public ActionResult AddToCartPartial(int id)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            CartVM model = new CartVM();

            using(Db db = new Db()) {
                ProductDTO product = db.Products.Find(id);

                // Check if the product is already in cart
                var productInCart = cart.FirstOrDefault(x => x.ProductId == id);
                // If not, add
                if(productInCart == null) {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price = product.Price,
                        Image = product.ImageName
                    });

                } else {
                    // If it is , increment
                    productInCart.Quantity++;
                }
            }

            // Get Total qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach(var item in cart) {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }

            model.Quantity = qty;
            model.Price = price;

            // Save cart back to session
            Session["cart"] = cart;

            return PartialView(model);
        }

        // GET: /Cart/IncrementProduct
        public ActionResult IncrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using(Db db = new Db()) {
                
                // Get cartVM from list
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                // increment qty
                model.Quantity++;

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Cart/DecrementProduct
        public ActionResult DecrementProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using(Db db = new Db()) {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                if(model.Quantity > 1) {
                    model.Quantity--;
                }else {
                    model.Quantity = 0;
                    cart.Remove(model);
                }

                var result = new { qty = model.Quantity, price = model.Price };

                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }

        // GET: /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db()) {
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);

                cart.Remove(model);
            }
        }

        // POST: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            string username = User.Identity.Name;
            int orderId = 0;

            using (Db db = new Db()) {
                OrderDTO orderDTO = new OrderDTO();

                // Get user id
                var q = db.Users.FirstOrDefault(x => x.Account == username);
                int userId = q.Id;

                // Add to OrderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();

                // Get insert Id
                orderId = orderDTO.OrderId;

                // Init OrderDetailDTO
                OrderDetailDTO orderDetailDTO = new OrderDetailDTO();

                // Add to DTO
                foreach(var item in cart) {
                    orderDetailDTO.OrderId = orderId;
                    orderDetailDTO.UserId = userId;
                    orderDetailDTO.ProductId = item.ProductId;
                    orderDetailDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailDTO);
                    db.SaveChanges();
                }

                // Email admin
                // Using mailtrap
                var client = new SmtpClient("smtp.mailtrap.io", 2525)
                {
                    Credentials = new NetworkCredential("885dfaed592216", "66f1d9eb38948a"),
                    EnableSsl = true
                };
                client.Send("admin@example.com", "admin@example.com", "New Order", "You have a new order, Order Id :" + orderId);

                // Reset session
                Session["cart"] = null;
            }
        }
    }
}