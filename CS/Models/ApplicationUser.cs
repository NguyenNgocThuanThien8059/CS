using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CS.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Nickname { get; set; }
    }
}
