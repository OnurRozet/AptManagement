using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Domain.Enums
{
    public enum PaymentCategory
    {
        Dues=1,          // Aidat
        ExtraPayment =2, // Ek Ödeme
        Invoice=3,
        Maintenance=4,//Bakım-Onarım
        BankTransfer=5    
    }
}
