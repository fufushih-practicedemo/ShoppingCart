using ShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ShoppingCart.Controllers.API
{
    public class ProductsController : ApiController
    {
        public IEnumerable<object> GetProducts()
        {
            Db db = new Db();
            var products = db.Products.ToArray().Select(x => new 
            {
                x.Name,
                x.Price,
                x.CategoryName,
                x.CategoryId,
            }).ToList();

            return products;
        }


        public ProductDTO GetProduct(int id)
        {
            Db db = new Db();
            return db.Products.FirstOrDefault(x => x.Id == id);

        }

        public IEnumerable<OrderDTO> GetOrders()
        {
            Db db = new Db();
            var orders = db.Orders.ToArray().Select(x => new OrderDTO()
            {
                OrderId = x.OrderId,
                UserId = x.UserId,
                CreatedAt = x.CreatedAt
            }).ToList();

            return orders;
        }

        public IEnumerable<UserDTO> GetUsers()
        {
            Db db = new Db();
            var users = db.Users.ToArray().Select(x => new UserDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Email = x.Email,
                Account = x.Account,
                Password = x.Password
            }).ToList();

            return users;
        }
    }
}
