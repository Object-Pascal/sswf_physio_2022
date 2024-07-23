using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Core.Domain
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public int DataId { get; set; }

        public ApplicationUser()
            : base() { }
    }
}