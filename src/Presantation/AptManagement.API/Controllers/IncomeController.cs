using AptManagement.Application.Common;
using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using AptManagement.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AptManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeController(IIncomeService incomeService) : ControllerBase
    {
        [HttpGet("get")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var entity = await incomeService.GetIncomeById(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] IncomeSearch request)
        {
            var entity = await incomeService.Search(request);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("get-summary-income")]
        public IActionResult GetSummaryIncomeReport()
        {
            var entity = incomeService.GetSummaryIncomeReport();
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] IncomeDto dto)
        {
            var entity = await incomeService.CreateOrEdit(dto);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await incomeService.DeleteIncomeAsync(id);
            if (!entity.IsSuccess) return NotFound();
            return Ok(entity);
        }

        [HttpGet("payment-matrix/{year}")]
        public async Task<IActionResult> GetPaymentMatrix(int year)
        {
            var data = await incomeService.GetYearlyPaymentMatrixAsync(year);
            return Ok(ServiceResult<List<PaymentMatrixDto>>.Success(data));
        }
    }
}

