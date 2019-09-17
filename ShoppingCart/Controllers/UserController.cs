using ShoppingCart.Models.Data;
using ShoppingCart.Models.Features;
using ShoppingCart.Models.ViewModels.Shop;
using ShoppingCart.Models.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ShoppingCart.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: /user/login
        public ActionResult Login()
        {
            string username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
                return RedirectToAction("user-profile");

            return View();
        }

        // GET: /user/create-account
        [ActionName("create-account")]
        public ActionResult CreateAccount()
        {
            return View("CreateAccount");
        }

        // POST: /user/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            // check model state
            if (!ModelState.IsValid) {
                return View("CreateAccount", model);
            }

            // check password match confirm password
            if (!model.Password.Equals(model.ConfirmPassword)) {
                ModelState.AddModelError("", "密碼不符");
                return View("CreateAccount", model);
            }

            using (Db db = new Db()) {
                AccountFeature accountFeature = new AccountFeature();
                var hashPassword = accountFeature.HashPassword(model.Password);

                // make sure account is unique
                if (db.Users.Any(x => x.Account.Equals(model.Account))) {
                    ModelState.AddModelError("", "Account " + model.Account + "已有人使用!!");
                    model.Account = "";
                    return View("CreateAccount", model);
                }

                // create new userDTO
                UserDTO userDTO = new UserDTO()
                {
                    Name = model.Name,
                    Email = model.Email,
                    Account = model.Account,
                    Password = hashPassword
                };
                // add DTO to db and save
                db.Users.Add(userDTO);
                db.SaveChanges();

                // add to userroleDTO
                int userId = userDTO.Id;
                UserRoleDTO userRoleDTO = new UserRoleDTO()
                {
                    UserId = userId,
                    RoleId = 2
                };
                // add to db and save
                db.UserRoles.Add(userRoleDTO);
                db.SaveChanges();

            }

            TempData["SM"] = "你已經創建帳戶!!";

            // Redirect
            return Redirect("~/user/login");
        }

        // POST: /user/login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            if (!ModelState.IsValid) {
                return View(model);
            }

            // check if the user is valid
            bool isValid = false;

            using (Db db = new Db()) {
                AccountFeature accountFeature = new AccountFeature();
                var hashPassword = accountFeature.HashPassword(model.Password);

                if (db.Users.Any(x => x.Account.Equals(model.Account) && x.Password.Equals(hashPassword))) {
                    isValid = true;
                }

                if (!isValid) {
                    ModelState.AddModelError("", "Invalid account or password!!!");
                    return View(model);
                } else {
                    FormsAuthentication.SetAuthCookie(model.Account, model.RememberMe); // see System.Web.Security
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Account, model.RememberMe));
                }
            }
        }

        // GET: /user/Logout
        [Authorize]
        public ActionResult LogOut()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/user/login");
        }

        [Authorize]
        public ActionResult UserNavPartial()
        {
            string userAccount = User.Identity.Name;

            UserNavPartialVM model;
            using (Db db = new Db()) {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Account == userAccount);

                model = new UserNavPartialVM()
                {
                    Name = dto.Name
                };
            }
            return PartialView(model);
        }

        // GET: /user/user-profile
        [Authorize]
        [ActionName("user-profile")]
        public ActionResult UserProfile()
        {
            string userAccount = User.Identity.Name;
            UserProfileVM model;

            using(Db db = new Db()) {
                UserDTO dto = db.Users.FirstOrDefault(x => x.Account == userAccount);

                model = new UserProfileVM(dto);
            }

            return View("UserProfile", model);
        }

        // POST: /user/user-profile
        [Authorize]
        [HttpPost]
        [ActionName("user-profile")]
        public ActionResult UserProfile(UserProfileVM model)
        {
            if (!ModelState.IsValid) {
                return View("UserProfile", model);
            }

            // Check if password if need be
            if (!string.IsNullOrWhiteSpace(model.Password)) {
                if (!model.Password.Equals(model.ConfirmPassword)) {
                    ModelState.AddModelError("", "Password not match");
                    return View("UserProfile", model);
                }
            }

            using(Db db = new Db()) {
                string userAccount = User.Identity.Name;

                AccountFeature accountFeature = new AccountFeature();
                var hashPassword = accountFeature.HashPassword(model.Password);

                // Make sure account is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x => x.Account == userAccount)) {
                    ModelState.AddModelError("", "Account " + model.Account + " already exists!");
                    model.Account = "";
                    return View("UserProfile", model);
                }

                // Edit
                UserDTO dto = db.Users.Find(model.Id);

                dto.Name = model.Name;
                dto.Email = model.Email;
                dto.Account = model.Account;

                if (!string.IsNullOrWhiteSpace(model.Password)) {
                    dto.Password = hashPassword;
                }

                db.SaveChanges();
            }

            TempData["SM"] = "You have edit your profile!";

            return Redirect("~/user/user-profile");
        }

        // GET: /user/Orders
        [Authorize(Roles = "User")]
        public ActionResult Orders()
        {
            List<OrderForUserVM> orderForUser = new List<OrderForUserVM>();

            using(Db db = new Db()) {
                UserDTO user = db.Users.Where(x => x.Account == User.Identity.Name).FirstOrDefault();
                int userId = user.Id;

                List<OrderVM> orders = db.Orders.Where(x => x.UserId == userId).ToArray().Select(x => new OrderVM(x)).ToList();

                foreach(var order in orders) {
                    Dictionary<string, int> productAndQty = new Dictionary<string, int>();

                    decimal total = 0m;

                    List<OrderDetailDTO> orderDetailDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();

                    foreach(var orderDetails in orderDetailDTO) {
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();
                        decimal price = product.Price;
                        string productName = product.Name;

                        productAndQty.Add(productName, orderDetails.Quantity);

                        total += orderDetails.Quantity * price;
                    }

                    // Add to order for user vm list
                    orderForUser.Add(new OrderForUserVM()
                    {
                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductAndQty = productAndQty,
                        CreatedAt = order.CreatedAt
                    });
                }
            }

            return View(orderForUser);
        }
    }
}