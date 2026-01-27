using AptManagement.Application.Common.Base.Request;
using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace AptManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApartmentController(IApartmentService apartmentService) : ControllerBase
    {
        [HttpGet("get")]
        public async Task<IActionResult> GetById([FromQuery]int id)
        {
            var entity = await apartmentService.GetApartmentById(id);
            if(entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery]ApartmentSearch request)
        {
            var entity = await apartmentService.Search(request);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrEdit([FromBody]ApartmentDto dto)
        {
            var entity = await apartmentService.CreateOrEdit(dto);
            if (entity == null) return NotFound();
            return Ok(entity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await apartmentService.DeleteApartmentAsync(id);
            if (!entity) return NotFound();
            return Ok(entity);
        }

        [HttpPost("excel-create")]
        public async Task<IActionResult> CreateApartmentWithExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Lütfen geçerli bir Excel dosyası yükleyin.");

            // Uzantı kontrolü
            var ext = Path.GetExtension(file.FileName).ToLower();
            if (ext != ".xlsx" && ext != ".xls")
                return BadRequest("Lütfen .xlsx veya .xls formatında (Excel) dosyası yükleyiniz.");

            var fileContents = await apartmentService.ParseApartmentExcelAsync(file);
            if (fileContents == null || !fileContents.IsSuccess) 
                return BadRequest(fileContents?.Message ?? "Dosya işlenirken bir hata oluştu.");
            
            return Ok(fileContents);
        }
    }
}
