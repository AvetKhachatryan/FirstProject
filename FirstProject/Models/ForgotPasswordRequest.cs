﻿using System.ComponentModel.DataAnnotations;

namespace FirstProject.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
