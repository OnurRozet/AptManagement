using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Enums
{
    public enum DebtType
    {
        Dues=1,
        Fixture=2,           //Demirbaş
        TransferFromPast=3  //Geçmişten devreden borç
    }
}
