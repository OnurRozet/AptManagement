using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class UserResponse
    {
        public string? FullName { get; set; }
        public bool IsManager { get; set; }
        public string? ApartmentNumber { get; set; }
        public string? Email { get; set; }
    }
}