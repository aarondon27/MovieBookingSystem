using Microsoft.AspNetCore.Identity;

namespace BMS.Models
{
    public class AppUser : IdentityUser
    {
        public string Name { get; set; } = "";
    }
}
