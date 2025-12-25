using AptManagement.Application.Interfaces;
using AptManagement.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AptManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController(IReportService reportService) : ControllerBase
    {
        [HttpGet("summary-cards")]

        public IActionResult GetSummaryCards()
        {
            var result = reportService.GetGeneralStatusCardsAsync();
            return Ok(result);
        }

        [HttpGet("monthly-trend")]
        public async Task<IActionResult> GetMonthlyTrend()
        {
            var result = await reportService.GetMonthlyTrendAsync();
            return Ok(result);
        }

        [HttpGet("expense-distribution")]
        public IActionResult GetExpenseDistribution()
        {
            var result =  reportService.GetExpenseDistributionAsync();
            return Ok(result);
        }
    }
}
