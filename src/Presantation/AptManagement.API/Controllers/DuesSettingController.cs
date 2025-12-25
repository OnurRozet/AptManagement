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
    public class DuesSettingController(IDuesSettingService duesSettingService) : ControllerBase
    {
        [HttpGet("get")]
        public async Task<IActionResult> GetById([FromQuery] int id)
        {
            var entity = await duesSettingService.GetDuesSettingById(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] DuesSettingSearch request)
        {
            var entity = await duesSettingService.Search(request);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody] DuesSettingDto dto)
        {
            var entity = await duesSettingService.CreateOrEdit(dto);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await duesSettingService.DeleteDuesSettingAsync(id);
            if (!entity) return NotFound();
            return Ok(entity);
        }
    }
}
