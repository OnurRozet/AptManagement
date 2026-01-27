using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Dtos
{
    public class BulkApartmentRequest : ApartmentDto
    {
        public int TotalApartments { get; set; }
    }
}
