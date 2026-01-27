using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class ApartmentDto
    {
        public int Id { get; set; }
        public required string Label { get; set; }
        public required string OwnerName { get; set; }
        public string? TenantName { get; set; }
        public bool? IsManager { get; set; }
        public decimal Balance { get; set; } = 0;
        public decimal? OpeningBalance { get; set; } 
    }
}
