using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public int ApartmentId { get; set; }
        public Apartment Apartment { get; set; } = null!;

        public string ApartmentNumber { get; set; }
        public required string FullName { get; set; }
        public bool IsManager { get; set; }

    }
}
