using AptManagement.Application.Dtos;
using AptManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AptManagement.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BankController : ControllerBase
{
    private readonly IBankStatementService _bankService;

    public BankController(IBankStatementService bankService)
    {
        _bankService = bankService;
    }

    // 1. ADIM: Excel Yükle ve Önizleme Al
    [HttpPost("upload")]
    public IActionResult Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Dosya seçilmedi.");

        // Uzantı kontrolü
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (ext != ".xlsx")
            return BadRequest("Lütfen .xlsx formatında (Excel) dosyası yükleyiniz. Ziraat CSV veriyorsa Excel olarak kaydedip yükleyin.");

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var transactions = _bankService.ParseExcelFile(stream);
                return Ok(transactions);
            }
        }
        catch (Exception ex)
        {
            return BadRequest($"Dosya okunurken hata: {ex.Message}");
        }
    }

    // 2. ADIM: Onaylanan Listeyi Kaydet
    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] List<BankTransactionDto> transactions)
    {
        if (transactions == null || !transactions.Any())
            return BadRequest("Kaydedilecek işlem bulunamadı.");

        // Sadece işlenmemiş ve onaylanmış kayıtları gönderelim
        var result = await _bankService.ProcessBankStatementAsync(transactions);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result.Message);
    }
}