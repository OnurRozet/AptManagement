using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AptManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IncomeDebtAllocationController(IIncomeDebtAllocationService incomeDebtAllocationService) : ControllerBase
    {
        [HttpGet("get")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var entity = await incomeDebtAllocationService.GetIncomeDebtAllocationById(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] IncomeDebtAllocationSearch request)
        {
            var entity = await incomeDebtAllocationService.Search(request);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] IncomeDebtAllocationDto dto)
        {
            var entity = await incomeDebtAllocationService.CreateOrEdit(dto);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await incomeDebtAllocationService.DeleteIncomeDebtAllocationAsync(id);
            if (!entity.IsSuccess) return NotFound();
            return Ok(entity);
        }
    }
}

