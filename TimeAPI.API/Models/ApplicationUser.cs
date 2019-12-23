﻿using Microsoft.AspNetCore.Identity;

namespace TimeAPI.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool isSuperAdmin { get; set; }


    }
}
