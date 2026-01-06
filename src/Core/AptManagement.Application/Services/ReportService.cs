using AptManagement.Application.Common;
using AptManagement.Application.Dtos.Reports;
using AptManagement.Application.Interfaces;
using AptManagement.Domain.Entities;
using AptManagement.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AptManagement.Application.Services
{
    public class ReportService(IRepository<Income> incomeRepo, IRepository<Expense> expenseRepo, IRepository<ApartmentDebt> debtRepo) : IReportService
    {
        public ServiceResult<List<ExpenseDistributionDto>> GetExpenseDistributionAsync()
        {
            // 1. Toplam gideri bulalım.
            var totalExpense = expenseRepo.GetAll().Sum(x => x.Amount);

            if (totalExpense == 0) return ServiceResult<List<ExpenseDistributionDto>>.Error("Gider bulunamadı");

            // 2. Kategori bazlı gruplayalım ve toplayalım

            var distribution = expenseRepo.GetAll()
                .GroupBy(x => x.ExpenseCategory.Name)
                .Select(g => new ExpenseDistributionDto
                {
                    CategoryName = g.Key,
                    TotalAmount = g.Sum(x => x.Amount),
                    Percentage = Math.Round((double)((g.Sum(x => x.Amount) / totalExpense) * 100), 2)
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToList();

            return ServiceResult<List<ExpenseDistributionDto>>.Success(distribution);
        }

        public ServiceResult<DashboardSummaryDto> GetGeneralStatusCardsAsync()
        {
            var now = DateTime.Now;            

            var totalIncome = incomeRepo.GetAll().Sum(x => x.Amount);
            var totalExpense = expenseRepo.GetAll().Sum(x => x.Amount);

            // 2. Alacak Sorguları (Kapanmamış borçlar)
            var unpaidDebtsQuery = debtRepo.GetAll().Where(x => !x.IsClosed && !x.Apartment.IsManager);

            var expectedIncome = unpaidDebtsQuery.Sum(x => x.Amount - x.PaidAmount); //Beklenen Gelir (Dairenin borcundan , yaptıgı ödemeleri düştük)

            var overdueAmount = unpaidDebtsQuery.Where(x => x.DueDate < now).Sum(x => x.Amount - x.PaidAmount); // vadesi geçmiş alacaklar

            var activeDebtorsCount = unpaidDebtsQuery.Select(x => x.ApartmentId).Distinct().Count();  // Borçlu daire sayısı

            var summaryDto = new DashboardSummaryDto
            {
                ActiveDebtorsCount = activeDebtorsCount,
                ExpectedIncome = expectedIncome,
                TotalExpense = totalExpense,
                TotalIncome = totalIncome,
            };

            return ServiceResult<DashboardSummaryDto>.Success(summaryDto);
        }

        public async Task<ServiceResult<List<MonthlyTrendDto>>> GetMonthlyTrendAsync()
        {
            var startDate = DateTime.Now.AddMonths(-11).Date;
            startDate = new DateTime(startDate.Year, startDate.Month, 1); // 12 ay öncesinin başına git

            // 1. Son 12 ayın Gelirlerini gruplayarak çek
            var incomes = incomeRepo.GetAll()
                .Where(x => x.IncomeDate >= startDate)
                .GroupBy(x => new { x.IncomeDate.Year, x.IncomeDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(i => i.Amount)
                })
                .ToList();

            // 2. Son 12 ayın Giderlerini gruplayarak çek
            var expenses = expenseRepo.GetAll()
                .Where(x => x.ExpenseDate >= startDate)
                .GroupBy(x => new { x.ExpenseDate.Year, x.ExpenseDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(e => e.Amount)
                })
                .ToList();

            // 3. Hafızada son 12 ayı oluştur ve verileri eşleştir
            var result = new List<MonthlyTrendDto>();

            for (int i = 0; i < 12; i++)
            {
                var currentMonthDate = startDate.AddMonths(i);
                var monthName = currentMonthDate.ToString("MMMM yyyy"); // Örn: "Aralık 2025"

                var monthIncome = incomes.FirstOrDefault(x => x.Year == currentMonthDate.Year && x.Month == currentMonthDate.Month)?.Total ?? 0;
                var monthExpense = expenses.FirstOrDefault(x => x.Year == currentMonthDate.Year && x.Month == currentMonthDate.Month)?.Total ?? 0;

                result.Add(new MonthlyTrendDto
                {
                    MonthName = monthName,
                    TotalIncome = monthIncome,
                    TotalExpense = monthExpense
                });
            }

            return ServiceResult<List<MonthlyTrendDto>>.Success(result);

        }
    }
}
