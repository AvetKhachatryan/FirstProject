﻿using FirstProject.Data;

namespace FirstProject.Models
{
    public class RegistrationModel
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        //public UserRoleType Role { get; set; }
    }
}
