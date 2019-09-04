using ShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShoppingCart.Models.ViewModels.User
{
    public class UserProfileVM
    {
        public UserProfileVM()
        {

        }

        public UserProfileVM(UserDTO row)
        {
            Id = row.Id;
            Name = row.Name;
            Email = row.Email;
            Account = row.Account;
            Password = row.Password;
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