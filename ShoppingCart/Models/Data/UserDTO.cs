using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ShoppingCart.Models.Data
{
    [Table("tblUser")]
    public class UserDTO
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Account { get; set; }
        public string Password { get; set; }
    }
}