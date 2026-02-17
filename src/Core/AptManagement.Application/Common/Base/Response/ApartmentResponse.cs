using AptManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Common.Base.Response
{
    public class ApartmentResponse
    {
        public int Id { get; set; }
        public required string Label { get; set; }
        public required string OwnerName { get; set; }
        public string? TenantName { get; set; }
        public decimal Balance { get; set; } 
        public decimal OpeningBalance { get; set; }
        public bool IsManager { get; set; }
    }
}
