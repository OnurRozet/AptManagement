using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string ApartmentNumber { get; set; } // Hem kullanıcı adı hem daire no olacak
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
    }
}
