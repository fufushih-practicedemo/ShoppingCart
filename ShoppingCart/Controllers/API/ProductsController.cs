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
        public IEnumerable<ProductDTO> GetProducts()
        {
            Db db = new Db();
            var products = db.Products.ToArray().Select(x => new ProductDTO()
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Price = x.Price,
                CategoryName = x.CategoryName,
                CategoryId = x.CategoryId,
                ImageName = x.ImageName
            }).ToList();

            return products;
        }


        public ProductDTO GetProduct(int id)
        {
            Db db = new Db();
            return db.Products.FirstOrDefault(x => x.Id == id);

        }
    }
}
