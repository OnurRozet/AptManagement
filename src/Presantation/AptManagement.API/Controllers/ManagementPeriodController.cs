using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AptManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagementPeriodController(IManagementPeriodService managementPeriodService) : ControllerBase
    {
        [HttpGet("get")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var entity = await managementPeriodService.GetManagementPeriodById(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] ManagementPeriodSearch request)
        {
            var entity = await managementPeriodService.Search(request);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] ManagementPeriodDto dto)
        {
            var entity = await managementPeriodService.CreateOrEdit(dto);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await managementPeriodService.DeleteManagementPeriodAsync(id);
            if (!entity.IsSuccess) return NotFound();
            return Ok(entity);
        }
    }
}

