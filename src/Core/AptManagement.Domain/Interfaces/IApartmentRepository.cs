using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Interfaces
{
    public interface IApartmentRepository : IRepository<Apartment>
    {
    }
}
