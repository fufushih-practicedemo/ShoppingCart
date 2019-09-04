using ShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShoppingCart.Models.ViewModels.User
{
    public class UserVM
    {
        public UserVM()
        {

        }

        public UserVM(UserDTO row)
        {

        }

        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        public string Account { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string ConfirmPassword { get; set; }

    }
}