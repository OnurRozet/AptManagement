using AptManagement.Application.Common;
using AptManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AptManagement.Application.Interfaces
{
    public interface IBankStatementService
    {
        List<BankTransactionDto> ParseExcelFile(Stream fileStream);
        Task<ServiceResult<bool>> ProcessBankStatementAsync(List<BankTransactionDto> transactions);
    }
}
